using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Water counterpart to FoodBowl.cs - see that file for the rationale
// (DogAI reads/writes the public `filled` field directly rather than
// calling methods on this behaviour).
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WaterBowl : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("Visual child object representing the water surface, shown only while filled.")]
    public GameObject waterVisual;

    [UdonSynced] public bool filled;
    private bool lastAppliedFilled;

    void Start()
    {
        ApplyVisualLocal();
    }

    void Update()
    {
        if (filled != lastAppliedFilled) ApplyVisualLocal();
    }

    public override void Interact()
    {
        if (filled) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        filled = true;
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
