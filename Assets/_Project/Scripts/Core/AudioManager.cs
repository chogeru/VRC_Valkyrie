using UdonSharp;
using UnityEngine;

// Central audio hub for BGM and global (non-spatial) SFX - e.g. wave-start
// stingers, victory fanfare, player-hurt cues. Per-object spatial sounds
// (gun fire/reload, zombie voice lines) stay on their own AudioSources and
// don't go through here.
//
// To add a new music track or SFX: just add one row to the matching
// Name/Clip array pair in the Inspector. No script changes needed - call
// PlayMusic("YourTrackName") or PlaySfx("YourSfxName") from anywhere with
// a reference to this component.
public class AudioManager : UdonSharpBehaviour
{
    [Header("Music (looping, crossfades between tracks)")]
    public AudioSource musicSourceA;
    public AudioSource musicSourceB;
    public float musicFadeDuration = 1.5f;
    [Tooltip("Parallel arrays: musicNames[i] plays musicClips[i]. Add rows here for new tracks.")]
    public string[] musicNames;
    public AudioClip[] musicClips;

    [Header("Auto-Play")]
    [Tooltip("Track name to play automatically on Start. Leave empty to disable auto-play.")]
    public string autoPlayMusicName;

    [Header("SFX (one-shot, global game-flow cues)")]
    public AudioSource sfxSource;
    [Tooltip("Parallel arrays: sfxNames[i] plays sfxClips[i]. Add rows here for new SFX.")]
    public string[] sfxNames;
    public AudioClip[] sfxClips;

    private AudioSource activeMusicSource;
    private AudioSource previousMusicSource;
    private bool fading;
    private float fadeStartTime;
    private string currentMusicName = "";

    void Start()
    {
        activeMusicSource = musicSourceA;
        previousMusicSource = musicSourceB;

        // BGM must play clean - no reverb filter. Remove any AudioReverbFilter
        // that may have been accidentally added to either music source GameObject.
        RemoveReverbFilter(musicSourceA);
        RemoveReverbFilter(musicSourceB);

        if (!string.IsNullOrEmpty(autoPlayMusicName)) PlayMusic(autoPlayMusicName);
    }

    private void RemoveReverbFilter(AudioSource src)
    {
        if (src == null) return;
        AudioReverbFilter f = src.GetComponent<AudioReverbFilter>();
        if (f != null)
        {
            f.enabled = false;
            Debug.LogWarning("[AudioManager] Disabled AudioReverbFilter on BGM source '" + src.gameObject.name + "'. BGM should play clean - remove the component from the GameObject.");
        }
    }

    public void PlayMusic(string trackName)
    {
        if (string.IsNullOrEmpty(trackName) || trackName == currentMusicName) return;

        AudioClip clip = FindClip(musicNames, musicClips, trackName);
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] No music clip named '" + trackName + "' - add it to musicNames/musicClips.");
            return;
        }

        currentMusicName = trackName;

        AudioSource newActive = previousMusicSource;
        previousMusicSource = activeMusicSource;
        activeMusicSource = newActive;

        activeMusicSource.clip = clip;
        activeMusicSource.volume = 0f;
        activeMusicSource.loop = true;
        activeMusicSource.Play();

        fading = true;
        fadeStartTime = Time.time;
    }

    public void StopMusic()
    {
        currentMusicName = "";
        if (musicSourceA != null) musicSourceA.Stop();
        if (musicSourceB != null) musicSourceB.Stop();
        fading = false;
    }

    void Update()
    {
        if (!fading) return;

        float t = Mathf.Clamp01((Time.time - fadeStartTime) / Mathf.Max(0.01f, musicFadeDuration));
        activeMusicSource.volume = t;
        previousMusicSource.volume = 1f - t;

        if (t >= 1f)
        {
            fading = false;
            previousMusicSource.Stop();
            previousMusicSource.volume = 0f;
        }
    }

    public void PlaySfx(string sfxName)
    {
        AudioClip clip = FindClip(sfxNames, sfxClips, sfxName);
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] No SFX clip named '" + sfxName + "' - add it to sfxNames/sfxClips.");
            return;
        }
        if (sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    private AudioClip FindClip(string[] names, AudioClip[] clips, string wantedName)
    {
        if (names == null || clips == null) return null;
        int len = Mathf.Min(names.Length, clips.Length);
        for (int i = 0; i < len; i++)
        {
            if (names[i] == wantedName) return clips[i];
        }
        return null;
    }
}
