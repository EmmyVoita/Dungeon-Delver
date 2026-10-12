using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Handles spawning arrows and obstacles from text pattern files.
/// </summary>
[System.Serializable]
public class SpawnEvent
{
    public float time;        // spawn time (seconds)
    public float arrivalTime; // 👈 ADD THIS
    public string eventType;
    public Vector2 paramA;
    public float paramB;      // speed
    public string type;
}

public class ArrowSpawner : MonoBehaviour
{
    private enum ObstacleHandlingMode
    {
        PauseResume,
        Ignore
    }

    public static event Action<ArrowBase> OnArrowSpawned;
    public static ArrowSpawner Instance { get; private set; }
    public static Action OnClearArrows;

    [Header("References")]
    [SerializeField] private DirectionalWarningController directionalWarningController;
    [SerializeField] private PatternDatabase patternDatabase;
    
    [SerializeField] private float spawnDistance = 10f;
    [SerializeField] private float goalRadius = 1f;

    [Header("Prefabs")]

    //[SerializeField] private List<ArrowTypeDefinition> arrowTypeDefinitions;
    [SerializeField] private ArrowObjectDatabase arrowDatabase;
    [SerializeField] private ChallengeObjectDatabase challengeDatabase;

    [SerializeField] private ObstacleHandlingMode obstacleHandlingMode = ObstacleHandlingMode.PauseResume;

    // Internal
    [SerializeField] private List<SpawnEvent> patternEvents = new List<SpawnEvent>();
    private Coroutine spawnCoroutine;
    private bool stopRequested = false; // 🔹 new flag for graceful stop
    private float bpm;
    [SerializeField] private GameObject arrowContainer;
    [SerializeField] private GameObject challengeContainer;

    private bool isPausedByObstacle = false;
  
    //private double lastDSPTime;
    //private double scaledSongTime;


    private readonly HashSet<ArrowBase> _activeArrows = new();

    public int ActiveArrowCount => _activeArrows.Count;
    public bool HasActiveArrows =>_activeArrows.Count > 0;
    public bool AllArrowsResolved =>!IsSpawning && _activeArrows.Count == 0;

    public int TotalArrowsThisRound { get; private set; }
    public float ActiveBPM => bpm;
    public List<ArrowTypeDefinition> ArrowTypeDefinitions => arrowDatabase.arrows;
    public List<ObstacleTypeDefinition> ChallengesTypeDefinitions => challengeDatabase.challenges;
    public float SpawnDistance => spawnDistance;
    public float GoalRadius => goalRadius;
    public float ArrowTravelDistance => Mathf.Max(spawnDistance - goalRadius,0f);
    public bool IsSpawning { get; private set; }


    public bool PatternFinished => !IsSpawning;

    public bool AllGameplayResolved => PatternFinished && _activeArrows.Count == 0 && !ObstacleManager.Instance.AnyActive;


    private bool useEightDirections = false;

    //private double pauseDSP;
  


