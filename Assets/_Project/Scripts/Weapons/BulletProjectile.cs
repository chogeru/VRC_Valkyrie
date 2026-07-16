using UdonSharp;
using UnityEngine;

// Visual-only mecha-style bullet projectile. Moves forward at speed and
// auto-deactivates after it reaches its target distance or maxLifetime.
//
// Hit detection is handled by Gun.cs's instant Raycast - this object is
// purely cosmetic: it flies to the exact point the ray hit, then vanishes.
//
// Object pooling: pre-place N inactive copies as children of the weapon
// (or a scene pool root), assign them to Gun.bulletPool[], and Gun will
// round-robin through them on every shot. Pool size of 8-12 covers any
// realistic fire rate, including full-auto rifles.
//
// Visual setup (all assigned in the Inspector - script is mesh/renderer agnostic):
//   bulletLight   - Point Light component (cyan/blue, range ~2m, intensity ~3)
//                   for the glowing core effect
//   trail         - TrailRenderer with a bright sci-fi gradient (short time ~0.05s)
//   activeEffect  - Optional ParticleSystem playing while bullet is in flight
//                   (e.g. a tiny energy spark emitter riding the bullet)
//
// Tip: use an Unlit or Additive-blended material on the bullet mesh for the
// bright over-exposed look typical of mecha/sci-fi energy rounds.
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BulletProjectile : UdonSharpBehaviour
{
    [Header("Motion")]
    [Tooltip("Travel speed in world units per second. 80-150 m/s looks natural.")]
    public float speed = 100f;
    [Tooltip("Failsafe lifetime - bullet self-destructs after this many seconds regardless of distance.")]
    public float maxLifetime = 2f;

    [Header("Visual Components (assign in Inspector)")]
    [Tooltip("Point Light giving the bullet a glowing-core look. Disabled while pooled.")]
    public Light bulletLight;
    [Tooltip("TrailRenderer for the energy/plasma trail. Clear()ed each activation.")]
    public TrailRenderer trail;
    [Tooltip("Optional ParticleSystem that plays while the bullet is flying (e.g. spark emitter).")]
    public ParticleSystem activeEffect;

    private bool active;
    private Vector3 moveDir;
    private float distanceTraveled;
    private float stopDistance;   // set by Gun to the raycast hit distance
    private float spawnTime;

    // Called by Gun.cs to activate and launch this pooled bullet.
    // origin    - muzzle world position
    // dir       - normalised forward direction (may include spread)
    // travelDist- how far to fly before stopping (= raycast hit.distance,
    //             or config.range when the shot missed everything)
    public void Fire(Vector3 origin, Vector3 dir, float travelDist)
    {
        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(dir);
        moveDir = dir.normalized;
        distanceTraveled = 0f;
        stopDistance = Mathf.Max(0.1f, travelDist);
        spawnTime = Time.time;
        active = true;

        if (trail != null) trail.Clear();
        if (bulletLight != null) bulletLight.enabled = true;
        if (activeEffect != null) activeEffect.Play();

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!active) return;

        float step = speed * Time.deltaTime;
        transform.position += moveDir * step;
        distanceTraveled += step;

        if (distanceTraveled >= stopDistance || (Time.time - spawnTime) >= maxLifetime)
            Deactivate();
    }

    private void Deactivate()
    {
        active = false;
        if (bulletLight != null) bulletLight.enabled = false;
        if (activeEffect != null) activeEffect.Stop();
        gameObject.SetActive(false);
    }
}
