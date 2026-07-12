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
    [Tooltip("The scene's PlayerDataRegistry - looks up each player's PlayerHealthManager (health/score wallet).")]
    public PlayerDataRegistry playerDataRegistry;

    [Header("Flow Timing")]
    public float startCountdownSeconds = 5f;
    public float victoryDisplayTime = 15f;
    [Tooltip("How often (seconds) the master checks whether every claimed player slot has died, while a game is in progress.")]
    public float gameOverCheckInterval = 2f;
    public float gameOverDisplayTime = 10f;

    [Header("Locations")]
    [Tooltip("Also used as the respawn destination when a player dies (see PlayerHealthManager.RespawnLocalPlayer) - dying sends you back to the lobby.")]
    public Transform[] lobbySpawnPoints;
    public Transform[] battleSpawnPoints;
    public Transform[] zombieSpawnPoints;

    // Editor-only Scene view aid - never runs in the uploaded world (Udon
    // ignores OnDrawGizmos). Color-codes every spawn-point array so it's
    // obvious at a glance which Transform belongs to which list.
    private static readonly Color LobbyColor = new Color(0.2f, 0.6f, 1f);
    private static readonly Color BattleColor = new Color(1f, 0.25f, 0.25f);
    private static readonly Color ZombieSpawnColor = new Color(1f, 0.6f, 0f);

    private void OnDrawGizmos()
    {
        DrawSpawnGizmos(lobbySpawnPoints, LobbyColor);
        DrawSpawnGizmos(battleSpawnPoints, BattleColor);
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
