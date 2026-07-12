using UdonSharp;
using UnityEngine;

// One entry per wave. Build a list of these under GameSettings.waves to
// script the whole game's difficulty curve without touching any code.
public class WaveConfig : UdonSharpBehaviour
{
    [Header("Identity")]
    public string waveLabel = "Wave 1";

    [Header("Spawns")]
    public int zombieCount = 10;
    public float spawnInterval = 1.5f;

    [Header("Difficulty Scaling")]
    public float healthMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;

    [Header("Pacing")]
    public float intermissionAfterWave = 10f;
}
