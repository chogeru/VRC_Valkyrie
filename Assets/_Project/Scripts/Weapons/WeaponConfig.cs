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
}
