using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioClip LevelEditorTestMusic => TestSession.levelMusic;

    [Header("Music Source")]
    [SerializeField] private AudioSource mainSource;
    [SerializeField] public AudioClip mainClip;
    [SerializeField] private float mainTimeOffset = 0f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float fadeInDuration = 0.35f;

    public bool IsMainPlaying => mainSource != null && mainSource.isPlaying;
    public float MainVolume => mainSource != null ? mainSource.volume : 0f;

    private Tween fadeTween;
    private Coroutine startRoutine;
    private bool isPaused = false;
    private double pauseDSPTime;

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

        if (GameSceneLoader.PendingConfig != null &&
            (GameSceneLoader.PendingConfig.Mode == GameMode.LevelEditorTest ||
            GameSceneLoader.PendingConfig.Mode == GameMode.LevelEdtiorPlayFromPosition))
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
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        TimeManager.OnTimeScaleChanged -= HandleTimeScaleChanged;
    }

    // ----------------------------------------------------
    // State Handling
    // ----------------------------------------------------

    private void HandleStateChanged(GameState previous, GameState current)
    {
        if(previous == GameState.Paused || current == GameState.Paused) return;
        if (current == GameState.RoundActive && previous != GameState.RoundActive)
        {
            Debug.Log("Round started, scheduling music.");
            StartMusicAt();
        }
        else if (current == GameState.RoundResultsTally)
        {
            Debug.Log("Round ended, fading out music.");
            FadeOutAndStop();
        }
        else
        {
            Debug.Log($"State changed to {current}, stopping music.");
            StopImmediate();
        }
    }

    private void HandleTimeScaleChanged(float scale)
    {
        if (mainSource != null)
            mainSource.pitch = scale;
    }

    // ----------------------------------------------------
    // Deterministic DSP Start
    // ----------------------------------------------------

    public void StartMusicAt()
    {
        if (startRoutine != null)
            StopCoroutine(startRoutine);

        startRoutine = StartCoroutine(StartMusicRoutine());
    }

    private IEnumerator StartMusicRoutine()
    {
        fadeTween?.Kill();

        // ---- HARD VOICE RESET (frame 0) ----
        AudioSettings.Reset(AudioSettings.GetConfiguration());
        mainSource.Stop();
        mainSource.enabled = false;
        mainSource.enabled = true;

        float editorOffset = GameSceneLoader.PendingConfig != null ? GameSceneLoader.PendingConfig.levelEditorStartTime : 0;

        mainSource.clip = mainClip;
        mainSource.time = mainTimeOffset + editorOffset;
        mainSource.volume = 0f;
        mainSource.pitch = 1f;

        // ---- WAIT ONE FRAME (critical) ----
        yield return null;

        // ---- SCHEDULE (frame 1) ----
        UIToast.Show($"Scheduling music at DSP time {RoundManager.Instance.RoundStartDSP:F6}");
        mainSource.PlayScheduled(RoundManager.Instance.RoundStartDSP);
         // slight delay to ensure scheduling works
        //mainSource.Play();
        //StartCoroutine(CaptureTrueAudioStart());


        // ---- FADE IN ----
        fadeTween = DOTween.To(
            () => mainSource.volume,
            v => mainSource.volume = v,
            AudioSettingsManager.Instance.musicVolume,
            fadeInDuration
        );

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        StartCoroutine(DebugVerifyStart(RoundManager.Instance.RoundStartDSP));
#endif
    }

    private IEnumerator CaptureTrueAudioStart()
    {
        // Wait until Unity reports the source is playing AND has advanced samples
        int startSamples = mainSource.timeSamples;

        while (!mainSource.isPlaying || mainSource.timeSamples == startSamples)
            yield return null;

        RoundManager.Instance.RoundStartDSP = AudioSettings.dspTime;

        Debug.Log(
            $"🎯 TRUE audio start\n" +
            $"DSP = {AudioSettings.dspTime}\n" +
            $"samples = {mainSource.timeSamples}"
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
        pauseDSPTime = AudioSettings.dspTime;

        mainSource.Pause();
    }


    public void ResumeMusic()
    {
        if (!isPaused)
            return;

        double resumeDSP = AudioSettings.dspTime;
        double pausedDuration = resumeDSP - pauseDSPTime;

        RoundManager.Instance.RoundStartDSP += pausedDuration;

        isPaused = false;
        mainSource.UnPause();

        Debug.Log($"Resuming music. Adjusted RoundStartDSP by {pausedDuration:F6} seconds.");

        // 🔑 RESTORE VOLUME
        //fadeTween?.Kill();
        //mainSource.volume = AudioSettingsManager.Instance.musicVolume;
    }



    // ----------------------------------------------------
    // Debug (optional)
    // ----------------------------------------------------

    private IEnumerator DebugVerifyStart(double scheduledDSP)
    {
        while (AudioSettings.dspTime < scheduledDSP)
            yield return null;

        yield return null;

        int samples = mainSource.timeSamples;
        float seconds = samples / (float)mainSource.clip.frequency;

        UIToast.Show(
            $"🎵 MUSIC START\n" +
            $"DSP Now: {AudioSettings.dspTime:F6}\n" +
            $"Scheduled: {scheduledDSP:F6}\n" +
            $"Samples: {samples}\n" +
            $"Seconds: {seconds:F4}",
            5f
        );
    }


    public void SetMainVolume(float volume)
    {
        if (mainSource != null)
            mainSource.volume = volume;
    }
}
