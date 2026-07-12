using UdonSharp;
using UnityEngine;

// Data container for a single weapon type. Duplicate the WeaponConfig
// GameObject/prefab (e.g. Pistol, Rifle, Shotgun), tune these fields in the
// Inspector, then assign it to a Gun's "config" field. Gun.cs never changes.
public class WeaponConfig : UdonSharpBehaviour
{
    [Header("Identity")]
    public string weaponName = "Pistol";

    [Header("Damage")]
    public float damagePerHit = 20f;
    public float headshotMultiplier = 2f;

    [Header("Fire Behaviour")]
    public bool isAutomatic = false;
    public float fireRate = 4f; // shots per second
    public float range = 60f;
    public float spreadDegrees = 1.5f;

    [Header("Ammo")]
    public int magazineSize = 12;
    public float reloadTime = 1.6f;
    public int reserveAmmoMax = 96;

    [Header("Upgrade Shop (3 tiers, paid with score)")]
    [Tooltip("Score cost to purchase tier 1/2/3 at the upgrade shop. Should increase each tier.")]
    public int[] tierUpgradeCost = new int[] { 50, 120, 250 };
    [Tooltip("Multiplies damagePerHit once a tier is reached.")]
    public float[] tierDamageMultiplier = new float[] { 1.15f, 1.35f, 1.6f };
    [Tooltip("Multiplies fireRate (higher = faster shooting).")]
    public float[] tierFireRateMultiplier = new float[] { 1.1f, 1.25f, 1.45f };
    [Tooltip("Multiplies magazineSize (higher = more rounds per mag).")]
    public float[] tierMagazineSizeMultiplier = new float[] { 1.25f, 1.5f, 2f };
    [Tooltip("Multiplies reloadTime (lower = faster reload).")]
    public float[] tierReloadTimeMultiplier = new float[] { 0.9f, 0.8f, 0.65f };
}
