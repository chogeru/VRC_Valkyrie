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
    public const int STATE_GAMEOVER = 4;

    [Header("References")]
    public GameSettings settings;
    public WaveManager waveManager;
    public HudController hud;
    public AudioManager audioManager;

    [UdonSynced] private int gameState = STATE_LOBBY;
    [UdonSynced] private float countdownEndServerTime;

    private int lastAppliedState = -1;
    private float nextGameOverCheckTime;

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

        if (gameState == STATE_PLAYING && Networking.IsOwner(gameObject) && Time.time >= nextGameOverCheckTime)
        {
            nextGameOverCheckTime = Time.time + (settings != null ? settings.gameOverCheckInterval : 2f);
            CheckForGameOver();
        }
    }

    // A dead player is instantly healed and sent back to the lobby (see
    // PlayerHealthManager.RespawnLocalPlayer), so "everyone's dead" is only
    // ever a brief window right after the last survivor drops - this polls
    // for it instead of relying on a single damage event, since no one
    // script sees every player's health change in one place.
    private void CheckForGameOver()
    {
        if (settings == null || settings.playerDataRegistry == null) return;

        int claimed = settings.playerDataRegistry.CountClaimedSlots();
        if (claimed <= 0) return; // nobody has joined the fight yet, don't false-trigger

        int alive = settings.playerDataRegistry.CountAliveClaimedSlots();
        if (alive <= 0) TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (gameState != STATE_PLAYING) return;

        gameState = STATE_GAMEOVER;
        RequestSerialization();
        ApplyStateLocal();
        if (waveManager != null) waveManager.StopSpawning();
        SendCustomEventDelayedSeconds(nameof(ReturnToLobby), settings != null ? settings.gameOverDisplayTime : 10f);
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
        if (audioManager != null) audioManager.PlaySfx("CountdownStart");
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
        if (gameState != STATE_PLAYING) return; // e.g. a game-over already ended this round

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
            if (audioManager != null) audioManager.PlayMusic("Lobby");
        }
        else if (gameState == STATE_PLAYING)
        {
            if (local != null && settings != null && settings.battleSpawnPoints.Length > 0)
            {
                Transform sp = settings.battleSpawnPoints[Random.Range(0, settings.battleSpawnPoints.Length)];
                local.TeleportTo(sp.position, sp.rotation);
            }
            if (audioManager != null) audioManager.PlayMusic("Battle");
        }
        else if (gameState == STATE_VICTORY)
        {
            if (audioManager != null)
            {
                audioManager.PlayMusic("Victory");
                audioManager.PlaySfx("Victory");
            }
        }
        else if (gameState == STATE_GAMEOVER)
        {
            if (audioManager != null)
            {
                audioManager.PlayMusic("GameOver");
                audioManager.PlaySfx("GameOver");
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
