using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Place on the chew bone (or any other static toy) together with a
// Collider so it can be Interact()-ed. Simpler than DogBall - the dog just
// walks over and chews in place, it doesn't get carried anywhere.
//
// `wasGiven` is a public field DogAI polls directly (toy.wasGiven) rather
// than this script pushing a notification via a method call on DogAI -
// see FoodBowl.cs for why.
//
// `syncedHidden` is synced so every client hides/shows the prop and disables
// its collider and Interact in lockstep while the dog chews it and while it
// waits to respawn - same shape as DogTreat's syncedHidden.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DogToy : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("Auto-collected from children at Start if left empty.")]
    public Renderer[] renderers;

    [Header("Respawn")]
    [Tooltip("Seconds between the dog finishing with this toy and it reappearing at its original spot, ready to be given again.")]
    public float respawnDelay = 6f;

    // Polled directly by DogAI - see class comment above.
    public bool wasGiven;

    // Whether the toy is currently hidden (being chewed / respawning).
    // Synced so every client hides/shows in lockstep.
    [UdonSynced] private bool syncedHidden;
    private bool appliedHidden;

    private Collider toyCollider;
    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private float respawnTime;

    void Start()
    {
        toyCollider = GetComponent<Collider>();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>(true);
        spawnPos = transform.position;
        spawnRot = transform.rotation;
    }

    void Update()
    {
        if (syncedHidden != appliedHidden) ApplyHidden(syncedHidden);

        // Only the current owner drives the respawn timer, same single-driver
        // shape DogAI and DogTreat use so two clients never race to respawn.
        if (syncedHidden && Time.time >= respawnTime && Networking.IsOwner(gameObject))
        {
            Respawn();
        }
    }

    public override void Interact()
    {
        // Only allow interaction when visible.
        if (syncedHidden) return;
        wasGiven = true;
    }

    // Called by DogAI once it arrives and starts chewing.
    // Caller is responsible for taking ownership of this GameObject first
    // (same pattern as DogTreat.Consume()).
    public void Consume()
    {
        wasGiven = false;
        respawnTime = Time.time + respawnDelay;
        syncedHidden = true;
        RequestSerialization();
        ApplyHidden(true);
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(spawnPos, spawnRot);
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
        if (toyCollider != null) toyCollider.enabled = !hidden;
    }

    public override void OnDeserialization()
    {
        if (syncedHidden != appliedHidden) ApplyHidden(syncedHidden);
    }
}
