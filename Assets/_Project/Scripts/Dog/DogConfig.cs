using UdonSharp;
using UnityEngine;

// Data container describing the pet dog's tunable stats/timings. Duplicate
// this asset to make variant "breeds" later if needed - DogAI just reads
// whichever DogConfig is assigned to it.
public class DogConfig : UdonSharpBehaviour
{
    [Header("Locomotion Speed Thresholds (Animator 'Speed' blend tree)")]
    public float walkSpeed = 1.4f;
    public float trotSpeed = 2.8f;
    public float runSpeed = 4.5f;
    public float fastRunSpeed = 6.5f;

    [Header("NavMeshAgent Speeds")]
    public float wanderMoveSpeed = 1.6f;
    public float fetchMoveSpeed = 4.5f;
    public float agilityMoveSpeed = 4.0f;
    public float angularSpeed = 360f;
    public float acceleration = 8f;

    [Header("Needs Decay (per second, 0-1 scale)")]
    public float hungerDecayPerSecond = 0.01f;
    public float thirstDecayPerSecond = 0.015f;
    public float energyDecayPerSecond = 0.006f;
    [Tooltip("Needs below this (0-1) trigger the dog to seek food/water/sleep.")]
    public float needThreshold = 0.35f;
    [Tooltip("Energy regain rate per second while sleeping.")]
    public float energyRegenPerSecond = 0.08f;

    [Header("Affection")]
    public float affectionPerPet = 0.08f;
    [Tooltip("Affection decays slowly over time so petting stays meaningful.")]
    public float affectionDecayPerSecond = 0.002f;

    [Header("Action Durations (seconds)")]
    public float eatDuration = 4f;
    public float drinkDuration = 3.5f;
    public float chewDuration = 5f;
    public float petReactionDuration = 2.5f;
    public float sleepMinDuration = 8f;

    [Header("Wander")]
    public float wanderRadius = 6f;
    public float wanderIntervalMin = 3f;
    public float wanderIntervalMax = 8f;
    [Range(0f, 1f)] public float idleClipChance = 0.3f;

    [Header("Greeting")]
    [Tooltip("A player standing this close while the dog is idle triggers a happy greeting bark.")]
    public float greetDistance = 2.5f;
    [Tooltip("Minimum time between greeting barks, so a player standing nearby doesn't get barked at constantly.")]
    public float greetCooldownSeconds = 20f;

    [Header("Fetch")]
    public float ballPickupDistance = 0.6f;
    public float ballReturnDistance = 1.2f;

    [Header("Treat / Beg")]
    [Tooltip("How close the dog must get to the offered treat before it eats it.")]
    public float treatEatDistance = 0.6f;
    [Tooltip("How long the Treat animation plays before the dog returns to Idle. Roughly matches the Eat_tear clip's own length (~4.17s at 24fps).")]
    public float treatEatDuration = 4.17f;
    [Tooltip("Hunger (0-1 scale) instantly restored by eating one treat.")]
    public float treatHungerBoost = 0.3f;
    [Tooltip("Affection (0-1 scale) gained from being given a treat - bigger than a single pet, since offering food is a more meaningful gesture.")]
    public float affectionPerTreat = 0.15f;

    [Header("Agility Course")]
    [Tooltip("How often (seconds), on average, the dog decides to run the agility course when otherwise idle/wandering.")]
    public float agilityIntervalSeconds = 45f;
    public float agilityWaypointArriveDistance = 0.5f;
    public float agilityJumpLeadDistance = 1.0f;

    [Header("Voice Audio (optional)")]
    public AudioClip[] barkClips;
    public AudioClip[] happyClips;
}
