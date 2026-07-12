using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// One entry in PlayerDataRegistry.pool - a pre-placed (NOT instantiated at
// runtime) slot that gets claimed by a joining player's own client via
// ClaimForLocalPlayer(). This SDK version has no automatic "Player Object"
// feature, so per-player data has to be handled with a manually-claimed
// pool instead. Doubles as the player's wallet: score earned from zombie
// kills is spent at a WeaponUpgradeStation to tier up whichever gun is
// held. Damage/score are applied by whoever triggered them (they briefly
// take ownership to legally write the synced fields), while respawn/
// teleport/claim/release are always executed by the owning client.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerHealthManager : UdonSharpBehaviour
{
    [Header("References")]
    public GameSettings settings;
    public HudController hud;

    [UdonSynced] private float syncedHealth = -1f;
    [UdonSynced] private int syncedScore;
    [UdonSynced] private int ownerPlayerId = -1;

    private bool isRespawning;

    // Called by PlayerDataRegistry.OnPlayerJoined on the joining player's
    // own client, once it finds this as the first free slot.
    public void ClaimForLocalPlayer()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        ownerPlayerId = Networking.LocalPlayer != null ? Networking.LocalPlayer.playerId : -1;
        syncedHealth = settings != null ? settings.playerMaxHealth : 100f;
        syncedScore = 0;
        isRespawning = false;
        RequestSerialization();
        RefreshLocalHud();
    }

    // Called by PlayerDataRegistry.ProcessPendingRelease, only actually
    // acts if this client currently owns the slot (VRChat auto-transfers
    // ownership of a departed player's objects to the master).
    public void ReleaseSlot()
    {
        if (!Networking.IsOwner(gameObject)) return;
        ownerPlayerId = -1;
        syncedHealth = -1f;
        syncedScore = 0;
        RequestSerialization();
    }

    // Called directly (local call) by whatever hit this player.
    public void ApplyDamage(float amount)
    {
        if (syncedHealth <= 0f) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncedHealth -= amount;
        RequestSerialization();
        RefreshLocalHud();
        CheckLocalDeath();
    }

    // Called directly (local call) by whoever scored the kill - briefly
    // takes ownership so the score change is allowed to replicate.
    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncedScore += amount;
        RequestSerialization();
        RefreshLocalHud();
    }

    // Only meaningful when called by the owning player's own client (e.g.
    // from a shop interaction they triggered themselves).
    public bool TrySpendScore(int amount)
    {
        if (amount <= 0) return true;
        if (syncedScore < amount) return false;

        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        syncedScore -= amount;
        RequestSerialization();
        RefreshLocalHud();
        return true;
    }

    public override void OnDeserialization()
    {
        RefreshLocalHud();
        CheckLocalDeath();
    }

    private void CheckLocalDeath()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local != null && local.playerId == ownerPlayerId && syncedHealth <= 0f && !isRespawning)
        {
            isRespawning = true;
            SendCustomEventDelayedSeconds(nameof(RespawnLocalPlayer), settings != null ? settings.respawnDelay : 5f);
        }
    }

    public void RespawnLocalPlayer()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncedHealth = settings != null ? settings.playerMaxHealth : 100f;
        RequestSerialization();
        RefreshLocalHud();
        isRespawning = false;

        VRCPlayerApi local = Networking.LocalPlayer;
        if (local != null && settings != null && settings.playerRespawnPoints.Length > 0)
        {
            Transform rp = settings.playerRespawnPoints[Random.Range(0, settings.playerRespawnPoints.Length)];
            local.TeleportTo(rp.position, rp.rotation);
        }
    }

    private void RefreshLocalHud()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (hud == null || local == null || local.playerId != ownerPlayerId) return;

        hud.OnLocalHealthChanged(syncedHealth, settings != null ? settings.playerMaxHealth : 100f);
        hud.OnLocalScoreChanged(syncedScore);
    }

    public float GetHealth() { return syncedHealth; }
    public int GetScore() { return syncedScore; }
    public int GetOwnerPlayerId() { return ownerPlayerId; }
}
