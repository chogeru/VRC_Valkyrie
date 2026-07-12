using UdonSharp;
using UnityEngine;

// Place on a trigger-collider GameObject (ammo crate). Any Gun carried by
// the player who touches it gets refilled. Works for any weapon type since
// it just calls back into whichever Gun triggered it - not synced (local
// pickup timing is fine for a simple ammo crate).
public class AmmoPickup : UdonSharpBehaviour
{
    [Header("Settings")]
    public int ammoAmount = 60;
    public float respawnCooldown = 20f;

    [Header("References")]
    public GameObject visual;
    public Collider triggerCollider;
    [Tooltip("Must NOT be on the 'visual' object - it gets disabled immediately, which would cut this off mid-playback.")]
    public AudioSource pickupSound;

    private bool available = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!available) return;
        Gun gun = other.GetComponentInParent<Gun>();
        if (gun == null) return;

        gun.AddReserveAmmo(ammoAmount);
        available = false;
        if (pickupSound != null) pickupSound.Play();
        if (visual != null) visual.SetActive(false);
        if (triggerCollider != null) triggerCollider.enabled = false;
        SendCustomEventDelayedSeconds(nameof(Respawn), respawnCooldown);
    }

    public void Respawn()
    {
        available = true;
        if (visual != null) visual.SetActive(true);
        if (triggerCollider != null) triggerCollider.enabled = true;
    }

    // Editor-only Scene view aid (ignored by Udon at runtime).
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
