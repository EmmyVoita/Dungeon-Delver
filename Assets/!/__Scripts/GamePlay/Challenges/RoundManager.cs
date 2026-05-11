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

    public bool IsTestMode => TestSession.runSingleLevel;
    public TextAsset testLevel => TestSession.tempLevelAsset;

    // ------------------------------------------------------------
    // Settings
    // ------------------------------------------------------------
    
    [Header("State Management")]
    [SerializeField] private GameState postRoundNextState = GameState.WorldMapView;
    [SerializeField] private GameState deathState = GameState.DeathSequence;

    [Header("Dev Controls")]
    [SerializeField] private bool preventAutoStart = false;
    [SerializeField] private float fastForwardMultiplier = 2f;

    [Header("Input")]
    public Key skipRoundKey = Key.R;
    public Key skipStageKey = Key.T; // 👈 new



    [Header("References")]
    [SerializeField] private ScoreTallyController tallyController;
    [SerializeField] private ArrowSpawner arrowSpawner;
    [SerializeField] private RoundStatsUI roundStatsUI;


    [Header("Round End")]
    [SerializeField] private float roundEndDelay = 2f;
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
  
    public RoundStatsTracker stats;
    public RunStatsTracker runStats;
  



   


    public float RoundBPMMultiplier => 1 + _bpmBonus;
    public double RoundStartDSP { get; set; }
    public float  RoundStartTime { get; private set; } // gameplay time

    public float RoundCountdownStartTime { get; private set; }
    public bool IsBossRound => stages[_currentStageIndex].bossLevelFile == _currentStageSequence[_currentLevelIndex - 1];

    public int CurrentLevelIndex => _currentLevel;
    public string LevelIndex => $"{_currentStageIndex+1}-{_currentLevelIndex+1}";


    private List<TextAsset> _currentStageSequence;
    private Coroutine _activeRoundCoroutine;
    private int _currentLevel;
    private bool _isFastForward = false;
    private bool _applyTempBPMBonus = false;
    private float _bpmBonus = 0f;
    private int _currentStageIndex = 0;
    private int _currentLevelIndex = 0;

    // ------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------

    private void OnEnable()
    {
        UpgradeCardManager.UpgradeSelectionComplete += SetupAndStartRound;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
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

        _currentStageSequence = new List<TextAsset>();
        _currentLevelIndex = 0;
        _currentStageIndex = 0;
        _currentLevel = 0;
        
        stats.Reset();
        runStats.ResetRun();

        runStats.PrintStats();
    }

    private void Update()
    {

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleFastForward();

        if (Keyboard.current[skipRoundKey].wasPressedThisFrame)
            SkipRound();
    #endif

        if (IsTestMode && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("⏹ Test aborted. Returning to editor.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(TestSession.returnScene);
        }
    }


    public void ApplyTempBPMBonus(float bonus)
    {
        _bpmBonus = bonus;
        _applyTempBPMBonus = true;
    }   

    private void ToggleFastForward()
    {
        _isFastForward = !_isFastForward;
        Time.timeScale = _isFastForward ? fastForwardMultiplier : 1f;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        #if UNITY_EDITOR
            if (preventAutoStart)
                return;
        #endif  

        if(newState == GameState.RunLoad && ObstacleManager.Instance.TestOn != true)
        {
            StartStage(_currentStageIndex);
        }
        else if(newState == GameState.Editor)
        {
            StartCoroutine(PlayTestLevel());
        }
        
        if(newState == deathState)
        {
            StopAllCoroutines();
            _activeRoundCoroutine = null;

            ArrowSpawner.Instance.StopAllSpawning();
            ArrowSpawner.Instance.ClearAllArrows();
        }

        if(newState == GameState.GameOverTally)
        {
            runStats.AddRound(stats);
        }
    }


    


    private IEnumerator PlayTestLevel()
    {
        if (testLevel == null)
        {
            Debug.LogError("Test level TextAsset is null.");
            yield break;
        }

        stats.Reset();
        runStats.ResetRun();

        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            _activeRoundCoroutine = StartCoroutine(PlayRound(testLevel));
        });
    }

    // ------------------------------------------------------------
    // Stage / Round Flow
    // ------------------------------------------------------------

    private void HandleGameCompletion()
    {
        
    }

    private void StartStage(int stageIndex)
    {
        if(stageIndex == 0)
        {
            runStats.ResetRun();
        }

        if (stageIndex >= stages.Count)
        {
            HandleGameCompletion();
            return;
        }

        MusicManager.Instance.mainClip = stages[stageIndex].musicClip;

        StageData stage = stages[stageIndex];
        _currentStageSequence.Clear();

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
            _currentStageSequence.Add(stage.bossLevelFile);

        _currentLevelIndex = 0;
        SetupAndStartRound();
    }

    private void AddSequentialLevels(StageData stage, List<TextAsset> sourceLevels)
    {
        int count = Mathf.Min(stage.levelsToPlay, sourceLevels.Count);

        for (int i = 0; i < count; i++)
        {
            _currentStageSequence.Add(sourceLevels[i]);
        }
    }

    private void AddRandomLevels(StageData stage, List<TextAsset> sourceLevels)
    {
        sourceLevels.Shuffle();

        if (stage.allowRepeats)
        {
            for (int i = 0; i < stage.levelsToPlay; i++)
            {
                _currentStageSequence.Add(sourceLevels[i % sourceLevels.Count]);
            }
        }
        else
        {
            int count = Mathf.Min(stage.levelsToPlay, sourceLevels.Count);
            for (int i = 0; i < count; i++)
            {
                _currentStageSequence.Add(sourceLevels[i]);
            }
        }
    }




    public void SetupAndStartRound()
    {
    

        // Safety check to prevent starting a new round while one is active
        if (_activeRoundCoroutine != null)
            StopCoroutine(_activeRoundCoroutine);

        // Check if we've completed the current stage
        if (_currentLevelIndex >= _currentStageSequence.Count)
        {
            _currentStageIndex++;
            StartStage(_currentStageIndex);
            return;
        }


        TextAsset levelFile = _currentStageSequence[_currentLevelIndex];
        
        if (levelFile == null)
        {
            Debug.LogError("❌ LevelData has no TextAsset assigned!");
            return;
        }

       
        RoundCountdownStartTime = Time.time;
        
        //GameStateManager.Instance.SetState(GameState.PreRoundCountdown);
        GameStateManager.Instance.RequestStateChange(GameState.PreRoundCountdown);

        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            _activeRoundCoroutine = StartCoroutine(PlayRound(levelFile));
        });
    }


    private IEnumerator PlayRound(TextAsset levelFile)
    {
        OnRoundStart?.Invoke();

        // Reset stats for the new round
        stats.Reset();

        StageData stage = stages[_currentStageIndex];

        // If Boss round, trigger boss logic
        if (stage.bossLevelFile == levelFile && stage.bossDefinition != null)
        {
            BossManager.Instance.StartBoss(stage.bossDefinition);
        }


        GameStateManager.Instance.SetState(GameState.RoundActive);


        if(GameSceneLoader.PendingConfig != null)
        {
            float startTime = GameSessionBootstrap.Config.LevelEditorStartTime;

            yield return StartCoroutine(ArrowSpawner.Instance.PlayFromTime(levelFile, 
                                                                           startTime, 
                                                                           _bpmBonus));
        }
        else
        {
            yield return StartCoroutine(ArrowSpawner.Instance.HandleSpawning(levelFile, 
                                                                             _bpmBonus));
        }
            
        

        // ✅ Wait until obstacles are cleared
        yield return new WaitUntil(() =>
            !ArrowSpawner.Instance.IsSpawning &&
            !ObstacleManager.Instance.AnyActive
        );

        Debug.LogError("Arrows done spawning");


        yield return new WaitForSeconds(roundEndDelay);


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

       

        if (_applyTempBPMBonus)
        {
            _bpmBonus = 0f;
            _applyTempBPMBonus = false;
        }

    
        yield return StartCoroutine(EndOfRoundSequence());
    }

    // ------------------------------------------------------------
    // Stats / Utilities
    // ------------------------------------------------------------



    public void SkipRound()
    {
        if(GameStateManager.Instance.CurrentState != GameState.RoundActive)
            return;

        if (_activeRoundCoroutine != null)
        {
            StopCoroutine(_activeRoundCoroutine);
            _activeRoundCoroutine = null;
        }
            
        ArrowSpawner.Instance.StopAllSpawning();
        ArrowSpawner.Instance.ClearAllArrows();

        OnRoundEnd?.Invoke();

        StartCoroutine(EndOfRoundSequence());
    }


    private IEnumerator EndOfRoundSequence()
    {
        runStats.AddRound(stats);

        _currentLevelIndex++;
        _currentLevel++;

        GameStateManager.Instance.SetState(postRoundNextState);

        yield return StartCoroutine(CoroutineHelpers.WaitForConfirm(GameState.WorldMapViewEnd));
        
        GameStateManager.Instance.RequestStateChange(GameState.UpgradeSelection);
        //GameStateManager.Instance.SetState(GameState.UpgradeSelection);
    }
}
