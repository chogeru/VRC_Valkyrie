using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Master-authoritative game flow state machine (Lobby -> Countdown ->
// Playing -> Victory -> Lobby). Every client runs this same script; only
// the current instance master is allowed to mutate the synced state, and
// every client individually reacts to state changes (e.g. teleporting only
// its own local player).
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    public const int STATE_LOBBY = 0;
    public const int STATE_COUNTDOWN = 1;
    public const int STATE_PLAYING = 2;
    public const int STATE_VICTORY = 3;

    [Header("References")]
    public GameSettings settings;
    public WaveManager waveManager;
    public HudController hud;

    [UdonSynced] private int gameState = STATE_LOBBY;
    [UdonSynced] private float countdownEndServerTime;

    private int lastAppliedState = -1;

    void Start()
    {
        ApplyStateLocal();
    }

    void Update()
    {
        if (gameState != lastAppliedState)
        {
            ApplyStateLocal();
        }

        if (gameState == STATE_COUNTDOWN && Networking.IsOwner(gameObject))
        {
            if (GetServerTime() >= countdownEndServerTime)
            {
                BeginPlaying();
            }
        }
    }

    public override void OnDeserialization()
    {
        if (gameState != lastAppliedState)
        {
            ApplyStateLocal();
        }
    }

    // Invoked via SendCustomNetworkEvent(All) by GameStartButton on every
    // client; only the owner (the current master) actually acts on it.
    public void RequestStartGame()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (gameState != STATE_LOBBY) return;

        gameState = STATE_COUNTDOWN;
        countdownEndServerTime = GetServerTime() + settings.startCountdownSeconds;
        RequestSerialization();
        ApplyStateLocal();
    }

    private void BeginPlaying()
    {
        gameState = STATE_PLAYING;
        RequestSerialization();
        ApplyStateLocal();
        if (waveManager != null) waveManager.BeginWaves();
    }

    // Called by WaveManager once every configured wave has been cleared.
    public void NotifyVictory()
    {
        if (!Networking.IsOwner(gameObject)) return;
        gameState = STATE_VICTORY;
        RequestSerialization();
        ApplyStateLocal();
        SendCustomEventDelayedSeconds(nameof(ReturnToLobby), settings.victoryDisplayTime);
    }

    public void ReturnToLobby()
    {
        if (!Networking.IsOwner(gameObject)) return;
        gameState = STATE_LOBBY;
        RequestSerialization();
        ApplyStateLocal();
    }

    // Runs independently on every client: moves only the local player and
    // refreshes local HUD/panel visibility.
    private void ApplyStateLocal()
    {
        lastAppliedState = gameState;
        VRCPlayerApi local = Networking.LocalPlayer;

        if (gameState == STATE_LOBBY)
        {
            if (local != null && settings != null && settings.lobbySpawnPoints.Length > 0)
            {
                Transform sp = settings.lobbySpawnPoints[Random.Range(0, settings.lobbySpawnPoints.Length)];
                local.TeleportTo(sp.position, sp.rotation);
            }
        }
        else if (gameState == STATE_PLAYING)
        {
            if (local != null && settings != null && settings.battleSpawnPoints.Length > 0)
            {
                Transform sp = settings.battleSpawnPoints[Random.Range(0, settings.battleSpawnPoints.Length)];
                local.TeleportTo(sp.position, sp.rotation);
            }
        }

        if (hud != null) hud.OnGameStateChanged(gameState);
    }

    private float GetServerTime()
    {
        return Networking.GetServerTimeInMilliseconds() / 1000f;
    }

    public int GetState() { return gameState; }

    public float GetCountdownRemaining() { return countdownEndServerTime - GetServerTime(); }
}
