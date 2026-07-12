using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Place on the "改造" (upgrade) button/terminal GameObject in the lobby or
// battle area. Needs a Collider so it can be Interact()-ed. When a player
// presses it, whichever Gun they're currently holding in either hand gets
// offered a purchase of its next upgrade tier, paid from that player's
// score (see PlayerHealthManager.TrySpendScore / Gun.TryUpgrade).
public class WeaponUpgradeStation : UdonSharpBehaviour
{
    [Header("References")]
    public HudController hud;

    public override void Interact()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local == null) return;

        Gun gun = FindHeldGun(local);
        if (gun == null)
        {
            if (hud != null) hud.ShowShopMessage("武器を手に持ってから改造してください");
            return;
        }

        gun.TryUpgrade();
    }

    private Gun FindHeldGun(VRCPlayerApi player)
    {
        VRC_Pickup right = player.GetPickupInHand(VRC_Pickup.PickupHand.Right);
        if (right != null)
        {
            Gun gun = (Gun)right.GetComponent(typeof(Gun));
            if (gun != null) return gun;
        }

        VRC_Pickup left = player.GetPickupInHand(VRC_Pickup.PickupHand.Left);
        if (left != null)
        {
            Gun gun = (Gun)left.GetComponent(typeof(Gun));
            if (gun != null) return gun;
        }

        return null;
    }
}
