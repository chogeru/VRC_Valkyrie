using UdonSharp;
using UnityEngine;

// Marker placed on empty child GameObjects positioned along the agility
// course (bridge crossing, jump gap by the wheel, loop around the ladder).
// DogAI reads a Transform[] of these in course order via its own
// agilityWaypoints/agilityIsJumpPoint arrays - this script only exists so
// designers can see/organize the waypoints as real objects in the
// hierarchy and flag which ones should trigger a jump animation.
public class AgilityWaypoint : UdonSharpBehaviour
{
    [Tooltip("Order in the course. DogAI sorts by this if waypoints are assigned out of order.")]
    public int order;
    [Tooltip("If true, DogAI fires the Jump animation trigger shortly before reaching this point.")]
    public bool isJumpPoint;

    private void OnDrawGizmos()
    {
        Gizmos.color = isJumpPoint ? Color.red : Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.15f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.75f, new Vector3(0.05f, 1.5f, 0.05f));
    }
}
