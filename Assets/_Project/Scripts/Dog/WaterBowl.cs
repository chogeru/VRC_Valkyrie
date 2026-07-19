using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Water counterpart to FoodBowl.cs - see that file for the rationale.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WaterBowl : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("Visual child object representing the water surface, shown only while filled.")]
    public GameObject waterVisual;

    [UdonSynced] private bool filled;
    private bool lastAppliedFilled;

    void Start()
    {
        ApplyVisualLocal();
    }

    public override void Interact()
    {
        Debug.Log("[WaterBowl] Interact (already filled=" + filled + ")");
        if (filled) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        filled = true;
        RequestSerialization();
        ApplyVisualLocal();
    }

    public bool HasWater()
    {
        return filled;
    }

    // Called by DogAI (locally, on whichever client owns the dog) once it
    // finishes drinking.
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
        if (waterVisual != null) waterVisual.SetActive(filled);
    }
}