    // --------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        //arrowContainer = new GameObject("Arrow_Container");
    }

    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        ObstacleManager.OnFirstObstacleAppeared += HandlePauseSpawning;
        ObstacleManager.OnAllObstaclesCleared += HandleResumeSpawning;
        ArrowBase.OnArrowResolved += HandleArrowResolved;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        ObstacleManager.OnFirstObstacleAppeared -= HandlePauseSpawning;
        ObstacleManager.OnAllObstaclesCleared -= HandleResumeSpawning;
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
    }

    private void HandleArrowResolved(ArrowResolvedData data)
    {
        if (data.arrowBase == null)
            return;

        _activeArrows.Remove(data.arrowBase);
    }

    private void HandlePauseSpawning()
    {
        if (obstacleHandlingMode == ObstacleHandlingMode.Ignore) return;

        if (isPausedByObstacle) return;

        isPausedByObstacle = true;
    }

    private void HandleResumeSpawning(int damageTaken)
    {
        if (obstacleHandlingMode == ObstacleHandlingMode.Ignore) return;
        
        if (!isPausedByObstacle) return;

        isPausedByObstacle = false;
    }



    // --------------------------------------------------
    public IEnumerator HandleSpawning(TextAsset patternAsset)
    {
        IsSpawning = true;

        if (patternAsset == null)
        {
            Debug.LogError("HandleSpawning called with null TextAsset!");
            IsSpawning = false;
            yield break;
        }

        stopRequested = false;
        LoadPattern(patternAsset);

        Player.Instance.UseEightDirections = useEightDirections;
        spawnCoroutine = StartCoroutine(SpawnFromPattern(GameSessionBootstrap.Config.LevelEditorStartTime));
        yield return spawnCoroutine;
        yield return new WaitForSeconds(1.0f);
        IsSpawning = false;
    }


    public void HandleStateChanged(GameState previousState, GameState newState)
    {
        
        if(newState == GameState.GameOverTally || newState == GameState.DeathSequence)
        {
           StopAllSpawning();
        }
    }

    public void StopAllSpawning()
    {
        stopRequested = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ClearAllArrows()
    {
        foreach (var arrow in GameObject.FindGameObjectsWithTag("Arrow"))
            Destroy(arrow);

        _activeArrows.Clear();

        OnClearArrows?.Invoke();
    }

    public IEnumerator PlayFromTime(TextAsset patternAsset, float startTime)
    {
        StopAllSpawning();
        ClearAllArrows();

       

        stopRequested = false;

        LoadPattern(patternAsset);

        //lastDSPTime = AudioSettings.dspTime;
        //scaledSongTime = startTime;

         IsSpawning = true;

        

        spawnCoroutine = StartCoroutine(SpawnFromPattern(GameSessionBootstrap.Config.LevelStartTime));
        yield return spawnCoroutine;
        IsSpawning = false;
    }

    // --------------------------------------------------
    public void LoadPattern(TextAsset patternAsset)
    {
        patternEvents.Clear();
        useEightDirections = false;

        string[] lines = patternAsset.text.Split(new[] { '\n', '\r' },StringSplitOptions.RemoveEmptyEntries);

        Debug.LogError($"ACTUAL FILE CONTENT:\n{patternAsset.text}");

        // Parse the header for BPM and direction data
        bool hasBPM = ParseHeader(lines, out bpm);

        // Ensure that we have bpm data
        if(!hasBPM)
        {
            Debug.LogError("LEVEL FILE DOES NOT CONTAIN # BPM!");
            return;
        }
        
        // parse for event type
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
                    return;
            }
        }

        patternEvents.Sort((a, b) => a.time.CompareTo(b.time));

        TotalArrowsThisRound = 0;

        foreach (var e in patternEvents)
        {
            if (e.eventType == "arrow")
                TotalArrowsThisRound++;
        }

        Debug.Log($"Level File contains => {patternEvents.Count} Pattern Events, with => {TotalArrowsThisRound} total arrows");
    }

    private void HandlePatternEvent(string[] parts)
    {
        
        float time = float.Parse(parts[0]);
        string patternBlockType = parts[2].Trim().ToLower();
        int patternBlockLength = int.Parse(parts[3]);
        float speed = float.Parse(parts[4]);

        Debug.Log($"Found Pattern Event Time => {time}, Block Type {patternBlockType}," +
                  $"Block Length {patternBlockLength}, Speed => {speed}");

        if (patternBlockType.TryParsePatternType(out PatternType patternType))
        {
            // Create pattern block
            ArrowPattern pattern = patternDatabase.GetRandomPattern(patternType, patternBlockLength);

            if(pattern == null)
            {
                Debug.LogError("When parsing a pattern event and trying to create pattern," +
                               "there were no patterns found that match the parameters" +
                               $"PatternType => {patternType}, Pattern Block Length {patternBlockLength}");
                return;
            }

            foreach (ArrowSpawnData patternArrow in pattern.Arrows)
            {
                float arrowBeat = patternArrow.beat;
                float totalBeat = time + arrowBeat;

                Debug.Log($"Adding pattern arrow: LocalBeat={patternArrow.beat}, " + $"TotalBeat={totalBeat}");

                AddArrow(totalBeat,speed,patternArrow.direction, "normalarrow");
            }
        }
        else
        {
        Debug.LogError($"Failed to parse pattern type '{patternBlockType}'");
        }
    }

    private void HandleChallengeEvent(string[] parts)
    {
        float time = float.Parse(parts[0]);
        string type = parts[4].Trim().ToLower();

        float secondsPerBeat = 60f / bpm;

        patternEvents.Add(new SpawnEvent
        {
            time = time * secondsPerBeat,
            eventType = "obstacle",
            paramA = Vector2.zero,   // unused
            paramB = 0,              // unused
            type = type              // fileName from your CSV
        });
    }

    private void HandleArrowEvent(string[] parts)
    {
        float time = float.Parse(parts[0]);
        string type = parts[4].Trim().ToLower();
        
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

        AddArrow(time, speed, dir, type);        
    }

    private void AddArrow(float time, float speed, Vector2 dir, string type)
    {
        ArrowTypeDefinition arrowTypeDef =  arrowDatabase.arrows.Find(def => def.displayName.ToLower() == type.ToLower());

        float spawnTime = CalculateSpawnTime(time, speed);
        float arrivalTime = CalculateArrivalTime(time);

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
            arrivalTime = arrivalTime,
            eventType = "arrow",
            paramA = dir,
            paramB = speed,
            type = type
        });
    }

    private bool ParseHeader(string[] lines, out float bpm)
    {
        bpm = 0;
        bool hasBPM = false;
        //Check first few lines for a "# BPM:" definition
        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (line.StartsWith("# BPM", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(':');
                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedBPM))
                {
                    bpm = parsedBPM;
                    hasBPM = true;
                }
            }
            else if (line.StartsWith("# DIRECTIONS", StringComparison.OrdinalIgnoreCase))
            {
                if (line.Contains("8"))
                    useEightDirections = true;
            }

            // Header is finished once we reach actual level data.
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                break;
            }
        }
        
        return hasBPM;
    }

    public List<ArrowSpawnData> ResolvePatternBlock(List<PatternBlock> blocks)
    {
        List<ArrowSpawnData> result = new();

        foreach (PatternBlock block in blocks)
        {
            ArrowPattern pattern = patternDatabase.GetRandomPattern(block.PatternType, block.LengthInBeats);

            if (pattern == null)
                continue;

            
        }

        return result;
    }

    // --------------------------------------------------
    private float CalculateSpawnTime(float beat, float speed)
    {
        float secondsPerBeat = 60f / bpm;
        float arrivalTime = beat * secondsPerBeat;

        float adjustedDistance = Mathf.Max(0f, spawnDistance - goalRadius);
        float travelTime = adjustedDistance / speed;

        return Mathf.Max(0f, arrivalTime - travelTime);
    }

    private float CalculateArrivalTime(float beat)
    {
        float secondsPerBeat = 60f / bpm;
        float arrivalTime = beat * secondsPerBeat;

        return arrivalTime;
    }



    private bool IsGameplayActive()
    {
        var state = GameStateManager.Instance.CurrentState;

        return state == GameState.RoundActive ||
            state == GameState.PreRoundCountdown;
    }


    // --------------------------------------------------
    private IEnumerator SpawnFromPattern(float startTime = 0)
    {
        int index = FindStartingIndex(startTime);

        Debug.Log($"Spawning from pattern at time => {startTime} \n" + 
                  $"Which evaluates to start index => {index}");


        while (index < patternEvents.Count)
        {
            if (GameStateManager.Instance.CurrentState == GameState.Paused)
            {
                yield return null; 
                continue;
            }

            if (stopRequested || !IsGameplayActive())
                yield break;

            var evt = patternEvents[index];

            // wait until it's time to spawn this event
            yield return WaitUntilEventTime(evt.time);

            SpawnEvent(evt);
            index++;
        }

        UIToast.Show("✅ SpawnFromPattern finished normally.");
    }


    private IEnumerator WaitUntilEventTime(double targetTime)
    {
        while (true)
        {
            if (stopRequested || !IsGameplayActive())
                yield break;

            double elapsed = MusicManager.Instance.ScaledElapsedTime;

            if (elapsed >= targetTime)
                yield break;

            yield return null;
        }
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
                SpawnArrow(e.paramA, e.paramB, e.time, e.arrivalTime, e.type);
                break;

            case "obstacle":
                SpawnObstacle(e.paramA, e.type);
                break;

            case "warning":
                TriggerWarning(e.paramA, e.type);
                break;
        }
    }






   
    public void SpawnObstacle(Vector2 _ignored, string type, int damageOverride = -1)
    {

        ObstacleTypeDefinition obstacleTypeDef = challengeDatabase.challenges.Find(def => def.fileName.ToLower() == type.ToLower());

        if(obstacleTypeDef == null)
        {
            Debug.LogError($"Obstacle obstacleTypeDef for type '{type}' not found!");
            return;
        }

        GameObject prefab = obstacleTypeDef.prefab;
        GameObject obstacle = Instantiate(prefab, Vector2.zero, Quaternion.identity);

        obstacle.transform.parent = challengeContainer.transform;

        ObstacleManager.OnObstacleSpawned?.Invoke(obstacle);   

        ChallengeBase challengeBase = obstacle.GetComponentInChildren<ChallengeBase>();


        if (challengeBase == null)
        {
            Debug.LogError(
                $"{obstacle.name} has no ChallengeBase!"
            );

            Destroy(obstacle);
            return;
        }

        challengeBase.Begin();

        if (damageOverride != -1)
        {
            obstacle.GetComponent<DamageEffect>().damage = damageOverride;
        }
    }

   
    public void SpawnArrow(Vector2 direction, float speed, float spawnTime, float arrivalTime, string type, Color colorOverride = default, int damageOverride = -1)
    {
        ArrowTypeDefinition arrowTypeDef = arrowDatabase.arrows.Find(def => def.displayName.ToLower() == type.ToLower());

        GameObject arrowPrefab = arrowTypeDef.prefab;
        if(arrowPrefab == null)
        {
            Debug.LogError($"Arrow prefab for type '{type}' not found!");
            return;
        }

        Vector3 localDirection = arrowContainer.transform.InverseTransformDirection(direction);


        Vector2 spawnPos = (Vector2)direction * spawnDistance;
        Vector2 endPos = (Vector2)direction * goalRadius;

        GameObject arrow = Instantiate(arrowPrefab, arrowContainer.transform);
        arrow.tag = "Arrow";
        arrow.transform.localPosition = spawnPos;
        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = colorOverride == default ? Color.white : colorOverride;


        if(damageOverride != -1)
        {
            arrow.GetComponent<DamageEffect>().damage = damageOverride;
        }

        ArrowEffectManager.Instance.ApplyEffectsToArrow(arrow.GetComponent<ArrowBase>());

        arrow.GetComponent<ArrowBase>().Init(direction, 
                                             speed,
                                             spawnTime,
                                             arrivalTime,
                                             spawnPos,
                                             endPos);

        _activeArrows.Add(arrow.GetComponent<ArrowBase>());
        
        RunHistoryManager.Instance.RoundStatsTracker.AddSpawned();

        OnArrowSpawned?.Invoke(arrow.GetComponent<ArrowBase>());
    }

    public void TriggerWarning(Vector2 direction, string type)
    {
        ArrowTypeDefinition arrowTypeDef = arrowDatabase.arrows.Find(def => def.displayName.ToLower() == type.ToLower());

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
