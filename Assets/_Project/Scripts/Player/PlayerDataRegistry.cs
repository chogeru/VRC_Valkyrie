using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// This VRChat SDK version has no automatic "Player Object" feature (an
// UdonSharpBehaviour prefab VRChat auto-instantiates once per player), so
// per-player data (see PlayerHealthManager) is handled with a manually
// managed pool instead: pre-place enough PlayerHealthManager instances in
// the scene to cover your world's player cap, wire them all into "pool"
// here, and this script claims one for each joining player and looks them
// up for other scripts (Gun.cs, ZombieAI.cs, WeaponUpgradeStation.cs).
public class PlayerDataRegistry : UdonSharpBehaviour
{
    [Tooltip("Pre-placed pool, one entry per potential concurrent player. Size to match your world's max capacity (VRChat instances cap at 80, but most worlds use far fewer).")]
    public PlayerHealthManager[] pool;

    private int pendingReleasePlayerId = -1;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal) return; // only the joining player's own client claims a slot

        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] == null) continue;
            if (pool[i].GetOwnerPlayerId() == -1)
            {
                pool[i].ClaimForLocalPlayer();
                return;
            }
        }

        Debug.LogWarning("[PlayerDataRegistry] No free PlayerHealthManager slot for " + player.displayName + " - increase the pool size in the Inspector.");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null) return;
        // VRChat needs a moment to transfer ownership of the departed
        // player's objects to the master before this client can legally
        // reset the slot's synced fields - give that a beat to settle.
        pendingReleasePlayerId = player.playerId;
        SendCustomEventDelayedSeconds(nameof(ProcessPendingRelease), 1f);
    }

    public void ProcessPendingRelease()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && pool[i].GetOwnerPlayerId() == pendingReleasePlayerId)
            {
                pool[i].ReleaseSlot();
            }
        }
    }

    public PlayerHealthManager GetPlayerHealthManager(VRCPlayerApi player)
    {
        if (player == null) return null;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && pool[i].GetOwnerPlayerId() == player.playerId) return pool[i];
        }
        return null;
    }

    // Used by GameManager to detect a full team wipe (every claimed slot's
    // health at 0) while a game is in progress, to trigger Game Over.
    public int CountClaimedSlots()
    {
        int count = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && pool[i].GetOwnerPlayerId() != -1) count++;
        }
        return count;
    }

    public int CountAliveClaimedSlots()
    {
        int count = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && pool[i].GetOwnerPlayerId() != -1 && pool[i].GetHealth() > 0f) count++;
        }
        return count;
    }
}
