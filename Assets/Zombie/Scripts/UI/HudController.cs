using UdonSharp;
using UnityEngine;
using TMPro;

// Pure presentation: every update is pushed in from GameManager / WaveManager
// / PlayerHealthManager. Wire the TextMeshPro fields and panels in the
// Inspector; this script has no game logic of its own.
public class HudController : UdonSharpBehaviour
{
    [Header("References")]
    public GameManager gameManager;

    [Header("Text Fields")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI zombiesRemainingText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI healthText;

    [Header("Panels")]
    public GameObject lobbyPanel;
    public GameObject playingPanel;
    public GameObject victoryPanel;

    public void OnGameStateChanged(int state)
    {
        if (lobbyPanel != null) lobbyPanel.SetActive(state == GameManager.STATE_LOBBY || state == GameManager.STATE_COUNTDOWN);
        if (playingPanel != null) playingPanel.SetActive(state == GameManager.STATE_PLAYING);
        if (victoryPanel != null) victoryPanel.SetActive(state == GameManager.STATE_VICTORY);

        if (stateText != null)
        {
            if (state == GameManager.STATE_LOBBY) stateText.text = "Waiting to start...";
            else if (state == GameManager.STATE_COUNTDOWN) stateText.text = "Get ready!";
            else if (state == GameManager.STATE_PLAYING) stateText.text = "Fight!";
            else if (state == GameManager.STATE_VICTORY) stateText.text = "Victory!";
        }
    }

    public void OnWaveStarted(int waveIndex, WaveConfig wave, int loopCount)
    {
        if (waveText == null) return;
        string label = wave != null ? wave.waveLabel : ("Wave " + (waveIndex + 1));
        if (loopCount > 0) label = label + " (+" + loopCount + ")";
        waveText.text = label;
    }

    public void OnZombiesRemainingChanged(int remaining)
    {
        if (zombiesRemainingText != null) zombiesRemainingText.text = "Zombies: " + remaining;
    }

    public void OnLocalHealthChanged(float current, float max)
    {
        if (healthText != null) healthText.text = "HP: " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }

    void Update()
    {
        if (gameManager == null || countdownText == null) return;

        if (gameManager.GetState() == GameManager.STATE_COUNTDOWN)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.CeilToInt(Mathf.Max(0f, gameManager.GetCountdownRemaining())).ToString();
        }
        else
        {
            countdownText.gameObject.SetActive(false);
        }
    }
}
