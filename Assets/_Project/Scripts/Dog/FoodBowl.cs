using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Place on the food bowl together with a Collider so it can be Interact()-ed.
// Any player can refill it; DogAI walks over and eats from it once it's
// hungry and food is available.
//
// `filled` is a public synced field that DogAI both reads and writes
// directly (foodBowl.filled), rather than calling methods on this
// behaviour. Cross-UdonSharpBehaviour method INVOCATION (whether it returns
// a value, like the old HasFood(), or performs a field write as a side
// effect, like a push-notification/Consume() call) was observed in this
// project's runtime to execute the method body (visible side effects like
// Debug.Log fire) without the resulting field write actually persisting on
// the callee's live heap. A direct cross-object FIELD read/write (no method
// call involved) does not have this problem - DogAI already relies on
// reading config.* fields this way every tick. See
// [[unity_udonsharpbehaviour_cross_reference_binding]] memory.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FoodBowl : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("Visual child object representing the food pile, shown only while filled.")]
    public GameObject foodVisual;

    [UdonSynced] public bool filled;
    private bool lastAppliedFilled;

    void Start()
    {
        ApplyVisualLocal();
    }

    public bool debugLogging = false;
    private float nextDebugLogTime;

    void Update()
    {
        // Local-only visual follow, since DogAI writes `filled` directly
        // rather than through a method that could also refresh the visual.
        if (filled != lastAppliedFilled) ApplyVisualLocal();

        if (debugLogging && Time.time >= nextDebugLogTime)
        {
            nextDebugLogTime = Time.time + 3f;
            Debug.Log("[FoodBowl] status filled=" + filled);
        }
    }

    public override void Interact()
    {
        if (debugLogging) Debug.Log("[FoodBowl] Interact called, filled was=" + filled);
        if (filled) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        filled = true;
        RequestSerialization();
        ApplyVisualLocal();
        if (debugLogging) Debug.Log("[FoodBowl] Interact done, filled now=" + filled);
    }

    public override void OnDeserialization()
    {
        if (filled != lastAppliedFilled) ApplyVisualLocal();
    }

    private void ApplyVisualLocal()
    {
        lastAppliedFilled = filled;
        if (foodVisual != null) foodVisual.SetActive(filled);
    }
}
