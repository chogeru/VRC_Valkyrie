using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// UdonSharp-native procedural look-at for the dog.
//
// VRChat does NOT execute regular C# MonoBehaviour callbacks (Start/Update/
// LateUpdate) on custom scripts like FLookAnimator at runtime - only Udon
// programs run. This script therefore implements the bone rotation logic
// directly in LateUpdate() so it runs AFTER the Animator has written its
// frame, overriding just the head/neck bones to track the nearest player.
//
// SETUP (Inspector):
//   1. Assign `headBone`  - the dog's head bone Transform.
//   2. Optionally fill `spineChain` with 1-2 neck/spine Transforms above the
//      head (index 0 = closest to head). They rotate at lower weight for a
//      natural distributed look.
//   3. Set `headForwardAxis` to the LOCAL axis that points out the nose on the
//      head bone (commonly Vector3.forward, but check in the Scene view).
//   4. Set `dogRoot` to the dog's root Transform (or leave null to use this GO).
//
// BehaviourSyncMode.None - visual / local only. Every client computes
// independently using their own local player positions.
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DogLookController : UdonSharpBehaviour
{
    [Header("Bones")]
    [Tooltip("The dog's head bone. This receives the highest rotation weight.")]
    public Transform headBone;
    [Tooltip("Optional chain of neck/spine bones above the head (index 0 = closest to head). Rotated at lower weight.")]
    public Transform[] spineChain;

    [Header("Bone Axis Setup")]
    [Tooltip("Which LOCAL axis on headBone points 'forward' (out the nose). Verify in the Scene view with the bone selected.")]
    public Vector3 headForwardAxis = Vector3.forward;
    [Tooltip("Which LOCAL axis on headBone points 'up'.")]
    public Vector3 headUpAxis = Vector3.up;

    [Header("Weights")]
    [Range(0f, 1f)]
    [Tooltip("How much of the look rotation is applied to the head bone (0 = none, 1 = full).")]
    public float headWeight = 0.65f;
    [Range(0f, 1f)]
    [Tooltip("Total rotation weight shared equally among all spineChain bones.")]
    public float spineWeight = 0.25f;

    [Header("Angle Limits")]
    [Tooltip("Maximum horizontal rotation from the bone's resting orientation (degrees).")]
    public float maxYaw = 55f;
    [Tooltip("Maximum vertical rotation from the bone's resting orientation (degrees).")]
    public float maxPitch = 35f;

    [Header("Player Tracking")]
    [Tooltip("Only track players within this radius (world units).")]
    public float trackDistance = 7f;
    [Tooltip("Vertical offset above a player's foot position to target (approximates head height).")]
    public float playerHeadOffset = 1.6f;
    [Tooltip("Root Transform of the dog (used for default 'look ahead' point). Defaults to this GO if null.")]
    public Transform dogRoot;

    [Header("Motion")]
    [Tooltip("Smoothing speed - higher = snappier tracking.")]
    public float smoothSpeed = 3.5f;
    [Tooltip("Distance to look ahead when no player is nearby.")]
    public float idleForwardDistance = 2.5f;

    [Header("Idle Glance")]
    [Tooltip("Average seconds between random glance-away moments when idle (0 = disabled).")]
    public float glanceInterval = 7f;
    [Tooltip("How long each glance lasts in seconds.")]
    public float glanceDuration = 2f;
    [Tooltip("Half-width of the random glance offset (world units).")]
    public float glanceRadius = 1.2f;

    // --- runtime state ---
    private Vector3 smoothedTarget;
    private Vector3 rawTarget;
    private float nextPlayerCheck;
    private VRCPlayerApi nearestPlayer;
    private float nextGlance;
    private float glanceEnd;
    private Vector3 glanceOfs;
    private bool glancing;
    private bool ready;

    void Start()
    {
        if (dogRoot == null) dogRoot = transform;
        if (headBone == null)
        {
            Debug.LogWarning("[DogLookController] headBone not assigned - disabled.");
            enabled = false;
            return;
        }
        smoothedTarget = dogRoot.position + dogRoot.forward * idleForwardDistance + Vector3.up * playerHeadOffset;
        rawTarget = smoothedTarget;
        ScheduleGlance();
        ready = true;
    }

    // LateUpdate runs AFTER the Animator has written bone rotations this frame,
    // so our procedural override stacks cleanly on top of the animation.
    void LateUpdate()
    {
        if (!ready) return;

        // --- 1. Rate-limited nearest-player search ---
        if (Time.time >= nextPlayerCheck)
        {
            nextPlayerCheck = Time.time + 0.35f;
            nearestPlayer = FindNearest();
        }

        // --- 2. Choose raw target ---
        if (nearestPlayer != null && nearestPlayer.IsValid())
        {
            rawTarget = nearestPlayer.GetPosition() + Vector3.up * playerHeadOffset;
            // Reset glance timer so idle glances restart after player leaves.
            glancing = false;
            ScheduleGlance();
        }
        else
        {
            rawTarget = dogRoot.position
                        + dogRoot.forward * idleForwardDistance
                        + Vector3.up * playerHeadOffset;

            if (glanceInterval > 0f)
            {
                if (!glancing && Time.time >= nextGlance)
                {
                    glancing = true;
                    glanceEnd = Time.time + glanceDuration;
                    glanceOfs = new Vector3(
                        Random.Range(-glanceRadius, glanceRadius),
                        Random.Range(-glanceRadius * 0.3f, glanceRadius * 0.3f),
                        0f);
                }
                if (glancing)
                {
                    if (Time.time < glanceEnd)
                        rawTarget += glanceOfs;
                    else
                    {
                        glancing = false;
                        ScheduleGlance();
                    }
                }
            }
        }

        // --- 3. Smooth toward target ---
        smoothedTarget = Vector3.Lerp(smoothedTarget, rawTarget,
            1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        // --- 4. Apply bone rotations ---
        ApplyLook(headBone, smoothedTarget, headWeight, maxYaw, maxPitch);

        if (spineChain != null && spineChain.Length > 0)
        {
            float perBone = spineWeight / spineChain.Length;
            for (int i = 0; i < spineChain.Length; i++)
            {
                if (spineChain[i] != null)
                    ApplyLook(spineChain[i], smoothedTarget, perBone, maxYaw * 0.4f, maxPitch * 0.4f);
            }
        }
    }

    // Rotates `bone` so that `headForwardAxis` points toward `target`,
    // clamped to ±yawLim/pitchLim relative to the parent's orientation,
    // blended with `weight` against the animator-driven rotation.
    private void ApplyLook(Transform bone, Vector3 target, float weight, float yawLim, float pitchLim)
    {
        Vector3 dir = target - bone.position;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        // World rotation that would make headForwardAxis point at dir.
        // Quaternion.LookRotation gives the rotation for +Z; we factor out the
        // bone's own axis offset via the correction quaternion.
        Quaternion axisCorrection = Quaternion.Inverse(
            Quaternion.LookRotation(headForwardAxis.normalized, headUpAxis.normalized));
        Quaternion desiredWorld = Quaternion.LookRotation(dir, Vector3.up) * axisCorrection;

        // Clamp angles in parent-local space so limits stay relative to the
        // rig's current animated pose (not world axes).
        Transform par = bone.parent;
        if (par != null)
        {
            Quaternion localDes = Quaternion.Inverse(par.rotation) * desiredWorld;
            Vector3 e = localDes.eulerAngles;
            // Remap 0-360 to -180..180
            if (e.x > 180f) e.x -= 360f;
            if (e.y > 180f) e.y -= 360f;
            e.x = Mathf.Clamp(e.x, -pitchLim, pitchLim);
            e.y = Mathf.Clamp(e.y, -yawLim, yawLim);
            e.z = 0f; // never roll the head
            desiredWorld = par.rotation * Quaternion.Euler(e);
        }

        // Blend from the animator-set rotation to our desired rotation.
        bone.rotation = Quaternion.Slerp(bone.rotation, desiredWorld, weight);
    }

    private VRCPlayerApi FindNearest()
    {
        VRCPlayerApi[] arr = new VRCPlayerApi[80];
        arr = VRCPlayerApi.GetPlayers(arr);
        VRCPlayerApi best = null;
        float bestSq = trackDistance * trackDistance;
        Vector3 pos = dogRoot.position;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null || !arr[i].IsValid()) continue;
            float sq = (arr[i].GetPosition() - pos).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = arr[i]; }
        }
        return best;
    }

    private void ScheduleGlance()
    {
        if (glanceInterval <= 0f) return;
        nextGlance = Time.time + glanceInterval
                     + Random.Range(-glanceInterval * 0.25f, glanceInterval * 0.25f);
    }
}
