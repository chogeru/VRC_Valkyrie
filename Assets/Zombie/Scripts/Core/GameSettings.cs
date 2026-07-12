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
}
