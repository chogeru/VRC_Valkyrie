using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Attach to a weapon GameObject alongside a VRC Pickup component. Swap the
// "config" reference to reuse this exact script for every weapon type
// (pistol / rifle / shotgun / ...) - only the data changes, never the code.
//
// Killing a zombie pays score (ZombieConfig.scoreValue) to the shooter's
// wallet. Score is spent at a WeaponUpgradeStation to raise this specific
// gun through up to 3 tiers, each boosting damage / fire rate / magazine
// size / reload speed (WeaponConfig.tier*Multiplier arrays) and costing
// more than the last (WeaponConfig.tierUpgradeCost). The tier lives on the
// gun itself (synced), so it persists even if the weapon changes hands.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Gun : UdonSharpBehaviour
{
    [Header("Config")]
    public WeaponConfig config;

    [Header("References")]
    public GameSettings settings;
    public HudController hud;
    public Transform muzzle;
    public ParticleSystem muzzleFlash;
    public AudioSource fireSound;
    public AudioSource reloadSound;
    public AudioSource emptySound;
    public AudioSource tierUpSound;
    public LayerMask hitMask = ~0;

    [Header("Runtime (read-only, for debugging)")]
    public int currentAmmo;
    public int reserveAmmo;

    // Procedural slide/bolt + charging handle motion - purely script-driven
    // (no baked Animator clips required), so it works with any weapon model:
    // just assign the moving child Transform and tune the offsets/timings.
    [Header("Slide / Bolt Animation (optional)")]
    [Tooltip("The slide/bolt child Transform that racks back on every shot. Leave empty to skip.")]
    public Transform slide;
    [Tooltip("Local-space offset (from its rest position) the slide reaches at full travel.")]
    public Vector3 slideBackOffset = new Vector3(0f, 0f, -0.03f);
    public float slideBackDuration = 0.04f;
    public float slideForwardDuration = 0.08f;

    [Header("Charging Handle Animation (optional)")]
    [Tooltip("The charging handle child Transform that racks once after a reload finishes. Leave empty to skip.")]
    public Transform chargingHandle;
    public Vector3 chargingHandleBackOffset = new Vector3(0f, 0f, -0.05f);
    public float chargingHandleCycleDuration = 0.25f;

    public const int MaxTier = 3;
    [UdonSynced] public int tier; // 0 = base, 1-3 = upgrade tiers purchased at the shop

    private bool isReloading;
    private bool triggerHeld;
    private float nextFireTime;
    private int lastAppliedTier = -1;

    private Vector3 slideRestLocalPos;
    private bool slideAnimating;
    private bool slideGoingBack;
    private float slideAnimStartTime;

    private Vector3 chargeRestLocalPos;
    private bool chargeAnimating;
    private bool chargeGoingBack;
    private float chargeAnimStartTime;

    void Start()
    {
        currentAmmo = EffectiveMagazineSize();
        reserveAmmo = config.reserveAmmoMax;
        lastAppliedTier = tier;

        if (slide != null) slideRestLocalPos = slide.localPosition;
        if (chargingHandle != null) chargeRestLocalPos = chargingHandle.localPosition;
    }

    void Update()
    {
        UpdateSlideAnim();
        UpdateChargeAnim();
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

    public override void OnDeserialization()
    {
        if (tier != lastAppliedTier)
        {
            lastAppliedTier = tier;
            if (hud != null) hud.OnWeaponTierChanged(config.weaponName, tier);
            if (tierUpSound != null) tierUpSound.Play();
        }
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
        float effectiveFireRate = EffectiveFireRate();
        nextFireTime = Time.time + (1f / Mathf.Max(0.01f, effectiveFireRate));

        currentAmmo--;
        FireShot();

        if (config.isAutomatic && triggerHeld)
        {
            SendCustomEventDelayedSeconds(nameof(AutoFireTick), 1f / Mathf.Max(0.01f, effectiveFireRate));
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
        TriggerSlideCycle();

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
                float dmg = EffectiveDamage();
                ZombieHeadHitbox headHitbox = (ZombieHeadHitbox)hit.collider.GetComponent(typeof(ZombieHeadHitbox));
                if (headHitbox != null) dmg *= config.headshotMultiplier;

                bool killedByThisShot = zombie.TakeDamage(dmg);
                if (killedByThisShot)
                {
                    int reward = zombie.config != null ? zombie.config.scoreValue : 0;
                    AwardScoreToLocalPlayer(reward);
                }
                return;
            }

            if (settings != null && settings.friendlyFireEnabled)
            {
                PlayerHealthManager targetHealth = (PlayerHealthManager)hit.collider.GetComponentInParent(typeof(PlayerHealthManager));
                if (targetHealth != null) targetHealth.ApplyDamage(EffectiveDamage());
            }
        }
    }

    // The shooter is always the local player (firing is a local input
    // event), so credit their own per-player wallet (PlayerHealthManager).
    private void AwardScoreToLocalPlayer(int amount)
    {
        if (amount <= 0) return;
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local == null || settings == null || settings.playerHealthObjectPrefab == null) return;

        GameObject obj = local.GetPlayerObject(settings.playerHealthObjectPrefab);
        if (obj == null) return;
        PlayerHealthManager wallet = (PlayerHealthManager)obj.GetComponent(typeof(PlayerHealthManager));
        if (wallet != null) wallet.AddScore(amount);
    }

    // Called by WeaponUpgradeStation.Interact() when the local player is
    // holding this gun and presses the upgrade button.
    public void TryUpgrade()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local == null || settings == null || settings.playerHealthObjectPrefab == null) return;

        if (tier >= MaxTier)
        {
            if (hud != null) hud.ShowShopMessage(config.weaponName + " は既に最大強化です");
            return;
        }

        int cost = GetUpgradeCost(tier + 1);
        GameObject obj = local.GetPlayerObject(settings.playerHealthObjectPrefab);
        if (obj == null) return;
        PlayerHealthManager wallet = (PlayerHealthManager)obj.GetComponent(typeof(PlayerHealthManager));
        if (wallet == null) return;

        if (!wallet.TrySpendScore(cost))
        {
            if (hud != null) hud.ShowShopMessage("スコア不足 (" + wallet.GetScore() + " / " + cost + ")");
            return;
        }

        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        tier++;
        lastAppliedTier = tier;
        RequestSerialization();
        if (hud != null) hud.OnWeaponTierChanged(config.weaponName, tier);
        if (tierUpSound != null) tierUpSound.Play();
    }

    public int GetUpgradeCost(int targetTier)
    {
        int[] costs = config.tierUpgradeCost;
        if (costs == null || costs.Length == 0) return 0;
        int index = Mathf.Clamp(targetTier - 1, 0, costs.Length - 1);
        return costs[index];
    }

    private float GetTierMultiplier(float[] tierArray)
    {
        if (tier <= 0 || tierArray == null || tierArray.Length == 0) return 1f;
        int index = Mathf.Clamp(tier - 1, 0, tierArray.Length - 1);
        return tierArray[index];
    }

    private float EffectiveDamage() { return config.damagePerHit * GetTierMultiplier(config.tierDamageMultiplier); }
    private float EffectiveFireRate() { return config.fireRate * GetTierMultiplier(config.tierFireRateMultiplier); }
    private int EffectiveMagazineSize() { return Mathf.Max(1, Mathf.RoundToInt(config.magazineSize * GetTierMultiplier(config.tierMagazineSizeMultiplier))); }
    private float EffectiveReloadTime() { return config.reloadTime * GetTierMultiplier(config.tierReloadTimeMultiplier); }

    private void StartReload()
    {
        if (isReloading) return;
        if (reserveAmmo <= 0) return;
        isReloading = true;
        if (reloadSound != null) reloadSound.Play();
        SendCustomEventDelayedSeconds(nameof(FinishReload), EffectiveReloadTime());
    }

    public void FinishReload()
    {
        int needed = EffectiveMagazineSize() - currentAmmo;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;
        isReloading = false;
        TriggerChargeCycle();
    }

    public void AddReserveAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(config.reserveAmmoMax, reserveAmmo + amount);
    }

    public int GetTier() { return tier; }

    // --- Procedural slide/bolt animation -----------------------------

    private void TriggerSlideCycle()
    {
        if (slide == null) return;
        slideAnimating = true;
        slideGoingBack = true;
        slideAnimStartTime = Time.time;
    }

    private void UpdateSlideAnim()
    {
        if (!slideAnimating || slide == null) return;

        float duration = slideGoingBack ? slideBackDuration : slideForwardDuration;
        float t = Mathf.Clamp01((Time.time - slideAnimStartTime) / Mathf.Max(0.001f, duration));
        Vector3 from = slideGoingBack ? slideRestLocalPos : slideRestLocalPos + slideBackOffset;
        Vector3 to = slideGoingBack ? slideRestLocalPos + slideBackOffset : slideRestLocalPos;
        slide.localPosition = Vector3.Lerp(from, to, t);

        if (t >= 1f)
        {
            if (slideGoingBack)
            {
                slideGoingBack = false;
                slideAnimStartTime = Time.time;
            }
            else
            {
                slideAnimating = false;
                slide.localPosition = slideRestLocalPos;
            }
        }
    }

    // --- Procedural charging handle animation -------------------------

    private void TriggerChargeCycle()
    {
        if (chargingHandle == null) return;
        chargeAnimating = true;
        chargeGoingBack = true;
        chargeAnimStartTime = Time.time;
    }

    private void UpdateChargeAnim()
    {
        if (!chargeAnimating || chargingHandle == null) return;

        float half = chargingHandleCycleDuration * 0.5f;
        float t = Mathf.Clamp01((Time.time - chargeAnimStartTime) / Mathf.Max(0.001f, half));
        Vector3 from = chargeGoingBack ? chargeRestLocalPos : chargeRestLocalPos + chargingHandleBackOffset;
        Vector3 to = chargeGoingBack ? chargeRestLocalPos + chargingHandleBackOffset : chargeRestLocalPos;
        chargingHandle.localPosition = Vector3.Lerp(from, to, t);

        if (t >= 1f)
        {
            if (chargeGoingBack)
            {
                chargeGoingBack = false;
                chargeAnimStartTime = Time.time;
            }
            else
            {
                chargeAnimating = false;
                chargingHandle.localPosition = chargeRestLocalPos;
            }
        }
    }
}
