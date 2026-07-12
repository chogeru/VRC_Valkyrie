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

    // Script-driven fallback animation for models that only ship a
    // locomotion clip (e.g. NewPunch's ShirtlessZombieFree, which has no
    // attack/death clips). Purely local visual tweens - no networking
    // needed beyond the existing synced "dead" flag that triggers them.
    [Header("Procedural Attack/Death Animation (optional)")]
    [Tooltip("Child visual root to lunge on attack, so the NavMeshAgent-driven root transform isn't fought while alive. Leave empty to skip the attack lunge.")]
    public Transform visualRoot;
    public float attackLungeDistance = 0.3f;
    public float attackLungeOutDuration = 0.12f;
    public float attackLungeBackDuration = 0.18f;
    public float deathCollapseDuration = 1.2f;
    public Vector3 deathCollapseLocalRotationEuler = new Vector3(80f, 0f, 0f);
    public float deathSinkDistance = 0.3f;

    [UdonSynced] private bool syncedActive;
    [UdonSynced] private bool syncedDead;
    [UdonSynced] private float syncedHealth;
    [UdonSynced] private Vector3 syncedSpawnPos;
    [UdonSynced] private Quaternion syncedSpawnRot;

    private float currentSpeedMultiplier = 1f;
    private VRCPlayerApi targetPlayer;
    private float nextRetargetTime;
    private float nextAttackTime;

    private bool lungeAnimating;
    private bool lungeGoingOut;
    private float lungeAnimStartTime;
    private Vector3 lungeRestLocalPos;

    private bool deathAnimating;
    private float deathAnimStartTime;
    private Vector3 deathStartLocalPos;
    private Quaternion deathStartLocalRot;

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
            if (agent != null) agent.enabled = false; // stop fighting the collapse tween below
            if (animator != null) animator.SetTrigger("Die");
            // Runs on every client (driven by the synced flag), so everyone
            // in earshot hears the death sound and sees the collapse, not
            // just whoever landed the kill.
            PlayRandomClip(config.deathClips);
            TriggerDeathCollapse();
            return;
        }

        transform.localRotation = Quaternion.identity;
        transform.SetPositionAndRotation(syncedSpawnPos, syncedSpawnRot);
        if (agent != null)
        {
            agent.enabled = true; // in case this pool entry died on its previous life
            agent.Warp(syncedSpawnPos);
            agent.speed = config.moveSpeed * Mathf.Max(0.01f, currentSpeedMultiplier);
        }
        if (hitCollider != null) hitCollider.enabled = true;
        deathAnimating = false;
        lungeAnimating = false;
    }

    void Update()
    {
        // Purely local visual tweens - keep ticking even while dead/not-owned
        // so every client sees the same collapse/lunge motion.
        UpdateLungeAnim();
        UpdateDeathCollapseAnim();

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
        TriggerAttackLunge();

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

    // --- Procedural attack lunge (local-only visual feedback) ---------

    private void TriggerAttackLunge()
    {
        if (visualRoot == null) return; // would fight the NavMeshAgent-driven root while alive
        lungeRestLocalPos = visualRoot.localPosition;
        lungeAnimating = true;
        lungeGoingOut = true;
        lungeAnimStartTime = Time.time;
    }

    private void UpdateLungeAnim()
    {
        if (!lungeAnimating || visualRoot == null) return;

        float duration = lungeGoingOut ? attackLungeOutDuration : attackLungeBackDuration;
        float t = Mathf.Clamp01((Time.time - lungeAnimStartTime) / Mathf.Max(0.001f, duration));
        Vector3 outPos = lungeRestLocalPos + Vector3.forward * attackLungeDistance;
        Vector3 from = lungeGoingOut ? lungeRestLocalPos : outPos;
        Vector3 to = lungeGoingOut ? outPos : lungeRestLocalPos;
        visualRoot.localPosition = Vector3.Lerp(from, to, t);

        if (t >= 1f)
        {
            if (lungeGoingOut)
            {
                lungeGoingOut = false;
                lungeAnimStartTime = Time.time;
            }
            else
            {
                lungeAnimating = false;
                visualRoot.localPosition = lungeRestLocalPos;
            }
        }
    }

    // --- Procedural death collapse (broadcast via the synced dead flag) -

    private void TriggerDeathCollapse()
    {
        deathAnimating = true;
        deathAnimStartTime = Time.time;
        deathStartLocalPos = transform.localPosition;
        deathStartLocalRot = transform.localRotation;
    }

    private void UpdateDeathCollapseAnim()
    {
        if (!deathAnimating) return;

        float t = Mathf.Clamp01((Time.time - deathAnimStartTime) / Mathf.Max(0.001f, deathCollapseDuration));
        Quaternion targetRot = deathStartLocalRot * Quaternion.Euler(deathCollapseLocalRotationEuler);
        transform.localRotation = Quaternion.Slerp(deathStartLocalRot, targetRot, t);

        Vector3 targetPos = deathStartLocalPos - new Vector3(0f, deathSinkDistance, 0f);
        transform.localPosition = Vector3.Lerp(deathStartLocalPos, targetPos, t);

        if (t >= 1f) deathAnimating = false;
    }
}
