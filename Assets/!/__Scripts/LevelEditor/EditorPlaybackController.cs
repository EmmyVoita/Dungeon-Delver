using UnityEngine;
using System.Collections.Generic;
using System;

using System.Collections;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorPlaybackController : MonoBehaviour
{
    public static EditorPlaybackController Instance { get; private set; }

    [Header("References")]
    public ArrowSpawner arrowSpawner;     
    public TimelineView timelineView;    

    [SerializeField] private float defaultZoom = 0.5f;


    [Header("Playback State")]
    public bool isPlaying = false;
    public float playSpeed = 1f;          // playback speed multiplier (1x, 2x, etc.)

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    
    

    
    private bool loopToolActive;
    private float loopStartTime;
    private float loopEndTime;
    private AudioClip _editorMusic;
    private float currentTime = 0f;        // current playback time in SECONDS           
    private List<EditorSimulatedArrow> simulatedArrows = new();

    public AudioClip LevelEditorTestMusic => _editorMusic;
    public float SpawnDistance => arrowSpawner.SpawnDistance;
    public float CurrentTime => currentTime;
    public bool SuppressSimulationAudio { get; private set; }

    void OnEnable()
    {
        TimeManager.OnTimeScaleChanged += HandleTimeScaleChanged;
    }

    void OnDisable()
    {
        TimeManager.OnTimeScaleChanged -= HandleTimeScaleChanged;
    }

    private void HandleTimeScaleChanged(float scale)
    {
        if (musicSource != null)
            musicSource.pitch = scale;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


    public void AssignLoop(float startTime, float endTime)
    {
        loopStartTime = startTime;
        loopEndTime = endTime;
        loopToolActive = true;
    }

    public void ClearLoopTool()
    {
        loopStartTime = 0;
        loopEndTime = 0;
        loopToolActive = false;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        musicSource.clip = _editorMusic;
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        if(TestSession.levelMusic != null)
            _editorMusic = TestSession.levelMusic;
    }

    void Start()
    {
        TimeManager.Instance.ClearAllModifiers();

        if(TestSession.levelMusic != null)
        {
            SetEditorMusic(TestSession.levelMusic);
        }
    }


    // Update – drive playback forward when playing
    // ---------------------------------------------------------
    void Update()
    {
        if (!isPlaying)
            return;

        currentTime += Time.unscaledDeltaTime * TimeManager.Instance.GetCurrentScale();

        if (loopToolActive && currentTime >= loopEndTime)
        {
            currentTime = loopStartTime;

            SyncSimulationToTime();
            SyncMusicToTime();
        }
        else if (!loopToolActive && currentTime >= LevelEditorData.Instance.MaxBeatTime)
        {
            currentTime = 0f;

            SyncSimulationToTime();
            SyncMusicToTime();
        }

        foreach (var arrow in simulatedArrows)
            arrow.Simulate(currentTime);
    }

    private void SyncSimulationToTime()
    {
        foreach (var sim in simulatedArrows)
            sim.Simulate(currentTime);
    }

    public void SelectMusicFile()
    {
    #if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel(
            "Select Level Music",
            "",
            "wav,mp3,ogg"
        );

        if (string.IsNullOrEmpty(path))
            return;

        StartCoroutine(LoadMusicFile(path));
    #else
        Debug.LogWarning(
            "The built-in file picker is only available inside the Unity Editor."
        );
    #endif
    }

    private IEnumerator LoadMusicFile(string path)
    {
        AudioType audioType = GetAudioType(path);

        if (audioType == AudioType.UNKNOWN)
        {
            Debug.LogError($"Unsupported audio file: {path}");
            yield break;
        }

        string url = "file://" + path;

        using UnityWebRequest request =UnityWebRequestMultimedia.GetAudioClip(url,audioType);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load music: {request.error}");

            yield break;
        }

        AudioClip clip =DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            Debug.LogError("Loaded audio clip was null.");

            yield break;
        }

        SetEditorMusic(clip);

        Debug.Log($"Loaded level editor music: {clip.name}");
    }

    private AudioType GetAudioType(string path)
    {
        string extension =System.IO.Path.GetExtension(path).ToLowerInvariant();

        switch (extension)
        {
            case ".wav":
                return AudioType.WAV;

            case ".mp3":
                return AudioType.MPEG;

            case ".ogg":
                return AudioType.OGGVORBIS;

            default:
                return AudioType.UNKNOWN;
        }
    }

    private void SetEditorMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        bool wasPlaying = isPlaying;

        musicSource.Stop();

        _editorMusic = clip;
        musicSource.clip = clip;

        // Make sure our current timeline position is valid
        // for the newly loaded song.
        currentTime = Mathf.Clamp(
            currentTime,
            0f,
            Mathf.Max(0f, clip.length - 0.01f)
        );

        SyncMusicToTime();

        if (wasPlaying)
        {
            musicSource.pitch = TimeManager.Instance.GetCurrentScale();
            musicSource.Play();
        }
    }


    // Build / rebuild simulation arrows 
    // ---------------------------------------------------------

    public void RebuildSimulation()
    {
        SuppressSimulationAudio = true;

        BuildSimulatedArrows();
        JumpToTime(currentTime);

        SuppressSimulationAudio = false;
    }


    public void BuildSimulatedArrows()
    {
        // Destroy previous arrows
        foreach (var a in simulatedArrows)
            if (a != null) Destroy(a.gameObject);
        simulatedArrows.Clear();

        // Convert → simulation events
        List<ArrowEvent> simEvents = LevelEditorData.Instance.ConvertToSimulatedEvents(SpawnDistance);

        foreach (var evt in simEvents)
        {
            // Look up ScriptableObject by type name (case-insensitive)
            ArrowTypeDefinition typeDef = arrowSpawner.ArrowTypeDefinitions
                .Find(t => t.displayName.Equals(evt.nameOfGameObjectToSpawn, StringComparison.OrdinalIgnoreCase));

            if (typeDef == null)
            {
                Debug.LogError($"❌ ArrowType '{evt.nameOfGameObjectToSpawn}' not found in ArrowSpawner.");
                continue;
            }

            GameObject prefab = typeDef.prefab;
            if (prefab == null)
            {
                Debug.LogError($"❌ ArrowType '{evt.nameOfGameObjectToSpawn}' has no prefab assigned.");
                continue;
            }

            // Instantiate simulated arrow object
            GameObject arrow = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            // Disable collisions inside editor
            Collider2D col = arrow.GetComponent<Collider2D>();
            if (col) col.enabled = false;

            // Orient arrow visually
            ArrowBase arrowBase = arrow.GetComponent<ArrowBase>();
            
            arrowBase.OrientArrow(evt.direction);
            arrowBase.SetToEditorArrow();

            // Add simulation component
            EditorSimulatedArrow sim = arrow.AddComponent<EditorSimulatedArrow>();
            sim.Init(evt, SpawnDistance);

            // stays hidden until spawnTime
            arrow.SetActive(false); 

            simulatedArrows.Add(sim);
        }
    }


    
    // Playback Controls
    // ---------------------------------------------------------
    
    // time is in seconds
    public void JumpToTime(float t)
    {
        SuppressSimulationAudio = true;

        // Clamp to TIMELINE length, not music length
        currentTime = Mathf.Clamp(
            t,
            0f,
            LevelEditorData.Instance.MaxSecondsTime
        );

        foreach (var sim in simulatedArrows)
            sim.Simulate(currentTime);

        // Handles looping music
        SyncMusicToTime();

        SuppressSimulationAudio = false;

        LevelTimelineUI.Instance.ScrollToTime(currentTime);
    }

    public void Play()
    {
        isPlaying = true;

        if (_editorMusic == null)
            return;

        musicSource.time = currentTime;
        musicSource.pitch = TimeManager.Instance.GetCurrentScale();
        SyncMusicToTime();
        musicSource.Play();
    }

    public void SyncMusicToTime()
    {
        if (musicSource == null ||
            musicSource.clip == null ||
            musicSource.clip.length <= 0f)
            return;

        float offset = LevelEditorData.Instance.SongOffsetSeconds;

        float targetTime = currentTime + offset;

        // Properly wrap into 0 -> clip.length
        targetTime = Mathf.Repeat(targetTime, musicSource.clip.length);

        UIToast.Show(
            $"SYNC MUSIC | Timeline: {currentTime:F3}" +
            $"\nClip Length: {musicSource.clip.length:F3}" +
            $"\nMusic Time: {targetTime:F3}",
            0.1f
        );

        musicSource.time = targetTime;
    }


    public void Pause()
    {
        isPlaying = false;
        musicSource.Pause();
    }

    public void Stop()
    {
        isPlaying = false;

        if (musicSource.isPlaying)
            musicSource.Stop();

        currentTime = 0f;

        foreach (var sim in simulatedArrows)
            sim.Simulate(currentTime);
    }
}
