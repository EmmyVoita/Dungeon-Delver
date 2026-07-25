using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;






public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    
    public static event Action OnRoundStart;
    public static event Action OnRoundEndA;
    public static event Action OnRoundEnd;

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

    [Header("Input")]
    public Key skipRoundKey = Key.R;
    public Key skipStageKey = Key.T; // 👈 new



    [Header("Round End")]
    [SerializeField] private float roundEndDelay = 2f;
    [SerializeField] private float roundEndTimeout = 20f;

    // ------------------------------------------------------------
    // Events
    // ------------------------------------------------------------


    // ------------------------------------------------------------
    // Stages
    // ------------------------------------------------------------

    [Header("Stages")]
    //public List<StageData> stages;
    public List<StageDataObject> stages;

    // ------------------------------------------------------------
    // Runtime State
    // ------------------------------------------------------------

    [Header("Runtime")]
  
    public RoundStatsTracker roundStats;
    public RunStatsTracker runStats;
  



   

    public bool GameComplete => _currentLevelIndex >= _currentStageSequence.Count && _currentStageIndex == Mathf.Max(stages.Count - 1, 0);
    public float RoundBPMMultiplier => 1 + _bpmBonus;
    public double RoundStartDSP { get; set; }
    public float  RoundStartTime { get; private set; } // gameplay time

    public float RoundCountdownStartTime { get; private set; }
    public bool IsBossRound => stages[_currentStageIndex].bossFile.LevelFile == _currentStageSequence[_currentLevelIndex - 1];

    public int CurrentLevelIndex => _currentLevel;
    public string LevelIndex => $"{_currentStageIndex+1}-{_currentLevelIndex+1}";
    
    public int CurrentLevelReward => _currentStageSequence[_currentLevelIndex].BaseCurrencyReward;


    private List<LevelDataObject> _currentStageSequence;
    private Coroutine _activeRoundCoroutine;
    private int _currentLevel;
    private bool _applyTempBPMBonus = false;
    private float _bpmBonus = 0f;
    private int _currentStageIndex = 0;
    private int _currentLevelIndex = 0;


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

        _currentStageSequence = new();
        _currentLevelIndex = 0;
        _currentStageIndex = 0;
        _currentLevel = 0;

        
        
        
    }

    private void Start()
    {
        runStats = ScoreManager.Instance.RunStatsTracker;
        roundStats = ScoreManager.Instance.RoundStatsTracker;
        
        roundStats.Reset();
        runStats.ResetRun();
        runStats.PrintStats();
    }

    private void Update()
    {

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Keyboard.current[skipRoundKey].wasPressedThisFrame)
            SkipRound();
    #endif

        if (IsTestMode && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("⏹ Test aborted. Returning to editor.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(TestSession.returnScene);
        }
    }



    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        #if UNITY_EDITOR
            if (preventAutoStart)
                return;
        #endif  

        if(newState == GameState.DeathSequence)
        {
            //runStats.AddRound(stats);
        }

        if(newState == GameState.RunLoad && ObstacleManager.Instance.TestOn != true)
        {
            StartStage(_currentStageIndex);
        }
        else if(newState == GameState.Editor)
        {
            Player.Instance.HealPlayer(10);
            StartCoroutine(PlayTestLevel());
        }
        
        if(newState != GameState.RoundActive &&  newState != GameState.Editor && newState != GameState.RoundResultsTally && newState != GameState.RoundResultsExit)
        {
            if (_activeRoundCoroutine != null)
            {
                StopCoroutine(_activeRoundCoroutine);
                _activeRoundCoroutine = null;
            }

            ArrowSpawner.Instance.StopAllSpawning();
            ArrowSpawner.Instance.ClearAllArrows();
        }

        if(newState == GameState.GameOverTally)
        {
            runStats.AddRound(roundStats);
        }
    }


    


    private IEnumerator PlayTestLevel()
    {
        if (testLevel == null)
        {
            Debug.LogError("Test level TextAsset is null.");
            yield break;
        }

        roundStats.Reset();
        runStats.ResetRun();

        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            _activeRoundCoroutine = StartCoroutine(PlayRound(testLevel));
        });
    }


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

        StageDataObject stage = stages[stageIndex];
        _currentStageSequence.Clear();


        List<LevelDataObject> sourceLevels = new();

        foreach(LevelDataObject obj in stage.levelFiles)
        {
            sourceLevels.Add(obj);
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
        if (stage.bossFile.LevelFile == null)
        {
            Debug.LogError("Boss file was null");
            return;
        }

        _currentStageSequence.Add(stage.bossFile);
            

        _currentLevelIndex = 0;
        SetupAndStartRound();
    }

    private void AddSequentialLevels(StageDataObject stage, List<LevelDataObject> sourceLevels)
    {
        int count = Mathf.Min(stage.levelsToPlay, sourceLevels.Count);

        for (int i = 0; i < count; i++)
        {
            _currentStageSequence.Add(sourceLevels[i]);
        }
    }

    private void AddRandomLevels(StageDataObject stage, List<LevelDataObject> sourceLevels)
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

        // If we completed all of the levels in the stage, we need to move to next stage or completion
        if (_currentLevelIndex >= _currentStageSequence.Count)
        {
            _currentStageIndex++;

            if (_currentStageIndex >= stages.Count)
            {
                HandleGameCompletion();
                return;
            }

            StartStage(_currentStageIndex);
            return;
        }


        LevelDataObject levelData = _currentStageSequence[_currentLevelIndex];
        
        if (levelData == null)
        {
            Debug.LogError("LevelData has no TextAsset assigned!");
            return;
        }

       
        RoundCountdownStartTime = Time.time;
        
        GameStateManager.Instance.RequestStateChange(GameState.PreRoundCountdown);

        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            _activeRoundCoroutine = StartCoroutine(PlayRound(levelData.LevelFile));
        });
    }


    private IEnumerator PlayRound(TextAsset levelFile)
    {
        OnRoundStart?.Invoke();

        // Reset stats for the new round
        roundStats.Reset();

        StageDataObject stage = stages[_currentStageIndex];

        // If Boss round, trigger boss logic
        if (stage.bossFile.LevelFile == levelFile)
        {
            BossManager.Instance.StartBoss();
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
            
        

        // Wait until obstacles are cleared
        yield return new WaitUntil(() =>
            !ArrowSpawner.Instance.IsSpawning &&
            !ObstacleManager.Instance.AnyActive
        );


        yield return new WaitForSeconds(roundEndDelay);

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

    private IEnumerator HandleEndOfRoundTally()
    {
        bool tallyComplete = false;

        ScoreTallyController.RoundEndTallyComplete += Handler;

        void Handler() => tallyComplete = true;

        GameStateManager.Instance.SetState(GameState.RoundResultsTally);

        yield return CoroutineHelpers.WaitUntilOrTimeout("Waiting for round results tally to complete", 
                                                          () => tallyComplete, 
                                                          roundEndTimeout);

        ScoreTallyController.RoundEndTallyComplete -= Handler;
    }

    private IEnumerator EndOfRoundSequence()
    {
        yield return StartCoroutine(HandleEndOfRoundTally());
        yield return StartCoroutine(CurrencyManager.Instance.EndOfRoundSequence());

        runStats.AddRound(roundStats);

        _currentLevelIndex++;
        _currentLevel++;

        GameStateManager.Instance.SetState(postRoundNextState);

        yield return StartCoroutine(CoroutineHelpers.WaitForJump(GameState.WorldMapViewEnd));

        Debug.Log("Input recieved to continue from world map view end");

       if(GameComplete)
            GameStateManager.Instance.SetState(GameState.GameWin);
        else
            GameStateManager.Instance.RequestStateChange(GameState.UpgradeSelection);
    }

    public void ApplyTempBPMBonus(float bonus)
    {
        _bpmBonus = bonus;
        _applyTempBPMBonus = true;
    }   

}
