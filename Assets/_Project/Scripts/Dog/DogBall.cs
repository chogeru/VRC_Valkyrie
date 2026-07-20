using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Attach to the fetch ball together with a Rigidbody, a VRC Pickup, and a
// VRC Object Sync component (added directly in the scene, no code
// reference needed here - Object Sync automatically broadcasts whatever
// the current network owner does to this transform, whether that's a
// player throwing it or DogAI carrying it in its mouth).
//
// `wasThrown`/`heldByPlayer` are public fields DogAI polls directly each
// tick (ball.wasThrown, ball.heldByPlayer) rather than this script pushing
// a fetch request via a method call on DogAI - see FoodBowl.cs for why
// cross-behaviour method invocation isn't used for this kind of signal in
// this project.
public class DogBall : UdonSharpBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    public bool wasThrown;
    public bool heldByPlayer;

    private VRC_Pickup pickup;
    private Collider ballCollider;
    private bool carried;

    public bool debugLogging = true;
    private float nextDebugLogTime;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        pickup = GetComponent<VRC_Pickup>();
        ballCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (debugLogging && Time.time >= nextDebugLogTime)
        {
            nextDebugLogTime = Time.time + 2f;
            Debug.Log("[DogBall] status wasThrown=" + wasThrown + " heldByPlayer=" + heldByPlayer + " carried=" + carried + " pos=" + transform.position);
        }
    }

    public override void OnPickup()
    {
        heldByPlayer = true;
        wasThrown = false;
        if (debugLogging) Debug.Log("[DogBall] OnPickup fired");
    }

    public override void OnDrop()
    {
        heldByPlayer = false;
        // Only a genuine player release should start a fetch; DogAI's own
        // carry/drop cycle (SetCarried) never goes through VRC_Pickup, so
        // `carried` distinguishes "the dog is holding it" from a real throw.
        if (!carried) wasThrown = true;
        if (debugLogging) Debug.Log("[DogBall] OnDrop fired, carried=" + carried + " wasThrown now=" + wasThrown);
    }

    // Called by DogAI when it picks the ball up in its mouth / sets it back down.
    public void SetCarried(bool state)
    {
        carried = state;
        if (pickup != null) pickup.pickupable = !state;
        // Disable the physical collider while carried - it stays kinematic
        // and glued to the mouth socket every frame, so a live collider can
        // only ever generate spurious contacts against the dog's own
        // CapsuleCollider or ground geometry as it's dragged through them.
        if (ballCollider != null) ballCollider.enabled = !state;
        if (rb != null)
        {
            rb.isKinematic = state;
            if (!state) rb.velocity = Vector3.zero;
        }
    }
}
