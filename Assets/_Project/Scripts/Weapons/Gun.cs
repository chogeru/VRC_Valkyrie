using UdonSharp;
using UnityEngine;

// Attach to a weapon GameObject alongside a VRC Pickup component. Swap the
// "config" reference to reuse this exact script for every weapon type
// (pistol / rifle / shotgun / ...) - only the data changes, never the code.
public class Gun : UdonSharpBehaviour
{
    [Header("Config")]
    public WeaponConfig config;

    [Header("References")]
    public GameSettings settings;
    public Transform muzzle;
    public ParticleSystem muzzleFlash;
    public AudioSource fireSound;
    public AudioSource reloadSound;
    public AudioSource emptySound;
    public LayerMask hitMask = ~0;

    [Header("Runtime (read-only, for debugging)")]
    public int currentAmmo;
    public int reserveAmmo;

    private bool isReloading;
    private bool triggerHeld;
    private float nextFireTime;

    void Start()
    {
        currentAmmo = config.magazineSize;
        reserveAmmo = config.reserveAmmoMax;
    }

    public override void OnPickupUseDown()
    {
        triggerHeld = true;
        TryFire();
    }

    public override void OnPickupUseUp()
    {
        triggerHeld = false;
    }

    public override void OnDrop()
    {
        triggerHeld = false;
    }

    private void TryFire()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            if (emptySound != null) emptySound.Play();
            StartReload();
            return;
        }

        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + (1f / Mathf.Max(0.01f, config.fireRate));

        currentAmmo--;
        FireShot();

        if (config.isAutomatic && triggerHeld)
        {
            SendCustomEventDelayedSeconds(nameof(AutoFireTick), 1f / Mathf.Max(0.01f, config.fireRate));
        }
    }

    public void AutoFireTick()
    {
        if (triggerHeld) TryFire();
    }

    private void FireShot()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        if (fireSound != null) fireSound.Play();

        Vector3 origin = muzzle != null ? muzzle.position : transform.position;
        Vector3 dir = muzzle != null ? muzzle.forward : transform.forward;

        dir = Quaternion.Euler(
            Random.Range(-config.spreadDegrees, config.spreadDegrees),
            Random.Range(-config.spreadDegrees, config.spreadDegrees),
            0f) * dir;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit, config.range, hitMask))
        {
            ZombieAI zombie = (ZombieAI)hit.collider.GetComponentInParent(typeof(ZombieAI));
            if (zombie != null)
            {
                float dmg = config.damagePerHit;
                ZombieHeadHitbox headHitbox = (ZombieHeadHitbox)hit.collider.GetComponent(typeof(ZombieHeadHitbox));
                if (headHitbox != null) dmg *= config.headshotMultiplier;
                zombie.TakeDamage(dmg);
                return;
            }

            if (settings != null && settings.friendlyFireEnabled)
            {
                PlayerHealthManager targetHealth = (PlayerHealthManager)hit.collider.GetComponentInParent(typeof(PlayerHealthManager));
                if (targetHealth != null) targetHealth.ApplyDamage(config.damagePerHit);
            }
        }
    }

    private void StartReload()
    {
        if (isReloading) return;
        if (reserveAmmo <= 0) return;
        isReloading = true;
        if (reloadSound != null) reloadSound.Play();
        SendCustomEventDelayedSeconds(nameof(FinishReload), config.reloadTime);
    }

    public void FinishReload()
    {
        int needed = config.magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;
        isReloading = false;
    }

    public void AddReserveAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(config.reserveAmmoMax, reserveAmmo + amount);
    }
}
