using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Register this prefab's root GameObject in the VRC Scene Descriptor's
// "Player Objects" list so VRChat auto-spawns exactly one instance per
// player. Damage is applied by whoever hit this player (they briefly take
// ownership to legally write the synced health), while respawn/teleport is
// always executed by the owning player's own client.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerHealthManager : UdonSharpBehaviour
{
    [Header("References")]
    public GameSettings settings;
    public HudController hud;

    [UdonSynced] private float syncedHealth = -1f;
    [UdonSynced] private int ownerPlayerId = -1;

    private bool isRespawning;

    void Start()
    {
        if (Networking.IsOwner(gameObject))
        {
            ownerPlayerId = Networking.LocalPlayer != null ? Networking.LocalPlayer.playerId : -1;
            syncedHealth = settings != null ? settings.playerMaxHealth : 100f;
            RequestSerialization();
        }
        RefreshLocalHud();
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
        if (hud != null && local != null && local.playerId == ownerPlayerId)
        {
            hud.OnLocalHealthChanged(syncedHealth, settings != null ? settings.playerMaxHealth : 100f);
        }
    }

    public float GetHealth() { return syncedHealth; }
    public int GetOwnerPlayerId() { return ownerPlayerId; }
}
