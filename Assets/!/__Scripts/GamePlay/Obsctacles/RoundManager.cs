using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;






public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    // ------------------------------------------------------------
    // Test Mode
    // ------------------------------------------------------------

    public bool isTestMode => TestSession.runSingleLevel;
    public TextAsset testLevel => TestSession.tempLevelAsset;

    // ------------------------------------------------------------
    // Settings
    // ------------------------------------------------------------

    [Header("Dev Controls")]
    [SerializeField] private bool preventAutoStart = false;
    [SerializeField] private float fastForwardMultiplier = 2f;

    [Header("Input")]
    public Key skipRoundKey = Key.R;
    public Key skipStageKey = Key.T; // 👈 new

    [Header("Player Upgrades")]
    public float bpmBonus = 0f;

    [Header("References")]
    [SerializeField] private ScoreTallyController tallyController;
    [SerializeField] private ArrowSpawner arrowSpawner;
    [SerializeField] private RoundStatsUI roundStatsUI;
    [SerializeField] private float roundDelay = 2f;
    [SerializeField] private float roundEndTimeout = 20f;

    // ------------------------------------------------------------
    // Events
    // ------------------------------------------------------------

    public static event Action OnRoundStart;
    public static event Action OnRoundEndA;
    public static event Action OnRoundEnd;

    // ------------------------------------------------------------
    // Stages
    // ------------------------------------------------------------

    [Header("Stages")]
    public List<StageData> stages;

    // ------------------------------------------------------------
    // Runtime State
    // ------------------------------------------------------------

    [Header("Runtime")]
    [SerializeField] private int currentStageIndex = 0;
    [SerializeField] private int currentLevelIndex = 0;
    public RoundStatsTracker stats = new();
    public RunStatsTracker runStats = new();
  


    private List<TextAsset> currentStageSequence = new();
    private Coroutine activeRoundCoroutine;
    private bool isFastForward = false;
    private bool applyTempBPMBonus = false;


   
    public float RoundBPMMultiplier => 1 + bpmBonus;
    public double RoundStartDSP { get; set; }
    public float  RoundStartTime { get; private set; } // gameplay time

    public float RoundCountdownStartTime { get; private set; }
    public bool IsBossRound => stages[currentStageIndex].bossLevelFile == currentStageSequence[currentLevelIndex - 1];
 

    // ------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------

    private void OnEnable()
    {
        ArrowBase.OnArrowResolved += stats.RegisterArrow;
        UpgradeCardManager.UpgradeSelectionComplete += SetupAndStartRound;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        ArrowBase.OnArrowResolved -= stats.RegisterArrow;
        UpgradeCardManager.UpgradeSelectionComplete -= SetupAndStartRound;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        stats.Reset();
        runStats.ResetRun();
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        #if UNITY_EDITOR
            if (preventAutoStart)
                return;
        #endif  

        if(previousState == newState) return;

        if(newState == GameState.RunLoad)
        {
            StartStage(currentStageIndex);
        }
        else if(newState == GameState.Editor)
        {
            StartCoroutine(PlayTestLevel());
        }
    }

    private void Start()
    {

        /*
        if(GameSceneLoader.PendingConfig == null)
        {
            StartStage(currentStageIndex);
            return;
        } 

        switch(GameSceneLoader.PendingConfig.Mode)
        {
            case GameMode.StandardRun:
                StartStage(currentStageIndex);
                break;
            case GameMode.ObstaclePractice:
                break;
            case GameMode.LevelEditorTest:
                StartCoroutine(PlayTestLevel());
                break;
            case GameMode.LevelEdtiorPlayFromPosition:
                 StartCoroutine(PlayTestLevel());
                 break;
            default:
                break;
        }
        */
        /*
        if(GameSessionContextHandler.Instance.RunTestLevel)
        {
            StartCoroutine(PlayTestLevel());
        }

        else if (GameSessionContextHandler.Instance.RunStandardGame)
        {
            StartStage(currentStageIndex);
        }
        */
    }

   private void Update()
    {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleFastForward();
    #endif

        if (Keyboard.current[skipRoundKey].wasPressedThisFrame)
            SkipRound();

        if (isTestMode && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("⏹ Test aborted. Returning to editor.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(TestSession.returnScene);
        }
    }

    


    // ------------------------------------------------------------
    // Test Mode
    // ------------------------------------------------------------

    private IEnumerator PlayTestLevel()
    {
        if (testLevel == null)
        {
            Debug.LogError("❌ Test level TextAsset is null.");
            yield break;
        }

        stats.Reset();
        runStats.ResetRun();

        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            activeRoundCoroutine = StartCoroutine(PlayRound(testLevel));
        });

        Debug.Log("✔ Test level complete. Returning to editor…");
    }

    // ------------------------------------------------------------
    // Stage / Round Flow
    // ------------------------------------------------------------

    private void StartStage(int stageIndex)
    {
        if(stageIndex == 0)
        {
            runStats.ResetRun();
        }

        if (stageIndex >= stages.Count)
        {
            return;
        }

        MusicManager.Instance.mainClip = stages[stageIndex].musicClip;

        StageData stage = stages[stageIndex];
        currentStageSequence.Clear();

        List<TextAsset> sourceLevels = new(stage.normalLevelFiles);

        if (sourceLevels.Count == 0)
        {
            Debug.LogWarning($"⚠ Stage {stage.stageName} has no normal levels.");
        }

        switch (stage.levelOrderMode)
        {
            case LevelOrderMode.InOrder:
                AddSequentialLevels(stage, sourceLevels);
                break;

            case LevelOrderMode.Random:
                AddRandomLevels(stage, sourceLevels);
                break;
        }

        // Boss always last
        if (stage.bossLevelFile != null)
            currentStageSequence.Add(stage.bossLevelFile);

        currentLevelIndex = 0;
        SetupAndStartRound();
    }

    private void AddSequentialLevels(StageData stage, List<TextAsset> sourceLevels)
    {
        int count = Mathf.Min(stage.levelsToPlay, sourceLevels.Count);

        for (int i = 0; i < count; i++)
        {
            currentStageSequence.Add(sourceLevels[i]);
        }
    }

    private void AddRandomLevels(StageData stage, List<TextAsset> sourceLevels)
    {
        sourceLevels.Shuffle();

        if (stage.allowRepeats)
        {
            for (int i = 0; i < stage.levelsToPlay; i++)
            {
                currentStageSequence.Add(sourceLevels[i % sourceLevels.Count]);
            }
        }
        else
        {
            int count = Mathf.Min(stage.levelsToPlay, sourceLevels.Count);
            for (int i = 0; i < count; i++)
            {
                currentStageSequence.Add(sourceLevels[i]);
            }
        }
    }




    public void SetupAndStartRound()
    {
        // Reset stats for the new round
        stats.Reset();

        // Safety check to prevent starting a new round while one is active
        if (activeRoundCoroutine != null)
            StopCoroutine(activeRoundCoroutine);

        // Check if we've completed the current stage
        if (currentLevelIndex >= currentStageSequence.Count)
        {
            currentStageIndex++;
            StartStage(currentStageIndex);
            return;
        }


        TextAsset levelFile = currentStageSequence[currentLevelIndex];
        
        if (levelFile == null)
        {
            Debug.LogError("❌ LevelData has no TextAsset assigned!");
            return;
        }

       
        RoundCountdownStartTime = Time.time;

        
        GameStateManager.Instance.SetState(GameState.PreRoundCountdown);
        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            activeRoundCoroutine = StartCoroutine(PlayRound(levelFile));
        });
        
    }


    private IEnumerator PlayRound(TextAsset levelFile)
    {
        OnRoundStart?.Invoke();

        StageData stage = stages[currentStageIndex];

        // If Boss round, trigger boss logic
        if (stage.bossLevelFile == levelFile && stage.bossDefinition != null)
        {
            BossManager.Instance.StartBoss(stage.bossDefinition);
        }


        GameStateManager.Instance.SetState(GameState.RoundActive);


        if(GameSceneLoader.PendingConfig != null && GameSceneLoader.PendingConfig.Mode == GameMode.LevelEdtiorPlayFromPosition)
        {
            float startTime = GameSessionBootstrap.Config.LevelEditorStartTime;

            yield return StartCoroutine(ArrowSpawner.Instance.PlayFromTime(levelFile, 
                                                                           startTime, 
                                                                           bpmBonus));
        }
        else
        {
            yield return StartCoroutine(ArrowSpawner.Instance.HandleSpawning(levelFile, 
                                                                             bpmBonus));
        }
            


        // ✅ Wait until obstacles are cleared
        yield return new WaitUntil(() =>
            !ArrowSpawner.Instance.IsSpawning &&
            !ObstacleManager.Instance.AnyActive
        );


        yield return new WaitForSeconds(roundDelay);
  


        bool tallyComplete = false;

        if(ComboManager.Instance.GetCurrentComboCount > 0)
        {
            ScoreTallyController.RoundEndTallyComplete += Handler;

            void Handler()
            {
                tallyComplete = true;
            }

            GameStateManager.Instance.SetState(GameState.RoundResultsTally);

            yield return CoroutineHelpers.WaitUntilOrTimeout(() => tallyComplete, roundEndTimeout);

            ScoreTallyController.RoundEndTallyComplete -= Handler;
        }

       

        if (applyTempBPMBonus)
        {
            bpmBonus = 0f;
            applyTempBPMBonus = false;
        }

        runStats.AddRound(stats);
     
        GameStateManager.Instance.SetState(GameState.RoundResults);
        yield return StartCoroutine(CoroutineHelpers.WaitForConfirm(GameState.RoundResultsExit));

        yield return StartCoroutine(roundStatsUI.PlayOutroAnimations());

        GameStateManager.Instance.SetState(GameState.UpgradeSelection);

        currentLevelIndex++;

        UpgradeCardManager.Instance.ShowCardChoices();
    }

    // ------------------------------------------------------------
    // Stats / Utilities
    // ------------------------------------------------------------

    public void ApplyTempBPMBonus(float bonus)
    {
        bpmBonus = bonus;
        applyTempBPMBonus = true;
    }   

   

    private void ToggleFastForward()
    {
        isFastForward = !isFastForward;
        Time.timeScale = isFastForward ? fastForwardMultiplier : 1f;
        Debug.Log($"⏩ Fast Forward {(isFastForward ? "ON" : "OFF")}");
    }

    // ------------------------------------------------------------
    // Skipping / End of Round
    // ------------------------------------------------------------

    public void SkipRound()
    {
        if(GameStateManager.Instance.CurrentState != GameState.RoundActive)
        {
            return;
        }

        ArrowSpawner.Instance.StopAllSpawning();

        if (activeRoundCoroutine != null)
            StopCoroutine(activeRoundCoroutine);

        foreach (var arrow in GameObject.FindGameObjectsWithTag("Arrow"))
            Destroy(arrow);

        StartCoroutine(SkipToEndSequence());
    }

    

    private IEnumerator SkipToEndSequence()
    {
        OnRoundEnd?.Invoke();

        GameStateManager.Instance.SetState(GameState.RoundResults);
        yield return StartCoroutine(CoroutineHelpers.WaitForConfirm(GameState.RoundResultsExit));

        yield return StartCoroutine(roundStatsUI.PlayOutroAnimations());

        GameStateManager.Instance.SetState(GameState.UpgradeSelection);

        currentLevelIndex++;
        UpgradeCardManager.Instance.ShowCardChoices();
    }


    


    /*
    private IEnumerator WaitForItemActivations()
    {
        while (ItemActivationManager.Instance == null)
            yield return null;

        while (ItemActivationManager.Instance.IsActive)
            yield return null;
    }
    */
}
