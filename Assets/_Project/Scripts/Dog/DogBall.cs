using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Attach to the fetch ball together with a Rigidbody, a VRC Pickup, and a
// VRC Object Sync component (added directly in the scene, no code
// reference needed here - Object Sync automatically broadcasts whatever
// the current network owner does to this transform, whether that's a
// player throwing it or DogAI carrying it in its mouth).
public class DogBall : UdonSharpBehaviour
{
    [Header("References")]
    public DogAI dogAI;
    public Rigidbody rb;

    private VRC_Pickup pickup;
    private bool carried;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        pickup = GetComponent<VRC_Pickup>();
    }

    public override void OnPickup()
    {
        Debug.Log("[DogBall] OnPickup by " + Networking.LocalPlayer.displayName);
        // A player grabbed it out of the air/ground - abandon any in-progress fetch.
        if (dogAI != null) dogAI.CancelFetch();
    }

    public override void OnDrop()
    {
        Debug.Log("[DogBall] OnDrop at " + transform.position + " carried(by dog)=" + carried);
        // Only a player release should start a fetch; DogAI's own carry/drop
        // cycle (SetCarried) never goes through VRC_Pickup, so this only
        // fires for genuine throws.
        if (dogAI != null) dogAI.RequestFetch(this);
    }

    // Called by DogAI when it picks the ball up in its mouth / sets it back down.
    public void SetCarried(bool state)
    {
        carried = state;
        if (pickup != null) pickup.pickupable = !state;
        if (rb != null)
        {
            rb.isKinematic = state;
            if (!state) rb.velocity = Vector3.zero;
        }
    }
}
