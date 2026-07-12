using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Attach to every pooled zombie instance. Pool entries live inactive in the
// scene and get repositioned/re-armed by WaveManager instead of being
// Instantiated at runtime (pre-placed pooled objects are the recommended
// pattern for networked VRChat worlds). Add a "VRC Object Sync" (or set
// Continuous transform sync) on this GameObject so remote clients see the
// owner's movement smoothly - this script only drives the owner's copy.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ZombieAI : UdonSharpBehaviour
{
    [Header("Config")]
    public ZombieConfig config;

    [Header("References")]
    public GameSettings settings;
    public WaveManager waveManager;
    public NavMeshAgent agent;
    public Animator animator;
    public Collider hitCollider;
    [Tooltip("3D AudioSource on this zombie used for ZombieConfig's attack/damage/death/idle clips.")]
    public AudioSource voiceAudioSource;

    [UdonSynced] private bool syncedActive;
    [UdonSynced] private bool syncedDead;
    [UdonSynced] private float syncedHealth;
    [UdonSynced] private Vector3 syncedSpawnPos;
    [UdonSynced] private Quaternion syncedSpawnRot;

    private float currentSpeedMultiplier = 1f;
    private VRCPlayerApi targetPlayer;
    private float nextRetargetTime;
    private float nextAttackTime;

    void Start()
    {
        if (hitCollider == null) hitCollider = GetComponent<Collider>();
        gameObject.SetActive(syncedActive);
    }

    public bool IsInUse()
    {
        return syncedActive;
    }

    // Called directly (local method call, not a network event) by whichever
    // client currently owns WaveManager (the master) to arm a pooled zombie.
    public void Activate(Vector3 pos, Quaternion rot, float healthMultiplier, float speedMultiplier)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncedSpawnPos = pos;
        syncedSpawnRot = rot;
        syncedHealth = config.maxHealth * Mathf.Max(0.01f, healthMultiplier);
        syncedDead = false;
        syncedActive = true;
        currentSpeedMultiplier = speedMultiplier;
        RequestSerialization();
        ApplyActivationLocal();
    }

    public override void OnDeserialization()
    {
        ApplyActivationLocal();
    }

    private void ApplyActivationLocal()
    {
        gameObject.SetActive(syncedActive);
        if (!syncedActive) return;

        if (syncedDead)
        {
            if (hitCollider != null) hitCollider.enabled = false;
            if (animator != null) animator.SetTrigger("Die");
            // Runs on every client (driven by the synced flag), so everyone
            // in earshot hears the death sound, not just whoever landed the kill.
            PlayRandomClip(config.deathClips);
            return;
        }

        transform.SetPositionAndRotation(syncedSpawnPos, syncedSpawnRot);
        if (agent != null)
        {
            agent.Warp(syncedSpawnPos);
            agent.speed = config.moveSpeed * Mathf.Max(0.01f, currentSpeedMultiplier);
        }
        if (hitCollider != null) hitCollider.enabled = true;
    }

    void Update()
    {
        if (!syncedActive || syncedDead) return;
        if (!Networking.IsOwner(gameObject)) return; // remote clients just see the synced transform
        RunAi();
    }

    private void RunAi()
    {
        if (Time.time >= nextRetargetTime)
        {
            nextRetargetTime = Time.time + 1f;
            targetPlayer = FindNearestPlayer();
            if (Random.value < config.idleClipChancePerRetarget) PlayRandomClip(config.idleClips);
        }
        if (targetPlayer == null || !targetPlayer.IsValid()) return;

        Vector3 targetPos = targetPlayer.GetPosition();
        if (agent != null) agent.SetDestination(targetPos);

        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist <= config.attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + config.attackCooldown;
            AttackPlayer(targetPlayer);
        }
    }

    private VRCPlayerApi FindNearestPlayer()
    {
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        VRCPlayerApi nearest = null;
        float best = float.MaxValue;
        foreach (VRCPlayerApi p in players)
        {
            if (p == null || !p.IsValid()) continue;
            float d = Vector3.Distance(transform.position, p.GetPosition());
            if (d < best)
            {
                best = d;
                nearest = p;
            }
        }
        return nearest;
    }

    private void AttackPlayer(VRCPlayerApi player)
    {
        PlayRandomClip(config.attackClips);

        if (settings == null || settings.playerHealthObjectPrefab == null) return;
        GameObject obj = player.GetPlayerObject(settings.playerHealthObjectPrefab);
        if (obj == null) return;
        PlayerHealthManager health = (PlayerHealthManager)obj.GetComponent(typeof(PlayerHealthManager));
        if (health != null) health.ApplyDamage(config.attackDamage);
    }

    // Note: this plays locally for whoever currently owns the zombie
    // (typically the shooter or master), not broadcast to every nearby
    // player - acceptable for this ambient flavor sound. Death sound (see
    // ApplyActivationLocal) IS broadcast to everyone since it's driven by
    // the synced "dead" flag.
    private void PlayRandomClip(AudioClip[] clips)
    {
        if (voiceAudioSource == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null) voiceAudioSource.PlayOneShot(clip);
    }

    // Called locally by whichever client's shot hit this zombie (see Gun.cs).
    // Returns true if this hit was the killing blow, so the shooter's Gun
    // can credit itself a kill for its tier-up progression.
    public bool TakeDamage(float amount)
    {
        if (!syncedActive || syncedDead) return false;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncedHealth -= amount;
        bool killedByThisHit = false;
        if (syncedHealth <= 0f)
        {
            killedByThisHit = true;
            Die();
        }
        else
        {
            PlayRandomClip(config.damageClips);
        }
        RequestSerialization();
        return killedByThisHit;
    }

    private void Die()
    {
        syncedDead = true;
        if (hitCollider != null) hitCollider.enabled = false;
        if (animator != null) animator.SetTrigger("Die");
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NotifyWaveManagerOfDeath));
        SendCustomEventDelayedSeconds(nameof(Deactivate), 3f);
    }

    // Broadcast so every client's WaveManager mirror decrements exactly once.
    public void NotifyWaveManagerOfDeath()
    {
        if (waveManager != null) waveManager.NotifyZombieDied();
    }

    public void Deactivate()
    {
        if (!Networking.IsOwner(gameObject)) return;
        syncedActive = false;
        RequestSerialization();
        ApplyActivationLocal();
    }
}
