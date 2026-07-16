using UdonSharp;
using UnityEngine;

// Per-weapon audio configuration. Add this UdonBehaviour as a component on
// the same GameObject as Gun (or a child of it), then assign it to
// Gun.audioConfig in the Inspector.
//
// Each weapon type gets its own WeaponAudioConfig instance with its own
// AudioSources and clips - no Gun.cs code change is ever needed when
// tuning or adding sounds for a weapon. Simply wire clips here.
//
// Setup in Inspector:
//   fireSource    - 3D AudioSource near the muzzle (spatial, short distance)
//   fireClips     - 1 to 3 clip variations (one picked at random each shot)
//   pitchVariance - small random pitch shift for natural feel (0.0 = off)
//   mechanicSource- 3D AudioSource on weapon body (reload / dry-fire)
//   reloadClip    - plays when reload begins
//   dryFireClip   - plays when trigger pulled on empty magazine
//   uiSource      - non-spatial (2D) AudioSource for tier-up fanfare
//   tierUpClip    - plays when a tier upgrade is purchased
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class WeaponAudioConfig : UdonSharpBehaviour
{
    [Header("Fire Sound")]
    [Tooltip("3D AudioSource placed near the muzzle tip for positional fire audio.")]
    public AudioSource fireSource;
    [Tooltip("One clip is picked at random per shot. Add 2-3 variations to avoid repetition.")]
    public AudioClip[] fireClips;
    [Tooltip("Random pitch offset applied each shot. 0 = no variation, 0.08 is a natural feel.")]
    [Range(0f, 0.3f)] public float pitchVariance = 0.06f;

    [Header("Mechanical Sounds (reload / dry-fire)")]
    [Tooltip("3D AudioSource on the weapon body. Shared for reload and dry-fire.")]
    public AudioSource mechanicSource;
    public AudioClip reloadClip;
    public AudioClip dryFireClip;

    [Header("Upgrade Sound")]
    [Tooltip("Non-spatial (2D) AudioSource for the tier-up fanfare - heard globally.")]
    public AudioSource uiSource;
    public AudioClip tierUpClip;

    // Called by Gun.cs every time a shot fires.
    // Picks a random clip from fireClips and applies a small pitch shift.
    public void PlayFire()
    {
        if (fireSource == null || fireClips == null || fireClips.Length == 0) return;
        AudioClip clip = fireClips[Random.Range(0, fireClips.Length)];
        if (clip == null) return;
        fireSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        fireSource.PlayOneShot(clip);
    }

    // Called by Gun.cs when a reload sequence begins.
    public void PlayReload()
    {
        if (mechanicSource != null && reloadClip != null)
            mechanicSource.PlayOneShot(reloadClip);
    }

    // Called by Gun.cs when the trigger is pulled with no ammo remaining.
    public void PlayDryFire()
    {
        if (mechanicSource != null && dryFireClip != null)
            mechanicSource.PlayOneShot(dryFireClip);
    }

    // Called by Gun.cs (locally and via OnDeserialization) on tier upgrade.
    public void PlayTierUp()
    {
        if (uiSource != null && tierUpClip != null)
            uiSource.PlayOneShot(tierUpClip);
    }
}
