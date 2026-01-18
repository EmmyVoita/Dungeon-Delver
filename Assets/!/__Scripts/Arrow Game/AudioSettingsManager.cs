using UnityEngine;
using System.IO;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    public AudioMixer mixer;

    [Header("Mixer Groups")]
    public AudioMixerGroup masterGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup arrowHitsGroup;
    public AudioMixerGroup uiGroup;
    public AudioMixerGroup ambienceGroup;

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;
    [Range(0f, 1f)] public float ambienceVolume = 1f;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip navigateSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip backSound;
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip negativeUISound;
    [SerializeField] private AudioClip tallySound;
    [SerializeField] private AudioClip accentTallySound;
    [SerializeField] private AudioClip arrowHitSound;

    private string savePath;

    [System.Serializable]
    private class AudioSettingsData
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float uiVolume;
        public float ambienceVolume;
    }

    // ---------------------------
    // Initialization
    // ---------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "audioSettings.json");
        LoadSettings();
    }

    public AudioMixerGroup GetMixerGroup(AudioChannel channel)
    {
        return channel switch
        {
            AudioChannel.Music => musicGroup,
            AudioChannel.SFX => sfxGroup,
            AudioChannel.UI => uiGroup,
            AudioChannel.Ambience => ambienceGroup,
            _ => masterGroup
        };
    }


    // ---------------------------
    // Volume Controls
    // ---------------------------
    public void SetVolume(AudioChannel channel, float volume)
    {
        volume = Mathf.Clamp01(volume);

        switch (channel)
        {
            case AudioChannel.Master:
                masterVolume = volume;
                break;
            case AudioChannel.Music:
                musicVolume = volume;
                break;
            case AudioChannel.SFX:
                sfxVolume = volume;
                break;
            case AudioChannel.UI:
                uiVolume = volume;
                break;
            case AudioChannel.Ambience:
                ambienceVolume = volume;
                break;
        }

        SaveSettings(); // auto-save when changed
    }

    public static float GetVolume(AudioChannel channel)
    {
        if (Instance == null) return 1f;

        float channelVolume = channel switch
        {
            AudioChannel.Music => Instance.musicVolume,
            AudioChannel.SFX => Instance.sfxVolume,
            AudioChannel.UI => Instance.uiVolume,
            AudioChannel.Ambience => Instance.ambienceVolume,
            _ => 1f
        };

        return Instance.masterVolume * channelVolume;
    }

    // ---------------------------
    // Audio Feedback
    // ---------------------------
    public static void PlayNavigateSound(float pitch = 1f)
    {
        if (Instance && Instance.navigateSound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.navigateSound, AudioChannel.UI,
                Camera.main.transform.position, 1.0f, pitch
            );
    }

    public static void PlaySelectSound(float pitch = 1f)
    {
        if (Instance && Instance.selectSound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.selectSound, AudioChannel.UI,
                Camera.main.transform.position, 1.0f, pitch
            );
    }

    public static void PlayBackSound(float pitch = 1f)
    {
        if (Instance && Instance.backSound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.backSound, AudioChannel.UI,
                Camera.main.transform.position, 1.0f, pitch
            );
    }

    public static void PlayGeneralButtonSound(float pitch = 1f)
    {
        if (Instance && Instance.buttonSound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.buttonSound, AudioChannel.UI,
                Camera.main.transform.position, 1.0f, pitch
            );
    }

    public static void PlayNegativeUISound(float pitch = 1f)
    {
        if (Instance && Instance.negativeUISound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.negativeUISound, AudioChannel.UI,
                Camera.main.transform.position, 1.0f, pitch
            );
    }

    public static void PlayTallySound(float pitch = 1f, float volume = 1f)
    {
        if (Instance && Instance.tallySound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.tallySound, AudioChannel.UI,
                Camera.main.transform.position, volume, pitch
            );
    }

    public static void PlayAccentTallySound(float pitch = 1f, float volume = 1f)
    {
        if (Instance && Instance.accentTallySound)
            AudioHelpers.PlayMyClipAtPoint(
                Instance.accentTallySound, AudioChannel.UI,
                Camera.main.transform.position, volume, pitch
            );
    }

    public void PlayArrowHitSound(float pitch = 1f)
    {
        if (arrowHitSound)
            AudioHelpers.PlayMyClipAtPoint(
                arrowHitSound, AudioChannel.SFX,
                Camera.main.transform.position, 1.0f, pitch
            );
    }

    // ---------------------------
    // Save / Load
    // ---------------------------
    private void SaveSettings()
    {
        AudioSettingsData data = new AudioSettingsData
        {
            masterVolume = masterVolume,
            musicVolume = musicVolume,
            sfxVolume = sfxVolume,
            uiVolume = uiVolume,
            ambienceVolume = ambienceVolume
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        // Debug.Log($"💾 Audio settings saved to {savePath}");
    }

    private void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            AudioSettingsData data = JsonUtility.FromJson<AudioSettingsData>(json);

            masterVolume = data.masterVolume;
            musicVolume = data.musicVolume;
            sfxVolume = data.sfxVolume;
            uiVolume = data.uiVolume;
            ambienceVolume = data.ambienceVolume;

            // Debug.Log($"🔊 Audio settings loaded from {savePath}");
        }
        else
        {
            // Default values
            masterVolume = 1f;
            musicVolume = 1f;
            sfxVolume = 1f;
            uiVolume = 1f;
            ambienceVolume = 1f;
            SaveSettings(); // create the file
        }
    }
}
