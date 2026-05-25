using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioClip LevelEditorTestMusic => TestSession.levelMusic;

    [Header("Music Source")]
    [SerializeField] private AudioSource mainSource;
    [SerializeField] public AudioClip mainClip;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float fadeInDuration = 0.35f;

    public bool IsMainPlaying => mainSource != null && mainSource.isPlaying;
    public float MainVolume => mainSource != null ? mainSource.volume : 0f;
    public double RawDSPTime => AudioSettings.dspTime - RoundManager.Instance.RoundStartDSP;
    public double ScaledElapsedTime => _scaledTime;

    private double _scaledTime;
    private double _lastDSPTime;

    private Tween fadeTween;
    private Coroutine startRoutine;

    private bool isPaused = false;

    // ----------------------------------------------------
    // Unity Lifecycle
    // ----------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (GameSessionBootstrap.Config != null &&
            GameSessionBootstrap.Config.Mode == GameMode.LevelEditorTest)
        {
            mainClip = LevelEditorTestMusic;
        }

        mainSource.playOnAwake = false;
        mainSource.volume = 0f;
        mainSource.clip = mainClip;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        TimeManager.OnTimeScaleChanged += HandleTimeScaleChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        OverlayManager.OnOverlayChanged += HandleOverlayChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        TimeManager.OnTimeScaleChanged -= HandleTimeScaleChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        OverlayManager.OnOverlayChanged -= HandleOverlayChanged;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetTiming();
    }

    private void HandleOverlayChanged(OverlayState previousState, OverlayState newState)
    {
        if (newState ==  OverlayState.Pause)
        {
            PauseMusic();
            return;
        }

        if (previousState == OverlayState.Pause)
        {
            ResumeMusic();
            return;
        }
    }

    // ----------------------------------------------------
    // State Handling
    // ----------------------------------------------------

    private void HandleStateChanged(GameState previous, GameState newState)
    {
        

        if (newState == GameState.RoundActive)
        {
            if (startRoutine != null)
                StopCoroutine(startRoutine);

            startRoutine = StartCoroutine(StartMusicRoutine());
        }
        else if (newState == GameState.RoundResultsTally)
        {
            FadeOutAndStop();
        }
        else
        {
            StopImmediate();
        }
    }

    private void HandleTimeScaleChanged(float scale)
    {
        if (mainSource != null)
            mainSource.pitch = scale;
    }

    void Update()
    {
        double currentDSP = AudioSettings.dspTime;

        // 🔒 Prevent large jumps when paused
        if (isPaused)
        {
            _lastDSPTime = currentDSP;
            return;
        }

        double dspDelta = currentDSP - _lastDSPTime;
        _lastDSPTime = currentDSP;

        float scale = TimeManager.Instance.GetCurrentScale();

        // 🎯 THIS is the important line
        _scaledTime += dspDelta * scale;
    }

    // ----------------------------------------------------
    // Deterministic DSP Start
    // ----------------------------------------------------

    private IEnumerator StartMusicRoutine()
    {
        ResetTiming();

        _lastDSPTime = AudioSettings.dspTime;
        

        fadeTween?.Kill();

        mainSource.Stop();
        //mainSource.enabled = false;
        //mainSource.enabled = true;

        float editorOffset = GameSessionBootstrap.Config.LevelEditorStartTime;

        _scaledTime = editorOffset;

        mainSource.clip = mainClip;
        mainSource.time = editorOffset;
        mainSource.volume = 0f;
        mainSource.pitch = TimeManager.Instance.GetCurrentScale();

        mainClip.LoadAudioData();

        while (mainClip.loadState != AudioDataLoadState.Loaded)
        {
            yield return null;
        }

        mainSource.PlayScheduled(RoundManager.Instance.RoundStartDSP);

        fadeTween = DOTween.To(
            () => mainSource.volume,
            v => mainSource.volume = v,
            AudioSettingsManager.Instance.musicVolume,
            fadeInDuration
        );
    }

    private void FadeOutAndStop()
    {
        if (!IsMainPlaying)
            return;

        fadeTween?.Kill();

        fadeTween = DOTween.To(
            () => mainSource.volume,
            v => mainSource.volume = v,
            0f,
            fadeOutDuration
        ).OnComplete(StopImmediate);
    }

    private void StopImmediate()
    {
        fadeTween?.Kill();

        if (startRoutine != null)
            StopCoroutine(startRoutine);

        mainSource.Stop();
        mainSource.volume = 0f;
    }

    public void PauseMusic()
    {
        if (isPaused || !mainSource.isPlaying)
            return;

        isPaused = true;
        mainSource.Pause();
    }

    public void ResumeMusic()
    {
        if (!isPaused)
            return;

        isPaused = false;
        mainSource.UnPause();
    }

    public void ResetTiming()
    {
        _scaledTime = 0;
        _lastDSPTime = AudioSettings.dspTime;
        isPaused = false;
    }


    public void SetMainVolume(float volume)
    {
        if (mainSource != null)
            mainSource.volume = volume;
    }
}

