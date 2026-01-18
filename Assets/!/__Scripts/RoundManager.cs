using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

#region Data Structures

[System.Serializable]
public class LevelData
{
    public TextAsset levelFile;
}

[System.Serializable]
public class Stage
{
    public string stageName;
    public List<TextAsset> normalLevelFiles;
    

    [Header("Boss")]
    public TextAsset bossLevelFile;
    public BossDefinition bossDefinition;

    public int randomLevelsToPlay = 2;

    [Tooltip("If true, levels can repeat if there aren't enough unique ones.")]
    public bool allowRepeats = false;
}

#endregion

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

    [Header("Player Upgrades")]
    public float bpmBonus = 0f;

    [Header("References")]
    [SerializeField] private ScoreTallyController tallyController;
    [SerializeField] private ArrowSpawner arrowSpawner;
    [SerializeField] private RoundStatsUI roundStatsUI;
    [SerializeField] private float roundDelay = 2f;

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
    public List<Stage> stages;

    // ------------------------------------------------------------
    // Runtime State
    // ------------------------------------------------------------

    [Header("Runtime")]
    [SerializeField] private int currentStageIndex = 0;
    [SerializeField] private int currentLevelIndex = 0;

    private List<TextAsset> currentStageSequence = new();
    private Coroutine activeRoundCoroutine;
    private bool roundActive = false;
    private bool isFastForward = false;

    private bool applyTempBPMBonus = false;

    // ------------------------------------------------------------
    // Round Stats
    // ------------------------------------------------------------

    public int arrowsSpawnedThisRound = 0;
    private int arrowsHitThisRound = 0;
    private int arrowsCritThisRound = 0;

    public float RoundAccuracy =>
        arrowsSpawnedThisRound == 0 ? 0f : (float)arrowsHitThisRound / arrowsSpawnedThisRound;

    public bool PerfectRound =>
        arrowsSpawnedThisRound > 0 && arrowsHitThisRound == arrowsSpawnedThisRound;

    public int ArrowsHitThisRoundCount => arrowsHitThisRound;
    public int ArrowsCritThisRoundCount => arrowsCritThisRound;
    public int ArrowsSpawnedThisRoundCount => arrowsSpawnedThisRound;
    public float RoundBPMMultiplier => 1 + bpmBonus;
    public bool IsBossRound => 
        stages[currentStageIndex].bossLevelFile == currentStageSequence[currentLevelIndex - 1];

    // ------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------

    private void OnEnable()
    {
        ArrowBase.OnArrowResolved += RegisterArrowHit;
        UpgradeCardManager.UpgradeSelectionComplete += StartNextRound;
    }

    private void OnDisable()
    {
        ArrowBase.OnArrowResolved -= RegisterArrowHit;
        UpgradeCardManager.UpgradeSelectionComplete -= StartNextRound;
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

    private void Start()
    {
#if UNITY_EDITOR
        if (preventAutoStart)
            return;
#endif

        if (isTestMode)
        {
            Debug.Log($"▶ TEST MODE: Playing {testLevel?.name}");
            StartCoroutine(PlayTestLevel());
            return;
        }

        CountdownUI.Instance.BeginCountdown(() =>
        {
            StartStage(currentStageIndex);
        });
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

        ResetRoundStats();
        roundActive = true;

        yield return StartCoroutine(
            ArrowSpawner.Instance.HandleSpawning(testLevel, bpmBonus)
        );

        roundActive = false;

        Debug.Log("✔ Test level complete. Returning to editor…");
        yield return new WaitForSeconds(10f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(TestSession.returnScene);
    }

    // ------------------------------------------------------------
    // Stage / Round Flow
    // ------------------------------------------------------------

    private void StartStage(int stageIndex)
    {
        if (stageIndex >= stages.Count)
        {
            Debug.Log("🎉 All stages complete!");
            return;
        }

        Stage stage = stages[stageIndex];
        currentStageSequence.Clear();

        //List<LevelData> shuffled = new(stage.normalLevelFiles);
        List<TextAsset> shuffled = new(stage.normalLevelFiles);
        shuffled.Shuffle();

        if (stage.allowRepeats)
        {
            for (int i = 0; i < stage.randomLevelsToPlay; i++)
                currentStageSequence.Add(shuffled[i % shuffled.Count]);
        }
        else
        {
            int count = Mathf.Min(stage.randomLevelsToPlay, shuffled.Count);
            for (int i = 0; i < count; i++)
                currentStageSequence.Add(shuffled[i]);
        }

        currentStageSequence.Add(stage.bossLevelFile);

        currentLevelIndex = 0;
        StartNextRound();
    }

    public void StartNextRound()
    {
        ResetRoundStats();

        GameStateManager.Instance.SetState(GameState.RoundActive);

        if (activeRoundCoroutine != null)
            StopCoroutine(activeRoundCoroutine);

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

        activeRoundCoroutine = StartCoroutine(PlayRound(levelFile));
    }

    private IEnumerator PlayRound(TextAsset levelFile)
    {
        OnRoundStart?.Invoke();
        roundActive = true;

        Stage stage = stages[currentStageIndex];

        bool isBossRound =
            stage.bossLevelFile == levelFile &&
            stage.bossDefinition != null;

        if (isBossRound)
        {
            BossManager.Instance.StartBoss(stage.bossDefinition);
        }

        Debug.Log($"▶ Playing level: {levelFile.name}");

        yield return StartCoroutine(
            ArrowSpawner.Instance.HandleSpawning(levelFile, bpmBonus)
        );


        // ✅ Wait until obstacles are cleared
        yield return new WaitUntil(() =>
            !ArrowSpawner.Instance.IsSpawning &&
            !ObstacleManager.Instance.AnyActive
        );


        yield return new WaitForSeconds(roundDelay);
        GameStateManager.Instance.SetState(GameState.RoundEnd);


        bool tallyComplete = false;

        ScoreTallyController.RoundEndTallyComplete += Handler;

        void Handler()
        {
            tallyComplete = true;
            Debug.Log("✅ Round end tally complete.");
            ScoreTallyController.RoundEndTallyComplete -= Handler;
        }

        yield return CoroutineHelpers.WaitUntilOrTimeout(() => tallyComplete, 20.0f);


        if (applyTempBPMBonus)
        {
            bpmBonus = 0f;
            applyTempBPMBonus = false;
        }

       
        GameStateManager.Instance.SetState(GameState.ItemActivations);
        yield return StartCoroutine(WaitForItemActivations());

        GameStateManager.Instance.SetState(GameState.RoundSummary);
        yield return StartCoroutine(CoroutineHelpers.WaitForConfirm(GameState.RoundSummaryEnd));

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

    public void RegisterArrowHit(ArrowResolvedData data)
    {
        arrowsHitThisRound++;
        if (data.goalType == Goal.GoalType.Critical)
            arrowsCritThisRound++;
    }

    public void ResetRoundStats()
    {
        arrowsSpawnedThisRound = 0;
        arrowsHitThisRound = 0;
        arrowsCritThisRound = 0;
        //ComboManager.Instance.ResetCombo();
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
        if (!roundActive)
            return;

        ArrowSpawner.Instance.StopAllSpawning();

        if (activeRoundCoroutine != null)
            StopCoroutine(activeRoundCoroutine);

        foreach (var arrow in GameObject.FindGameObjectsWithTag("Arrow"))
            Destroy(arrow);

        roundActive = false;
        StartCoroutine(SkipToEndSequence());
    }

    private IEnumerator SkipToEndSequence()
    {
        OnRoundEnd?.Invoke();

        GameStateManager.Instance.SetState(GameState.RoundSummary);
        yield return StartCoroutine(CoroutineHelpers.WaitForConfirm(GameState.RoundSummaryEnd));

        yield return StartCoroutine(roundStatsUI.PlayOutroAnimations());

        GameStateManager.Instance.SetState(GameState.UpgradeSelection);

        currentLevelIndex++;
        UpgradeCardManager.Instance.ShowCardChoices();
    }


    



    private IEnumerator WaitForItemActivations()
    {
        while (ItemActivationManager.Instance == null)
            yield return null;

        while (ItemActivationManager.Instance.IsActive)
            yield return null;
    }
}
