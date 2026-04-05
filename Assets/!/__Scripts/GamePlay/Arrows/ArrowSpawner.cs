using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Timers;
using NUnit.Framework;

/// <summary>
/// Handles spawning arrows and obstacles from text pattern files.
/// </summary>
[System.Serializable]
public class SpawnEvent
{
    public float time;
    public string eventType;   // "arrow" or "obstacle"
    public Vector2 paramA;     // direction or position
    public float paramB;       // speed or unused
    public string type;      // prefab index
}

public class ArrowSpawner : MonoBehaviour
{
    private enum ObstacleHandlingMode
    {
        PauseResume,
        Ignore
    }

    public static ArrowSpawner Instance { get; private set; }
    public static Action OnClearArrows;

    [Header("References")]
    [SerializeField] private DirectionalWarningController directionalWarningController;
    
    [SerializeField] private float spawnDistance = 10f;

    [Header("Prefabs")]

    [SerializeField] private List<ArrowTypeDefinition> arrowTypeDefinitions;
    [SerializeField] private List<ObstacleTypeDefinition> obstacleTypeDefinitions;

    [SerializeField] private ObstacleHandlingMode obstacleHandlingMode = ObstacleHandlingMode.PauseResume;

    // Internal
    [SerializeField] private List<SpawnEvent> patternEvents = new List<SpawnEvent>();
    private Coroutine spawnCoroutine;
    private bool stopRequested = false; // 🔹 new flag for graceful stop
    private float bpm;
    private GameObject arrowContainer;

    private bool isPausedByObstacle = false;
    private int currentIndex = 0;

    private double lastDSPTime;
    private float scaledSongTime;




    public int TotalArrowsThisRound { get; private set; }
    public float ActiveBPM => bpm;
    public List<ArrowTypeDefinition> ArrowTypeDefinitions => arrowTypeDefinitions;
    public List<ObstacleTypeDefinition> ChallengesTypeDefinitions => obstacleTypeDefinitions;
    public float SpawnDistance => spawnDistance;
    public bool IsSpawning { get; private set; }


    private bool useEightDirections = false;

    private double pauseDSP;
    private double accumulatedPauseDSP;


    public void OnPause()
    {
        pauseDSP = AudioSettings.dspTime;
    }

    public void OnResume()
    {
        accumulatedPauseDSP += AudioSettings.dspTime - pauseDSP;
    }



