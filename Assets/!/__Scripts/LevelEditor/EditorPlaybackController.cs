using UnityEngine;
using System.Collections.Generic;
using System;

public class EditorPlaybackController : MonoBehaviour
{
    public static EditorPlaybackController Instance { get; private set; }

    [Header("References")]
    public ArrowSpawner arrowSpawner;     


    [Header("Playback State")]
    public bool isPlaying = false;
    public float playSpeed = 1f;          // playback speed multiplier (1x, 2x, etc.)

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip editorMusic;

    public AudioClip LevelEditorTestMusic => editorMusic;



    private float currentTime = 0f;        // current playback time in SECONDS           
    private List<EditorSimulatedArrow> simulatedArrows = new();


    public float SpawnDistance => arrowSpawner.SpawnDistance;
    public float CurrentTime => currentTime;
    public bool SuppressSimulationAudio { get; private set; }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        musicSource.clip = editorMusic;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }


    // Update – drive playback forward when playing
    // ---------------------------------------------------------
    void Update()
    {
        Time.timeScale = 1.0f;
        if (!isPlaying)
            return;

        currentTime += Time.deltaTime * playSpeed;

        // Clamp to end of timeline
        if (currentTime > LevelEditorData.Instance.MaxTime)
        {
            currentTime = 0f;

            // resimulate arrows at start
            foreach (var sim in simulatedArrows)
                sim.Simulate(currentTime);

            SyncMusicToTime();
        }

        // Simulate arrow movement
        foreach (var arrow in simulatedArrows)
            arrow.Simulate(currentTime);

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
    
    public void JumpToTime(float t)
    {
        SuppressSimulationAudio = true;

        currentTime = Mathf.Clamp(t, 0f, LevelEditorData.Instance.MaxTime);

        foreach (var sim in simulatedArrows)
            sim.Simulate(currentTime);

        Debug.Log($"Clip? {musicSource.clip}, Length: {(musicSource.clip ? musicSource.clip.length : -1)}");

        if (musicSource != null && musicSource.clip != null && musicSource.clip.length > 0f)
        {
            float safeTime = Mathf.Clamp(currentTime, 0f, musicSource.clip.length - 0.01f);
            musicSource.time = safeTime;
        }

        SuppressSimulationAudio = false;

        //LevelTimelineUI.Instance.SetScrollRectPos(currentTime);

        float x = LevelTimelineUI.Instance.TimelineView.TimeToPixels(currentTime);

        float normalized = x / Mathf.Max(1f, LevelTimelineUI.Instance.content.sizeDelta.x);
        LevelTimelineUI.Instance.scrollRect.horizontalNormalizedPosition = normalized;
    }

    public void Play()
    {
        isPlaying = true;
        if (editorMusic != null)
        {
            musicSource.time = currentTime;
            musicSource.pitch = playSpeed;
            SyncMusicToTime();
            musicSource.Play();
        }
    }

    public void SyncMusicToTime()
    {
        if (musicSource == null || musicSource.clip == null || musicSource.clip.length <= 0f)
            return;

        float offset = LevelEditorData.Instance.SongOffsetSeconds;
        float targetTime = Mathf.Max(0f, currentTime + offset);
        targetTime %= musicSource.clip.length;

        float safeTime = Mathf.Clamp(targetTime, 0f, musicSource.clip.length - 0.01f);
        musicSource.time = safeTime;

        UIToast.Show($"safeTime -> {safeTime}");
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
