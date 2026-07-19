using UdonSharp;
using UnityEngine;

// Place on the chew bone (or any other static toy) together with a
// Collider so it can be Interact()-ed. Simpler than DogBall - the dog just
// walks over and chews in place, it doesn't get carried anywhere.
//
// `wasGiven` is a public field DogAI polls directly (toy.wasGiven) rather
// than this script pushing a notification via a method call on DogAI -
// see FoodBowl.cs for why.
public class DogToy : UdonSharpBehaviour
{
    public bool wasGiven;

    public override void Interact()
    {
        wasGiven = true;
    }
}
