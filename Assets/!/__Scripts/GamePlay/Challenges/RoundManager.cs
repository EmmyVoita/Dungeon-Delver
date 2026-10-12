using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;




public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }
    public static event Action OnRoundStart;
    public static event Action OnRoundEndA;
    public static event Action OnRoundEnd;
    public static event Action OnGameComplete;


    [Header("Dev Controls")]
    [SerializeField] private bool preventAutoStart = false;


    [Header("References")]
    [SerializeField] private BackgroundVisualManager visualManager;


    [Header("Stages")]
    [SerializeField] private StageDatabase stageDatabase;
    //public List<StageDataObject> stages;


    [Header("State Management")]
    [SerializeField] private GameState deathState = GameState.DeathSequence;


    [Header("Input")]
    public Key skipRoundKey = Key.R;
    public Key skipStageKey = Key.T; // 👈 new


    [Header("Round End")]
    [SerializeField] private float roundEndDelay = 2f;
    [SerializeField] private float roundEndTimeout = 20f;

  



   
    public List<StageDataObject> StageObjects => stageDatabase.Stages;
    public int StageCount => stageDatabase != null ? stageDatabase.Stages.Count : 0;


    public bool IsTestMode => TestSession.runSingleLevel;
    public TextAsset testLevel => TestSession.tempLevelAsset;
    public bool GameComplete => _currentLevelIndex >= _currentStageSequence.Count && _currentStageIndex == Mathf.Max(stageDatabase.Stages.Count - 1, 0);
    public double RoundStartDSP { get; set; }
    public float  RoundStartTime { get; private set; } // gameplay time
    public int TotalCurrentLevelIndex => _currentLevel;
    public int CurrentStageIndex => _currentStageIndex;
    public string LevelIndex => $"{_currentStageIndex+1}-{_currentLevelIndex+1}";
    public int CurrentLevelReward => _currentStageSequence[_currentLevelIndex].BaseCurrencyReward;
    public string CurrentStageName => stageDatabase.Stages[_currentStageIndex].stageName;
    public bool IsAdvancingToNextStage => _currentLevelIndex >= _currentStageSequence.Count && _currentStageIndex < stageDatabase.Stages.Count - 1;


    public string NextStageName
    {
        get
        {
            int nextStageIndex = _currentStageIndex + 1;

            if (nextStageIndex < 0 || nextStageIndex >= stageDatabase.Stages.Count)
                return CurrentStageName;

            return stageDatabase.Stages[nextStageIndex].stageName;
        }
    }


    private List<LevelDataObject> _currentStageSequence;
    private Coroutine _activeRoundCoroutine;
    private int _currentLevel;
    private int _currentStageIndex = 0;
    private int _currentLevelIndex = 0;
    private bool _gameCompleted = false;


    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
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
        _gameCompleted = false;
    }

    private void Start()
    {
        StartSession();
    }

    
    private void Update()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Keyboard.current[skipRoundKey].wasPressedThisFrame)
                SkipRound();
        #endif

        if (IsTestMode && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(TestSession.returnScene);
        }
    }



    private void StartSession()
    {
        if (GameSessionBootstrap.Config == null)
        {
            Debug.LogError("No GameSessionConfig found.");
            return;
        }

        // Currently we handle the normal run load in the game session bootstrap
        // & the run intro controller. The run intro controller plays the run 
        // intro and then sets the state to run load where our event listener
        // here waits for the run load state and then starts the first stage
        switch (GameSessionBootstrap.Config.Mode)
        {
            case GameMode.LevelEditorTest:
                Player.Instance.HealFull();
                GameStateManager.Instance.SetState(GameState.Editor);
                StartCoroutine(PlayTestLevel());
                break;
        }
    }



    // this is a function to help with testing
    public void SkipRound()
    {
        // we can only skip the round while one is active
        if(GameStateManager.Instance.CurrentState != GameState.RoundActive)
            return;

        StopActiveRound();

        OnRoundEnd?.Invoke();

        StartCoroutine(EndOfRoundSequence());
    }

    // When we stop a round we want to make sure that we clean up everything
    // we define a function in our arrowspawner that calls to kill all active
    // arrows. we define a similar function in our obstacle manager that will
    // end all active challenges.
    private void StopActiveRound()
    {
        if (_activeRoundCoroutine != null)
        {
            StopCoroutine(_activeRoundCoroutine);
            _activeRoundCoroutine = null;
        }
            
        ArrowSpawner.Instance.StopAllSpawning();
        ArrowSpawner.Instance.ClearAllArrows();
        ObstacleManager.Instance.ForceClearAll();
    }


    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        #if UNITY_EDITOR
            if (preventAutoStart)
                return;
        #endif  

        if(GameSceneLoader.PendingConfig != null && GameSceneLoader.PendingConfig.IsEditorMode)
            return;
        
        if(ObstacleManager.Instance.TestOn == true)
            return;

        if(newState == GameState.RunLoad)
            StartStage(_currentStageIndex);
        
        // This is a safety fall. We check whether were in a round state,
        // and then if we are no longer in a round state for whether or not 
        // to manually call to stop the active round.
        // This relys on the assumtion that the round logic will only start
        // being active when we transition into the round states.
        if(previousState.IsRoundState() && !newState.IsRoundState())//previousState.IsRoundState() && !newState.IsRoundState())
        {
            StopActiveRound();
        }                                                                                                                                                                                          
    }


    


    private IEnumerator PlayTestLevel()
    {
        if (testLevel == null)
        {
            Debug.LogError("Test level TextAsset is null.");
            yield break;
        }

        RunHistoryManager.Instance.RoundStatsTracker.Reset();
        RunHistoryManager.Instance.RunStatsTracker.ResetRun();

        MusicManager.Instance.mainClip = TestSession.levelMusic;

        CountdownUI.Instance.BeginCountdown(() =>
        {
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;
            _activeRoundCoroutine = StartCoroutine(PlayRound(testLevel));
        });
    }




    private void StartStage(int stageIndex)
    {   
        // When we start the first stage, reset the run stats tracker. 
        // We need to do this because scoremanager, which maintains a reference 
        // to both the round stats tracker and run stats tracker, is dont destory
        // on load so it persists between runs and if we reset the game.
        if(stageIndex == 0)
            RunHistoryManager.Instance.RunStatsTracker.ResetRun();

        /*
        // Each stage has a different music file, so we need to update that
        // in our music manager
        MusicManager.Instance.mainClip = stages[stageIndex].musicClip;
        */

        // Our stage data object holds a reference to a list of leveldataobjects.
        // LevelDataObject holds a reference to the level file alongside rewards 
        // for completetion. Here we are grabbing the levels from the stage data object
        // and passing it to a function to add levels sequentially.
        StageDataObject stage = stageDatabase.Stages[stageIndex];
        List<LevelDataObject> sourceLevels = stage.levelFiles;

        // Add levels to a list 
        AddSequentialLevels(stage, sourceLevels);

        // Currently we have custom backgrounds for different stages
        // this is something that i am experimenting with so i have a
        // setting to toggle it on and off. Here we update the background
        // when we move to a new stage.
        if(!BackgroundPreference.UseSimpleBackground)
            visualManager.ChangeDefaultBackground(stage.backgroundData);
            
        // Reset the current level index back to 0 when we start a new stage
        _currentLevelIndex = 0;

        SetupAndStartRound();
    }

    private void AddSequentialLevels(StageDataObject stage, List<LevelDataObject> sourceLevels)
    {
        // _currentStageSequence is a list that holds the current stage's level files
        // We call addsequentiallevels to add the normal levels, then after we add the boss
        
        _currentStageSequence.Clear();
       
        int count = Mathf.Min(stage.levelsToPlay, sourceLevels.Count);

        for (int i = 0; i < count; i++)
        {
            _currentStageSequence.Add(sourceLevels[i]);
        }

        // Always add the boss level to be last
        if (stage.bossFile.LevelFile != null)
            _currentStageSequence.Add(stage.bossFile);
    }

    // Setup and start round is THE function that we call to begin playing 
    // the next level We rely on other scripts to call this function.
    public void SetupAndStartRound()
    {
        // Safety check to prevent starting a new round while one is active
        if (_activeRoundCoroutine != null)
            StopCoroutine(_activeRoundCoroutine);


        // If we completed all of the levels in the stage, we need to move 
        // to next stage or completion
        if (_currentLevelIndex >= _currentStageSequence.Count)
        {
            _currentStageIndex++;

            StartStage(_currentStageIndex);
            return;
        }

        // We grab the level data to play by using our current level index.
        // We previously, when we loaded the stage, added all the stage level files
        // into _currentStageSequence
        LevelDataObject levelData = _currentStageSequence[_currentLevelIndex];
        
        // safety check 
        if (levelData == null)
        {
            Debug.LogError("LevelData has no TextAsset assigned!");
            return;
        }
        
        // We have an inbetween game state that represents when we start the countdown
        // below and when that countdown ends. The purpose of this state is just to 
        // to be able to leave our like world map view state and call functions on leaving that
        // while avoiding imeditaley transition to the round active state where functions would
        // prematurely be called if we immediatley transitioned to that.
        GameStateManager.Instance.SetState(GameState.PreRoundCountdown);

        MusicManager.Instance.mainClip = levelData.MusicClip;

        // We use a countdown when we start a level, and we have a dedicated
        // countdown manager because we use it elsewhere when we like unpause
        // from our pause menu during the level.   
        CountdownUI.Instance.BeginCountdown(() =>
        {
          

            // Managing having the arrows be percisely liked to the music audio time we 
            // need to use dsptime. So we set that exactly when we call to start the round.
            RoundStartDSP  = AudioSettings.dspTime;
            RoundStartTime = Time.time;

            _activeRoundCoroutine = StartCoroutine(PlayRound(levelData.LevelFile));
        });
    }


    private IEnumerator PlayRound(TextAsset levelFile)
    {
        OnRoundStart?.Invoke();

        // Reset stats for the new round
        RunHistoryManager.Instance.RoundStatsTracker.Reset();

        // If Boss round, trigger boss logic
        StageDataObject stage = stageDatabase.Stages[_currentStageIndex];

        if (stage.bossFile.LevelFile == levelFile)
            BossManager.Instance.StartBoss();

        // Increment the game state
        GameStateManager.Instance.SetState(GameState.RoundActive);

        // We use the same function for a normal game mode and our level editor test
        // gamemode. In our level editor, we provide a function to start the level at a 
        // specific time to make it easier to test things. By default the value of 
        // level editor start time is 0, so this does nothing for the normal game mode.
        // Game Session Bootstrap should never be null, but just in case we fall back to just
        // using 0 start time.
        if(GameSessionBootstrap.Config != null)
        {
            float startTime = GameSessionBootstrap.Config.LevelEditorStartTime;
            yield return StartCoroutine(ArrowSpawner.Instance.PlayFromTime(levelFile, startTime));
        }
        else
        {
            yield return StartCoroutine(ArrowSpawner.Instance.PlayFromTime(levelFile, 0));
        }
            
        // Wait until obstacles are cleared and our arrows are wll resolved
        yield return new WaitUntil(() => ArrowSpawner.Instance.AllGameplayResolved);

        // wait a short delay after the level is over
        yield return new WaitForSeconds(roundEndDelay);

        yield return StartCoroutine(EndOfRoundSequence());
    }




    private IEnumerator EndOfRoundSequence()
    {
        // We have a tally at the end of the level for adding our combo and
        // remaining ability charge
        yield return StartCoroutine(HandleEndOfRoundTally());

        // We have a bunch of rewards that we give the player at the end of the level
        // after we complete the tally like giving currency for combo or our level's
        // base reward. The coroutine loops through all of these rewards then returns
        yield return StartCoroutine(CurrencyManager.Instance.EndOfRoundSequence());

        // We check for the game win condition here at the end of the round
        // Our GameComplete bool is defined as ->
        // _currentLevelIndex >= _currentStageSequence.Count && _currentStageIndex == Mathf.Max(stages.Count - 1, 0);
        // which says that we are trying to advance to the next level on the last stage
        if(!GameComplete)
        {
            _currentLevelIndex++;
            _currentLevel++;
        }

        // This round is finished, so ALWAYS add its stats.
        RunHistoryManager.Instance.RunStatsTracker.AddRound(RunHistoryManager.Instance.RoundStatsTracker);

        if(GameComplete && !_gameCompleted)
        {
            OnGameComplete?.Invoke();
            GameStateManager.Instance.SetState(GameState.GameWin);
            _gameCompleted = true;
        }
        else
        {  
            // Here we handle which state that we transition to after the end of the round
            // We have to options where we instantly choose and get a reward, or we choose
            // a reward for the next level.
            switch(BetweenRoundFlowManager.Instance.RewardTiming)
            {
                case RouteRewardTiming.BeforeNextLevel:
                    GameStateManager.Instance.SetState(GameState.WorldMapView);
                break;
                    case RouteRewardTiming.AfterCurrentLevel:
                    GameStateManager.Instance.SetState(GameState.RoundReward);
                break;
            }
        }
    }

    // This coroutine just waits until the end of round tally completes
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
}
