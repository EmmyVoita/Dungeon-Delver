using UnityEngine;
using System.IO;
using UnityEngine.Audio;
using System;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    public static event Action OnVolumeUpdated;

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
    public void SetVolume(AudioControl channel, float volume)
    {
        volume = Mathf.Clamp01(volume);

        switch (channel)
        {
            case AudioControl.Master:
                masterVolume = volume;
                break;
            case AudioControl.Music:
                musicVolume = volume;
                break;
            case AudioControl.SFX:
                sfxVolume = volume;
                break;
            case AudioControl.UI:
                uiVolume = volume;
                break;
            case AudioControl.Ambience:
                ambienceVolume = volume;
                break;
        }

        OnVolumeUpdated?.Invoke();

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
