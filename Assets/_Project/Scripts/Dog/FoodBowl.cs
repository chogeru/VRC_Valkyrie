using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Place on the food bowl together with a Collider so it can be Interact()-ed.
// Any player can refill it; DogAI walks over and eats from it once it's
// hungry and this bowl reports HasFood(). Not owner-critical enough to
// worry about race conditions - refill/consume just toggle a synced bool.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FoodBowl : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("Visual child object representing the food pile, shown only while filled.")]
    public GameObject foodVisual;

    [UdonSynced] private bool filled;
    private bool lastAppliedFilled;

    void Start()
    {
        ApplyVisualLocal();
    }

    public override void Interact()
    {
        Debug.Log("[FoodBowl] Interact (already filled=" + filled + ")");
        if (filled) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        filled = true;
        RequestSerialization();
        ApplyVisualLocal();
    }

    public bool HasFood()
    {
        return filled;
    }

    // Called by DogAI (locally, on whichever client owns the dog) once it
    // finishes eating.
    public void Consume()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        filled = false;
        RequestSerialization();
        ApplyVisualLocal();
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
