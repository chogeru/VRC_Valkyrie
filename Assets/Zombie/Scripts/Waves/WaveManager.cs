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
    [Tooltip("Pre-placed, inactive zombie instances. Size this generously - it caps how many zombies can be alive at once.")]
    public ZombieAI[] zombiePool;

    [UdonSynced] private int syncedWaveIndex = -1;
    [UdonSynced] private int syncedZombiesRemaining;

    private int spawnedThisWave;
    private WaveConfig currentWave;
    private int loopBonusCount;

    public void BeginWaves()
    {
        if (!Networking.IsOwner(gameObject)) return;
        syncedWaveIndex = -1;
        loopBonusCount = 0;
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (!Networking.IsOwner(gameObject)) return;

        syncedWaveIndex++;
        if (syncedWaveIndex >= settings.waves.Length)
        {
            if (settings.loopFinalWave && settings.waves.Length > 0)
            {
                syncedWaveIndex = settings.waves.Length - 1;
                loopBonusCount++;
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
        if (hud != null) hud.OnWaveStarted(syncedWaveIndex, currentWave, loopBonusCount);
        SendCustomEventDelayedSeconds(nameof(SpawnNextZombie), 0.5f);
    }

    public void SpawnNextZombie()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (currentWave == null || spawnedThisWave >= currentWave.zombieCount) return;

        ZombieAI z = GetPooledZombie();
        if (z != null && settings.zombieSpawnPoints.Length > 0)
        {
            Transform sp = settings.zombieSpawnPoints[Random.Range(0, settings.zombieSpawnPoints.Length)];
            float healthMul = currentWave.healthMultiplier + (loopBonusCount * settings.finalWaveLoopHealthStep);
            z.Activate(sp.position, sp.rotation, healthMul, currentWave.moveSpeedMultiplier);
        }

        spawnedThisWave++;
        if (spawnedThisWave < currentWave.zombieCount)
        {
            SendCustomEventDelayedSeconds(nameof(SpawnNextZombie), currentWave.spawnInterval);
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
    }
}
