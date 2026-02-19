using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class LevelEditorData : MonoBehaviour
{
    public static LevelEditorData Instance { get; private set; }


    public List<ArrowEventData> events = new();
    public float MaxTime { get; private set; } = 0f;
    public int Directions { get; private set; } = 4;  

    [Header("Current Level")]
    public TextAsset currentLevelAsset;

    public float SongOffsetSeconds { get; private set; }

    public void SetSongOffset(float offset)
    {
        SongOffsetSeconds = offset;
    }



    private float bpm = 120f;

    public float BPM
    {
        get => bpm;
        private set
        {
            bpm = value;
            if(UIToast.Instance != null)  UIToast.Show($"BPM updated to: {BPM}");
        }
    }

    public void AddEvent(ArrowEventData evt)
    {
        events.Add(evt);
        SortEvents();
        RecalculateMaxTime();

        // Centralized feedback
        UIToast.Show(evt.Describe());
    }





    public static float SnapTime(float time, float interval = 0.25f)
    {
        return Mathf.Round(time / interval) * interval;
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
        MaxTime = 0f;

        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("❌ Level text is empty");
            return false;
        }

        string[] lines = text.Split(
            new[] { '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries
        );

        // ----------------------------
        // BPM
        // ----------------------------
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.StartsWith("# BPM", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedBPM))
                {
                    BPM = parsedBPM;
                    Debug.Log($"🎵 BPM loaded: {BPM}");
                }
                break;
            }
            if (line.StartsWith("# OFFSET", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedOffset))
                {
                    SongOffsetSeconds = parsedOffset;
                    Debug.Log($"🎵 Song offset loaded: {SongOffsetSeconds}s");
                }
            }
            if (line.StartsWith("# DIRECTIONS", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedDirections))
                {
                    Directions = parsedDirections;
                    Debug.Log($"🎵 Directions loaded from asset: {Directions}");
                }
                break;
            }
        }

        // ----------------------------
        // Events
        // ----------------------------
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            string[] p = line.Split(',');
            if (p.Length < 5)
                continue;

            float time = float.Parse(p[0]);
            string eventType = p[1].Trim().ToLower();

            if (eventType == "arrow")
            {
                string dirStr = p[2].Trim().ToLower();
                float speed = float.Parse(p[3]);
                string spawnName = p[4].Trim().ToLower();

                Vector2 dir = dirStr switch
                {
                    "up" => Vector2.up,
                    "down" => Vector2.down,
                    "left" => Vector2.left,
                    "right" => Vector2.right,
                    "up-right" => new Vector2(1, 1).normalized,
                    "down-right" => new Vector2(1, -1).normalized,
                    "down-left" => new Vector2(-1, -1).normalized,
                    "up-left" => new Vector2(-1, 1).normalized,
                    _ => Vector2.zero
                };

                events.Add(new ArrowEventData(time, "arrow", dir, speed, spawnName));
            }
            else if (eventType == "obstacle")
            {
                string spawnName = p[4].Trim().ToLower();
                events.Add(new ArrowEventData(time, "obstacle", Vector2.zero, 0f, spawnName));
            }

            MaxTime = Mathf.Max(MaxTime, time);
        }

        SortEvents();
        return true;
    }

    public string SerializeToString()
    {
        using StringWriter sw = new StringWriter();

        sw.WriteLine("# Saved by Level Editor");
        sw.WriteLine($"# BPM: {BPM}");
        sw.WriteLine($"# OFFSET: {SongOffsetSeconds}");
        sw.WriteLine($"# DIRECTIONS: {Directions}");
        sw.WriteLine("");

        foreach (var e in events)
        {
            float t = LevelTimelineUI.Instance.MarkerController
                .GetNearestSnapTime(e.beatTime);

            if (e.objectType == "arrow")
            {
                string dirStr =
                    e.direction == Vector2.up ? "up" :
                    e.direction == Vector2.down ? "down" :
                    e.direction == Vector2.left ? "left" :
                    e.direction == Vector2.right ? "right" :
                    (e.direction == new Vector2(1, 1).normalized ? "up-right" :
                    e.direction == new Vector2(1, -1).normalized ? "down-right" :
                    e.direction == new Vector2(-1, -1).normalized ? "down-left" :
                    e.direction == new Vector2(-1, 1).normalized ? "up-left" : "unknown");

                sw.WriteLine($"{t},arrow,{dirStr},{e.speed},{e.nameOfGameObjectToSpawn}");
            }
            else if (e.objectType == "obstacle")
            {
                sw.WriteLine($"{t},obstacle,0,0,{e.nameOfGameObjectToSpawn}");
            }
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
            if (e.objectType != "arrow")
                continue;

            float arrivalSec = e.beatTime * secPerBeat;
            float travelSec = spawnDistance / e.speed;
            float spawnSec = arrivalSec - travelSec;

            converted.Add(new ArrowEvent
            {
                time = e.beatTime,
                direction = e.direction,
                speed = e.speed,
                nameOfGameObjectToSpawn = e.nameOfGameObjectToSpawn,   
                arrivalTime = arrivalSec,
                spawnTime = spawnSec
            });
        }

        return converted;
    }



    // Utilities
    // -----------------------------------------------------------------------


    public void SetBPM(float newBPM)
    {
        BPM = Mathf.Clamp(newBPM, 20f, 300f); // sane limits
        Debug.Log($"🎵 BPM set to: {BPM}");
    }

    public void SortEvents() =>
        events.Sort((a, b) => a.beatTime.CompareTo(b.beatTime));

    public int CountArrows() =>
        events.FindAll(e => e.objectType == "arrow").Count;

    public int CountObstacles() =>
        events.FindAll(e => e.objectType == "obstacle").Count;

    public void RecalculateMaxTime() 
    { 
        MaxTime = 0f; 
        foreach (var e in events) 
        if (e.beatTime > MaxTime) 
        MaxTime = e.beatTime; 
    }

}
