using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// Attach to a treat prop together with a Rigidbody, a VRC Pickup, and a
// Collider (no VRC Object Sync needed - unlike DogBall this prop never gets
// carried around mid-air by the dog, it just vanishes in place and later
// reappears at its own spawn point, so only the hidden/visible flag needs to
// be synced, not a moving transform).
//
// `heldByPlayer`/`wasOffered` are public fields DogAI polls directly each
// tick (treat.wasOffered, treat.heldByPlayer), same shape as DogBall's
// wasThrown/heldByPlayer - see FoodBowl.cs for why cross-behaviour method
// calls aren't used for this kind of signal in this project.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DogTreat : UdonSharpBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    [Tooltip("Auto-collected from children at Start if left empty.")]
    public Renderer[] renderers;

    [Header("Respawn")]
    [Tooltip("Seconds between the dog eating this treat and it reappearing at its original spot, ready to be picked up again.")]
    public float respawnDelay = 4f;

    // Polled directly by DogAI - see class comment above.
    public bool heldByPlayer;
    public bool wasOffered;

    // Whether the treat is currently eaten/hidden. Synced so every client
    // (not just the owner driving the respawn timer) hides/shows the model
    // and disables/enables its collider and pickup in lockstep.
    [UdonSynced] private bool syncedHidden;
    private bool appliedHidden;

    private VRC_Pickup pickup;
    private Collider treatCollider;
    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private float respawnTime;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        treatCollider = GetComponent<Collider>();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>(true);
        spawnPos = transform.position;
        spawnRot = transform.rotation;
    }

    void Update()
    {
        if (syncedHidden != appliedHidden) ApplyHidden(syncedHidden);

        // Only the current owner drives the respawn timer and writes the
        // resulting state change - same single-driver shape DogAI itself
        // uses for its own synced fields, so two clients never race to
        // respawn the same treat.
        if (syncedHidden && Time.time >= respawnTime && Networking.IsOwner(gameObject))
        {
            Respawn();
        }
    }

    public override void OnPickup()
    {
        heldByPlayer = true;
        wasOffered = false;
    }

    public override void OnDrop()
    {
        heldByPlayer = false;
        // Only a genuine player release counts as "offered" - a drop that
        // happens because DogAI just ate it (syncedHidden already true) must
        // not immediately flag it as offered again.
        if (!syncedHidden) wasOffered = true;
    }

    // Called by DogAI once it arrives at the treat. The caller is
    // responsible for taking ownership of this GameObject first (same
    // pattern DogBall's callers use before SetCarried).
    public void Consume()
    {
        wasOffered = false;
        heldByPlayer = false;
        respawnTime = Time.time + respawnDelay;
        syncedHidden = true;
        RequestSerialization();
        ApplyHidden(true);
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(spawnPos, spawnRot);
        if (rb != null) rb.velocity = Vector3.zero;
        syncedHidden = false;
        RequestSerialization();
        ApplyHidden(false);
    }

    private void ApplyHidden(bool hidden)
    {
        appliedHidden = hidden;
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].enabled = !hidden;
            }
        }
        if (treatCollider != null) treatCollider.enabled = !hidden;
        if (pickup != null) pickup.pickupable = !hidden;
        if (rb != null) rb.isKinematic = hidden;
    }

    public override void OnDeserialization()
    {
        if (syncedHidden != appliedHidden) ApplyHidden(syncedHidden);
    }
}
