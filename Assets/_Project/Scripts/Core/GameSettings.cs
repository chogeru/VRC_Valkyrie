using UdonSharp;
using UnityEngine;

// Central tuning hub for the whole zombie game. Every other script reads
// its numbers from here instead of hard-coding them, so the entire game's
// pacing/difficulty/locations can be adjusted from one Inspector.
public class GameSettings : UdonSharpBehaviour
{
    [Header("Waves (in play order)")]
    public WaveConfig[] waves;
    public bool loopFinalWave = false;
    public float finalWaveLoopHealthStep = 0.15f;

    [Header("Player")]
    public float playerMaxHealth = 100f;
    public float respawnDelay = 5f;
    public bool friendlyFireEnabled = false;
    [Tooltip("Must be the same prefab registered in the VRC Scene Descriptor's Player Objects list.")]
    public GameObject playerHealthObjectPrefab;

    [Header("Flow Timing")]
    public float startCountdownSeconds = 5f;
    public float victoryDisplayTime = 15f;

    [Header("Locations")]
    public Transform[] lobbySpawnPoints;
    public Transform[] battleSpawnPoints;
    public Transform[] playerRespawnPoints;
    public Transform[] zombieSpawnPoints;

    // Editor-only Scene view aid - never runs in the uploaded world (Udon
    // ignores OnDrawGizmos). Color-codes every spawn-point array so it's
    // obvious at a glance which Transform belongs to which list.
    private static readonly Color LobbyColor = new Color(0.2f, 0.6f, 1f);
    private static readonly Color BattleColor = new Color(1f, 0.25f, 0.25f);
    private static readonly Color RespawnColor = new Color(0.3f, 1f, 0.3f);
    private static readonly Color ZombieSpawnColor = new Color(1f, 0.6f, 0f);

    private void OnDrawGizmos()
    {
        DrawSpawnGizmos(lobbySpawnPoints, LobbyColor);
        DrawSpawnGizmos(battleSpawnPoints, BattleColor);
        DrawSpawnGizmos(playerRespawnPoints, RespawnColor);
        DrawSpawnGizmos(zombieSpawnPoints, ZombieSpawnColor);
    }

    private void DrawSpawnGizmos(Transform[] points, Color color)
    {
        if (points == null) return;
        Gizmos.color = color;
        foreach (Transform t in points)
        {
            if (t == null) continue;
            Gizmos.DrawWireSphere(t.position, 0.3f);
            Gizmos.DrawLine(t.position, t.position + t.forward * 0.6f); // facing direction
        }
    }
}