    // --------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        arrowContainer = new GameObject("Arrow_Container");
    }

    void OnEnable()
    {
        UIManager.OnGameOver += StopAllSpawning;
        ObstacleManager.OnFirstObstacleAppeared += HandlePauseSpawning;
        ObstacleManager.OnAllObstaclesCleared += HandleResumeSpawning;
    }

    void OnDisable()
    {
        UIManager.OnGameOver -= StopAllSpawning;
        ObstacleManager.OnFirstObstacleAppeared -= HandlePauseSpawning;
        ObstacleManager.OnAllObstaclesCleared -= HandleResumeSpawning;
    }

    private void HandlePauseSpawning()
    {
        if (obstacleHandlingMode == ObstacleHandlingMode.Ignore) return;

        if (isPausedByObstacle) return;

        isPausedByObstacle = true;
    }

    private void HandleResumeSpawning()
    {
        if (obstacleHandlingMode == ObstacleHandlingMode.Ignore) return;
        
        if (!isPausedByObstacle) return;

        isPausedByObstacle = false;
    }



    // --------------------------------------------------
    public IEnumerator HandleSpawning(TextAsset patternAsset, float bpmModifier = 0f)
    {
        IsSpawning = true;

        lastDSPTime = AudioSettings.dspTime;
        scaledSongTime = 0f;


        if (patternAsset == null)
        {
            Debug.LogError("HandleSpawning called with null TextAsset!");
            IsSpawning = false;
            yield break;
        }

        stopRequested = false;
        LoadPattern(patternAsset, bpmModifier);

        Player.Instance.UseEightDirections = useEightDirections;
        spawnCoroutine = StartCoroutine(SpawnFromPattern());
        yield return spawnCoroutine;
        IsSpawning = false;
    }


    public void StopAllSpawning()
    {
        Debug.Log("🛑 ArrowSpawner.StopAllSpawning called");

        stopRequested = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ClearAllArrows()
    {
        OnClearArrows?.Invoke();
    }

    public IEnumerator PlayFromTime(TextAsset patternAsset, float startTime, float bpmModifier = 0f)
    {
        StopAllSpawning();
        ClearAllArrows();

       

        stopRequested = false;

        LoadPattern(patternAsset, bpmModifier);

        lastDSPTime = AudioSettings.dspTime;
        scaledSongTime = startTime;

         IsSpawning = true;

        currentIndex = FindStartingIndex(startTime);

        spawnCoroutine = StartCoroutine(SpawnFromPattern(startTime));
        yield return spawnCoroutine;
        IsSpawning = false;
    }

    // --------------------------------------------------
    public void LoadPattern(TextAsset patternAsset, float bpmModifier = 0f)
    {
        patternEvents.Clear();
        useEightDirections = false;

        string[] lines = patternAsset.text.Split(
            new[] { '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries
        );

        float fileBPM = bpm; // fallback

        // 🔹 Check first few lines for a "# BPM:" definition
        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (line.StartsWith("# BPM", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedBPM))
                {
                    fileBPM = parsedBPM;
                    Debug.Log($"🎵 BPM loaded from asset: {fileBPM}");
                }
                break;
            }

            if (line.StartsWith("# DIRECTIONS", StringComparison.OrdinalIgnoreCase))
            {
                if (line.Contains("8"))
                    useEightDirections = true;
            }
        }


        

        Debug.Log($"Using BPM: {fileBPM} (Modifier: {bpmModifier})");

        // Use the file BPM if available
        bpm = fileBPM + fileBPM * bpmModifier;
        float secondsPerBeat = 60f / bpm;

        


        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 5)
            {
                Debug.LogWarning($"Invalid line format: {line}");
                continue;
            }

            float time = float.Parse(parts[0]);
            string eventType = parts[1].Trim().ToLower();
            string type = parts[4].Trim().ToLower();

            if (eventType == "arrow")
            {
                string dirStr = parts[2].Trim().ToLower();
                float speed = float.Parse(parts[3]);

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

                ArrowTypeDefinition arrowTypeDef =  arrowTypeDefinitions.Find(def => def.displayName.ToLower() == type.ToLower());
                       
                float spawnTime = CalculateSpawnTime(time, speed);

                // 🔹 Add warning BEFORE arrow if needed
                if (arrowTypeDef != null && arrowTypeDef.requiresWarning)
                {
                    patternEvents.Add(new SpawnEvent
                    {
                        time = Mathf.Max(0, spawnTime - arrowTypeDef.warningLeadTime),
                        eventType = "warning",
                        paramA = dir,
                        type = type
                    });
                }

                patternEvents.Add(new SpawnEvent
                {
                    time = spawnTime,
                    eventType = "arrow",
                    paramA = dir,
                    paramB = speed,
                    type = type
                });
            }
            else if (eventType == "obstacle")
            {
                patternEvents.Add(new SpawnEvent
                {
                    time = time * secondsPerBeat,
                    eventType = "obstacle",
                    paramA = Vector2.zero,   // unused
                    paramB = 0,              // unused
                    type = type              // fileName from your CSV
                });
            }

        }

        patternEvents.Sort((a, b) => a.time.CompareTo(b.time));

        TotalArrowsThisRound = 0;

        foreach (var e in patternEvents)
        {
            if (e.eventType == "arrow")
                TotalArrowsThisRound++;
        }
    }

    // --------------------------------------------------
    private float CalculateSpawnTime(float beat, float speed)
    {
        float secondsPerBeat = 60f / bpm;
        float arrivalTime = beat * secondsPerBeat;
        float travelTime = spawnDistance / speed;
        float spawnTime = Mathf.Max(0, arrivalTime - travelTime);
        return spawnTime;
    }


    // --------------------------------------------------
    private IEnumerator SpawnFromPattern(float startTime = 0)
    {
        int index = FindStartingIndex(startTime);

        UIToast.Show($"🚀 SpawnFromPattern start Time {RoundManager.Instance.RoundStartTime - Time.time}");

        while (index < patternEvents.Count)
        {
            if(GameStateManager.Instance.CurrentState == GameState.Paused)
            {
                yield return null;
                continue;
            }

            if (stopRequested)
                yield break;

            if (isPausedByObstacle)
            {
                yield return null;
                continue;
            }

            double currentDSP = AudioSettings.dspTime;
            double dspDelta = currentDSP - lastDSPTime;

            scaledSongTime += (float)(dspDelta * Time.timeScale);

            lastDSPTime = currentDSP;

            float elapsed = scaledSongTime;

            index = SpawnReadyEvents(index, elapsed);

            yield return null;
        }

        UIToast.Show("✅ SpawnFromPattern finished normally.");
    }

    private int FindStartingIndex(float targetTime)
    {
        for (int i = 0; i < patternEvents.Count; i++)
        {
            if (patternEvents[i].time >= targetTime)
                return i;
        }

        return patternEvents.Count;
    }

    private int SpawnReadyEvents(int startIndex, float elapsed)
    {
        int index = startIndex;

        while (index < patternEvents.Count &&
            elapsed >= patternEvents[index].time)
        {
            SpawnEvent(patternEvents[index]);
            index++;
        }

        return index;
    }


    private void SpawnEvent(SpawnEvent e)
    {
        UIToast.Show(
            $"SpawnEvent: \n" +
            $"Time  {Time.time - RoundManager.Instance.RoundStartTime}s \n" +
            $"DSP Time = {AudioSettings.dspTime - RoundManager.Instance.RoundStartDSP}", 
            3f
        );

        switch (e.eventType)
        {
            case "arrow":
                SpawnArrow(e.paramA, e.paramB, e.type);
                break;

            case "obstacle":
                SpawnObstacle(e.paramA, e.type);
                break;

            case "warning":
                TriggerWarning(e.paramA, e.type);
                break;
        }
    }






    // --------------------------------------------------
    public void SpawnObstacle(Vector2 _ignored, string type, int damageOverride = -1)
    {

        ObstacleTypeDefinition obstacleTypeDef = obstacleTypeDefinitions.Find(def => def.fileName.ToLower() == type.ToLower());

        if(obstacleTypeDef == null)
        {
            Debug.LogError($"Obstacle obstacleTypeDef for type '{type}' not found!");
            return;
        }

        GameObject prefab = obstacleTypeDef.prefab;
        GameObject obstacle = Instantiate(prefab, Vector2.zero, Quaternion.identity);

        obstacle.transform.parent = arrowContainer.transform;

        ObstacleManager.OnObstacleSpawned?.Invoke(obstacle);   

        if (damageOverride != -1)
        {
            obstacle.GetComponent<DamageEffect>().damage = damageOverride;
        }
    }

    // --------------------------------------------------
    public void SpawnArrow(Vector2 direction, float speed, string type, Color colorOverride = default, int damageOverride = -1)
    {
        ArrowTypeDefinition arrowTypeDef = arrowTypeDefinitions.Find(def => def.displayName.ToLower() == type.ToLower());

        GameObject arrowPrefab = arrowTypeDef.prefab;
        if(arrowPrefab == null)
        {
            Debug.LogError($"Arrow prefab for type '{type}' not found!");
            return;
        }

        Vector2 spawnPos = (Vector2)transform.position + direction * spawnDistance;

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.tag = "Arrow";

        arrow.transform.parent = arrowContainer.transform;

        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = colorOverride == default ? Color.white : colorOverride;

        if(damageOverride != -1)
        {
            arrow.GetComponent<DamageEffect>().damage = damageOverride;
        }

        ArrowEffectManager.Instance.ApplyEffectsToArrow(arrow.GetComponent<ArrowBase>());
        arrow.GetComponent<ArrowBase>().Fire(direction, speed);
        RoundManager.Instance.stats.AddSpawned();

        Debug.Log($"🎯 Spawned arrow at level time {Time.time - RoundManager.Instance.RoundStartTime}s.");
    }

    public void TriggerWarning(Vector2 direction, string type)
    {
        ArrowTypeDefinition arrowTypeDef = arrowTypeDefinitions.Find(def => def.displayName.ToLower() == type.ToLower());

        if(arrowTypeDef == null)
        {
            Debug.LogError($"ArrowTypeDefinition for type '{type}' not found for warning!");
            return;
        }

        if(arrowTypeDef.requiresWarning)
        {
            directionalWarningController.Flash(DirectionFromVector(direction));
        }
    }

    private Direction DirectionFromVector(Vector2 dir)
    {
        if (dir == Vector2.up) return Direction.Up;
        if (dir == Vector2.down) return Direction.Down;
        if (dir == Vector2.left) return Direction.Left;
        if (dir == Vector2.right) return Direction.Right;

        // Diagonals (pick a rule)
        if (dir.x > 0 && dir.y > 0) return Direction.Up;
        if (dir.x > 0 && dir.y < 0) return Direction.Right;
        if (dir.x < 0 && dir.y > 0) return Direction.Left;
        if (dir.x < 0 && dir.y < 0) return Direction.Down;

        return Direction.Up;
    }

}
