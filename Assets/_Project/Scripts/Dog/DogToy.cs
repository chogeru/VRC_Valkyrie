using UdonSharp;
using UnityEngine;

// Place on the chew bone (or any other static toy) together with a
// Collider so it can be Interact()-ed. Simpler than DogBall - the dog just
// walks over and chews in place, it doesn't get carried anywhere.
public class DogToy : UdonSharpBehaviour
{
    [Header("References")]
    public DogAI dogAI;

    public override void Interact()
    {
        Debug.Log("[DogToy] Interact, dogAI=" + (dogAI != null));
        if (dogAI != null) dogAI.NotifyToyGiven(transform);
    }
}
