using UnityEngine;
using System.Collections.Generic;
using System;

public class EditorPlaybackController : MonoBehaviour
{
    public static EditorPlaybackController Instance { get; private set; }

    [Header("References")]
    public ArrowSpawner arrowSpawner;     
    public LevelEditorData editorData;   


    [Header("Playback State")]
    public bool isPlaying = false;
    public float playSpeed = 1f;          // playback speed multiplier (1x, 2x, etc.)


    private float currentTime = 0f;        // current playback time in SECONDS           
    private List<EditorSimulatedArrow> simulatedArrows = new();


    public float SpawnDistance => arrowSpawner.SpawnDistance;
    public float CurrentTime => currentTime;
    public bool SuppressSimulationAudio { get; private set; }





    void Awake()
    {
        Instance = this;
    }


    // Update – drive playback forward when playing
    // ---------------------------------------------------------
    void Update()
    {
        if (!isPlaying)
            return;

        currentTime += Time.deltaTime * playSpeed;

        // Clamp to end of timeline
        if (currentTime > editorData.MaxTime)
        {
            currentTime = editorData.MaxTime;
            isPlaying = false;
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
        List<ArrowEvent> simEvents = editorData.ConvertToSimulatedEvents(SpawnDistance);

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
            arrow.GetComponent<ArrowBase>().OrientArrow(evt.direction);

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

        currentTime = Mathf.Clamp(t, 0f, editorData.MaxTime);

        foreach (var sim in simulatedArrows)
            sim.Simulate(currentTime);

        SuppressSimulationAudio = false;

        //LevelTimelineUI.Instance.SetScrollRectPos(currentTime);

        float x = LevelTimelineUI.Instance.TimelineView.TimeToPixels(currentTime);

        float normalized = x / Mathf.Max(1f, LevelTimelineUI.Instance.content.sizeDelta.x);
        LevelTimelineUI.Instance.scrollRect.horizontalNormalizedPosition = normalized;
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        isPlaying = false;
        JumpToTime(0f);
    }
}
