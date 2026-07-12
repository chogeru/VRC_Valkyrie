using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Master-authoritative wave spawner. Draws zombies from a pre-placed pool
// (see ZombieAI) according to GameSettings.waves, and mirrors the "zombies
// remaining" counter to every client for the HUD.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WaveManager : UdonSharpBehaviour
{
    [Header("References")]
    public GameSettings settings;
    public GameManager gameManager;
    public HudController hud;
    public AudioManager audioManager;
    [Tooltip("Pre-placed, inactive zombie instances. Size this generously - it caps how many zombies can be alive at once.")]
    public ZombieAI[] zombiePool;

    [UdonSynced] private int syncedWaveIndex = -1;
    [UdonSynced] private int syncedZombiesRemaining;
    [UdonSynced] private int syncedLoopBonusCount;

    private int spawnedThisWave;
    private WaveConfig currentWave;
    private int lastAppliedWaveIndex = -2;
    private bool stopped;

    public void BeginWaves()
    {
        if (!Networking.IsOwner(gameObject)) return;
        stopped = false;
        syncedWaveIndex = -1;
        syncedLoopBonusCount = 0;
        StartNextWave();
    }

    // Called by GameManager.TriggerGameOver() so a team wipe doesn't leave
    // this quietly spawning zombies (and possibly reaching Victory) into an
    // empty battlefield after everyone's been sent back to the lobby.
    public void StopSpawning()
    {
        stopped = true;
    }

    public void StartNextWave()
    {
        if (!Networking.IsOwner(gameObject) || stopped) return;

        syncedWaveIndex++;
        if (syncedWaveIndex >= settings.waves.Length)
        {
            if (settings.loopFinalWave && settings.waves.Length > 0)
            {
                syncedWaveIndex = settings.waves.Length - 1;
                syncedLoopBonusCount++;
            }
            else
            {
                RequestSerialization();
                if (gameManager != null) gameManager.NotifyVictory();
                return;
            }
        }

        currentWave = settings.waves[syncedWaveIndex];
        syncedZombiesRemaining = currentWave.zombieCount;
        spawnedThisWave = 0;
        RequestSerialization();
        ApplyWaveStartedLocal();
        SendCustomEventDelayedSeconds(nameof(SpawnNextZombie), currentWave.firstSpawnDelay);
    }

    public void SpawnNextZombie()
    {
        if (!Networking.IsOwner(gameObject) || stopped) return;
        if (currentWave == null || spawnedThisWave >= currentWave.zombieCount) return;

        ZombieAI z = GetPooledZombie();
        if (z != null && settings.zombieSpawnPoints.Length > 0)
        {
            Transform sp = settings.zombieSpawnPoints[Random.Range(0, settings.zombieSpawnPoints.Length)];
            float healthMul = currentWave.healthMultiplier + (syncedLoopBonusCount * settings.finalWaveLoopHealthStep);
            z.Activate(sp.position, sp.rotation, healthMul, currentWave.moveSpeedMultiplier);
        }
        else
        {
            // Pool exhausted (or no spawn points configured) - this slot will
            // never spawn a zombie, so it can never send a death event either.
            // Without this, syncedZombiesRemaining would get stuck above 0
            // forever and the wave (and the whole game) could never advance.
            HandleUnspawnableSlot();
        }

        spawnedThisWave++;
        if (spawnedThisWave < currentWave.zombieCount)
        {
            SendCustomEventDelayedSeconds(nameof(SpawnNextZombie), currentWave.spawnInterval);
        }
    }

    private void HandleUnspawnableSlot()
    {
        syncedZombiesRemaining = Mathf.Max(0, syncedZombiesRemaining - 1);
        RequestSerialization();
        if (hud != null) hud.OnZombiesRemainingChanged(syncedZombiesRemaining);

        if (syncedZombiesRemaining <= 0 && spawnedThisWave + 1 >= currentWave.zombieCount)
        {
            SendCustomEventDelayedSeconds(nameof(StartNextWave), currentWave.intermissionAfterWave);
        }
    }

    private ZombieAI GetPooledZombie()
    {
        for (int i = 0; i < zombiePool.Length; i++)
        {
            if (zombiePool[i] != null && !zombiePool[i].IsInUse())
            {
                return zombiePool[i];
            }
        }
        Debug.LogWarning("[WaveManager] Zombie pool exhausted - increase pool size in the Inspector.");
        return null;
    }

    // Called by ZombieAI via SendCustomNetworkEvent(All) when it dies, so
    // every client's remaining-count and HUD stay in sync.
    public void NotifyZombieDied()
    {
        syncedZombiesRemaining = Mathf.Max(0, syncedZombiesRemaining - 1);
        if (hud != null) hud.OnZombiesRemainingChanged(syncedZombiesRemaining);

        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization();
            if (syncedZombiesRemaining <= 0 && currentWave != null && spawnedThisWave >= currentWave.zombieCount)
            {
                SendCustomEventDelayedSeconds(nameof(StartNextWave), currentWave.intermissionAfterWave);
            }
        }
    }

    public override void OnDeserialization()
    {
        if (hud != null) hud.OnZombiesRemainingChanged(syncedZombiesRemaining);

        if (syncedWaveIndex != lastAppliedWaveIndex)
        {
            ApplyWaveStartedLocal();
        }
    }

    // Runs on every client (owner calls it directly for instant feedback;
    // everyone else reaches it via OnDeserialization once syncedWaveIndex
    // changes) - looks the wave up from the shared GameSettings array
    // instead of relying on the owner-only local "currentWave" cache.
    private void ApplyWaveStartedLocal()
    {
        lastAppliedWaveIndex = syncedWaveIndex;
        WaveConfig wave = GetWaveConfigAt(syncedWaveIndex);
        if (hud != null) hud.OnWaveStarted(syncedWaveIndex, wave, syncedLoopBonusCount);
        if (audioManager != null) audioManager.PlaySfx("WaveStart");
    }

    private WaveConfig GetWaveConfigAt(int index)
    {
        if (settings == null || settings.waves == null) return null;
        if (index < 0 || index >= settings.waves.Length) return null;
        return settings.waves[index];
    }
}
