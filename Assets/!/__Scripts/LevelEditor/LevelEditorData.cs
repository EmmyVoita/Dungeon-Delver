using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class LevelEditorData : MonoBehaviour
{
    public static LevelEditorData Instance { get; private set; }


    [SerializeField] private TimelineView timelineView;
    [SerializeField] private float defaultZoom;

    public List<LevelEventData> events = new();

    // Max time in beats
    public float MaxBeatTime { get; private set; } = 0f;
    public float MaxSecondsTime => MaxBeatTime * (60f / _bpm);

    public int Directions { get; private set; } = 4;  

    [Header("Current Level")]
    public TextAsset currentLevelAsset;


    private float _bpm;


    public float SongOffsetSeconds { get; private set; }


    public void SetSongOffset(float offset) {SongOffsetSeconds = offset;}
    public void SetBPM(float newBPM) { BPM = Mathf.Clamp(newBPM, 20f, 300f); }
    public void SortEvents() => events.Sort((a, b) => a.beatTime.CompareTo(b.beatTime));
    public int CountArrows() => events.FindAll(e => e is ArrowEventData arrowEventData).Count;
    public int CountObstacles() => events.FindAll(e => e is ChallengeEventData challengeEventData).Count;

    
    public static float SnapTime(float time, float interval = 0.25f)
    {
        return Mathf.Round(time / interval) * interval;
    }

    public float BPM
    {
        get => _bpm;
        private set
        {
            _bpm = value;
            if(UIToast.Instance != null)  UIToast.Show($"BPM updated to: {BPM}");
        }
    }

    public void AddEvent(LevelEventData evt)
    {
        events.Add(evt);

        SortEvents();
        RecalculateMaxTime();

        // Centralized feedback
        UIToast.Show(evt.Describe());
    }

    public void RemoveEvent(LevelEventData evt)
    {
        events.Remove(evt);

        SortEvents();
        RecalculateMaxTime();

        // Centralized feedback
        UIToast.Show(evt.Describe());
    }







    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // Load & Save Level file
    // -----------------------------------------------------------------------

    public bool LoadLevel(TextAsset asset)
    {
        if (asset == null)
        {
            Debug.LogError("❌ LoadLevel called with null TextAsset");
            return false;
        }

        

        currentLevelAsset = asset;
        return LoadLevelFromText(asset.text);
    }


    public bool LoadLevelFromText(string text)
    {
        events.Clear();
        MaxBeatTime = 0f;

        //timelineView.AssignZoom(defaultZoom);

        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("❌ Level text is empty");
            return false;
        }

        string[] lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Parse the header and read: bpm, song starting offset, and number of arrow directions
        ParseHeader(lines);

        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(',');

            string eventType = parts[1].Trim().ToLower();

            switch(eventType)
            {
                case "arrow":
                    HandleArrowEvent(parts);
                    break;
                case "obstacle":
                    HandleChallengeEvent(parts);
                    break;
                case "pattern":
                    HandlePatternEvent(parts);
                    break;
                default:
                    Debug.LogError($"UNRECOGNIZED EVENTTYPE SYMBOL WHEN PARSING LEVEL FILE => {eventType}");
                    continue;
            }

            float timeInBeats = float.Parse(parts[0]);
            MaxBeatTime = Mathf.Max(MaxBeatTime, timeInBeats);
        }

        UIToast.Show($"Max time (in beats) on load => {MaxBeatTime}");

        SortEvents();
        return true;
    }

    private void ParseHeader(string[] lines)
    {
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.StartsWith("# BPM", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedBPM))
                {
                    BPM = parsedBPM;
                    UIToast.Show($"Song BPM loaded => {BPM} beats per minuite");
                }
                break;
            }
            if (line.StartsWith("# OFFSET", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedOffset))
                {
                    SongOffsetSeconds = parsedOffset;
                    UIToast.Show($"Song Offset loaded => {SongOffsetSeconds} seconds");
                }
            }
            if (line.StartsWith("# DIRECTIONS", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedDirections))
                {
                    Directions = parsedDirections;
                    UIToast.Show($"Song Directions loaded => {Directions} arrow directions");
                }
                break;
            }
        }
    }

    private void HandleArrowEvent(string[] parts)
    {
        float timeInBeats = float.Parse(parts[0]);
        float arrowSpeed = float.Parse(parts[3]);
        string spawnName = parts[4].Trim().ToLower();

        // cast our string direction to a vector2
        string dirStr = parts[2].Trim().ToLower();
        Vector2 direction = DirectionUtils.FromString(dirStr);

        events.Add(new ArrowEventData(timeInBeats, direction, arrowSpeed, spawnName));
    }

    private void HandleChallengeEvent(string[] parts)
    {
        float timeInBeats = float.Parse(parts[0]);
        string spawnName = parts[4].Trim().ToLower();

        events.Add(new ChallengeEventData(timeInBeats, spawnName));
    }

    private void HandlePatternEvent(string[] parts)
    {
        float timeInBeats = float.Parse(parts[0]);
        string patternBlockType = parts[2].Trim().ToLower();
        int patternBlockLength = int.Parse(parts[3]);
        float arrowSpeed = float.Parse(parts[4]);

        if (patternBlockType.TryParsePatternType(out PatternType patternType))
        {
            events.Add(new PatternEventData(timeInBeats,patternType,patternBlockLength, arrowSpeed));
        }
    }

    public string SerializeToString()
    {
        using StringWriter sw = new StringWriter();

        sw.WriteLine("# Saved by Level Editor");
        sw.WriteLine($"# BPM: {BPM}");
        sw.WriteLine($"# OFFSET: {SongOffsetSeconds}");
        sw.WriteLine($"# DIRECTIONS: {Directions}");
        sw.WriteLine();

        foreach (LevelEventData e in events)
        {
            e.beatTime = LevelTimelineUI.Instance.MarkerController
                .GetNearestSnapTime(e.beatTime);

            sw.WriteLine(e.Serialize());
        }

        return sw.ToString();
    }



    // Create an ArrowEvent list for simulation form the editor data. We don't spawn obstacles in the simulation.
    // -----------------------------------------------------------------------
    public List<ArrowEvent> ConvertToSimulatedEvents(float spawnDistance)
    {
        float secPerBeat = 60f / BPM;
        List<ArrowEvent> converted = new();

        foreach (var e in events)
        {
            if (e is ArrowEventData arrowEventData)
            {
                float arrivalSec = e.beatTime * secPerBeat;
                float travelSec = spawnDistance / arrowEventData.speed;
                float spawnSec = arrivalSec - travelSec;

                converted.Add(new ArrowEvent
                {
                    time = e.beatTime,
                    direction = arrowEventData.direction,
                    speed = arrowEventData.speed,
                    nameOfGameObjectToSpawn = arrowEventData.arrowType,   
                    arrivalTime = arrivalSec,
                    spawnTime = spawnSec
                });
            }
        }

        return converted;
    }



  


    public void RecalculateMaxTime() 
    { 
        MaxBeatTime = 0f; 
        foreach (var e in events) 
            if (e.beatTime > MaxBeatTime) 
                MaxBeatTime = e.beatTime; 
    }

}
