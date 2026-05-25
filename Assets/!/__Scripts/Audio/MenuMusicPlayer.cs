using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicPlayer : MonoBehaviour
{
    public static MenuMusicPlayer Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private List<GameState> targetStates;
    [SerializeField] private bool playOnStart = true;

    [Header("References")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip menuMusic;

    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;

    private Tween fadeTween;

    private void Awake()
    {
        // ------------------------------------------------
        // Singleton / Persist Between Scenes
        // ------------------------------------------------

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        AudioSettingsManager.OnVolumeUpdated += HandleVolumeUpdated;
        GameStateManager.OnStateChanged += HandleStateChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        AudioSettingsManager.OnVolumeUpdated -= HandleVolumeUpdated;
        GameStateManager.OnStateChanged -= HandleStateChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayMenuMusic();
        }
    }

    private void HandleVolumeUpdated()
    {
        if (source != null)
        {
            source.volume = AudioSettingsManager.Instance.musicVolume;
        }
    }

    private void HandleStateChanged(GameState previousState,GameState newState)
    {
        if (targetStates.Contains(newState))
        {
            PlayMenuMusic();
        }
        else if(newState != GameState.None)
        {
            FadeOutAndStop();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        if(scene.name == SceneNames.MainMenu)
        {
            PlayMenuMusic();
        }
        
    }

    public void PlayMenuMusic()
    {
        if (source == null || menuMusic == null)
            return;

        Debug.Log("Playing Menu Music");

        // Prevent restarting same track repeatedly
        if (source.isPlaying && source.clip == menuMusic)
            return;

        source.clip = menuMusic;
        source.loop = true;
        //source.volume = AudioSettingsManager.Instance.musicVolume;

        //source.Play();
        FadeIn();
    }

    private void FadeIn()
    {
        source.volume = 0f;

        source.Play();

        fadeTween?.Kill();

        fadeTween = DOTween.To(
            () => source.volume,
            v => source.volume = v,
            AudioSettingsManager.Instance.musicVolume,
            fadeInDuration
        );
    }


    private void FadeOutAndStop()
    {
        if (!source.isPlaying)
            return;

        

        fadeTween?.Kill();

        fadeTween = DOTween.To(
            () => source.volume,
            v => source.volume = v,
            0f,
            fadeOutDuration
        ).OnComplete(StopMusic);
    }

    public void StopMusic()
    {
        Debug.Log("Stopping Menu Music");

        if (source != null)
        {
            source.Stop();
        }
    }
}