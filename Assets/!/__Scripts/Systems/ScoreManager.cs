using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using NUnit.Framework.Interfaces;

public class ScoreManager : MonoBehaviour
{
    
    public static ScoreManager Instance { get; private set; }

    public static event Action<int> OnScoreAdded;
    public static event Action<int> OnScoreUpdated;
    public static event Action<Color> OnScoreFlashColor;
    public static event Action<int> OnHighScoreUpdated;
    
    [Header("References")]
    [SerializeField] private RunStatsTracker runStats;
    [SerializeField] private RoundStatsTracker roundStats;

    
    private int _currentScore;
    private int _overideHighScore;
    //private int _roundTotalScore;
    
    public float abilityChargeScorePerUnit = 500f;

    private readonly List<ScoreScaleModifier> modifiers = new();


    private SaveData saveData;
    private string saveFilePath;

    //public int RoundScoreTotal => CalculateTotalScore();
    public int CurrentScore => _currentScore;
    public int RunScoreTotal => runStats.TotalScore;
    public SaveData SaveData => saveData;
    public RunStatsTracker RunStatsTracker => runStats;
    public RoundStatsTracker RoundStatsTracker => roundStats;

    public int RoundScore => roundStats.Score;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleGameStateChange;
        RunLifecycle.OnRunAbandoned += HandleRunAbandoned;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleGameStateChange;
        RunLifecycle.OnRunAbandoned -= HandleRunAbandoned;
    }

 


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        ResetStats();
        LoadSaveData();
    }



    private void HandleGameStateChange(GameState previous, GameState newState)
    {
        if(newState == GameState.RunLoad)
            ResetStats();

        if(newState == GameState.GameOverTally)
        {
            RunStatsTracker.AddRound(roundStats);
            SaveRunRecord();
        }
    }


  


    public void FlashScoreColor(Color flashColor)
    {
        Debug.Log("Score color flashed to: " + flashColor);
        OnScoreFlashColor?.Invoke(flashColor);
    }



    private void ResetStats()
    {
        RoundStatsTracker.Reset();
        RunStatsTracker.ResetRun();
        RunStatsTracker.PrintStats();
    }

    
    public void SaveRunRecord()
    {
        // Find the run stats tracker if it is not assigned
        if(runStats == null)
           runStats = FindAnyObjectByType<RunStatsTracker>();

        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        string causeOfDeath = RoundManager.Instance.GameComplete ? "Game Win" : $"Killed by {Player.Instance.LastDamageSource}";

        // A run records stores the 
        RunRecord record = new RunRecord
        {
            score = runStats.TotalScore,
            abilityUsed = AbilitySelection.SelectedAbility,
            upgrades = ConvertUpgrades(ShopManager.Instance.AllPurchasedCards),
            damageTaken = runStats.TotalDamageTaken,
            highestCombo = runStats.HighestCombo,
            accuracy = runStats.RunAccuracy,
            critAccuracy = runStats.RunCritRate,
            noDamageRun = runStats.TotalDamageTaken == 0,
            timestamp = DateTime.Now.ToString("o"),
            causeOfDeath = causeOfDeath,
            levelIndex = RoundManager.Instance.LevelIndex,
            totalCurrency = CurrencyManager.Instance.TotalCurrency,
            totalHit = RunStatsTracker.TotalHit,
            totalSpawned = RunStatsTracker.TotalSpawned,
            totalAbilityCharge = runStats.TotalAbilityCharge,
            totalAbilityUsed = runStats.TotalAbilityUsed,
            totalcrit = runStats.TotalCrit,
            totalRerollsGained = runStats.RerollsGained,
            totalRerollsUsed = runStats.RerollsUsed,
            totalHealthHealed = runStats.TotalHealthHealed,
            totalShopsVisited = runStats.TotalShopsVisited
        };

        // add the record to our list of records that we store in a json file
        saveData.records.Add(record);

        SaveDataToFile();
    }

    private List<UpgradeRecord> ConvertUpgrades(Dictionary<UpgradeBase, int> chosenCards)
    {
        List<UpgradeRecord> records = new();

        foreach(var kvp in chosenCards)
        {
            var key = kvp.Key;
            var val = kvp.Value;

            records.Add(new UpgradeRecord
            {
                upgradeId = key.UpgradeID,
                upgradeName = key.displayName,
                count = val
            });
        }

        return records;
    }

    private void SaveDataToFile()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Saved High Score to => {saveFilePath}");
    }

    public void LoadSaveData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            saveData = new SaveData();
        }
    }

    public void ResetSaveData()
    {
        _overideHighScore = -1;

        saveData = new SaveData();

        SaveDataToFile();
    }

    public void OverideHighScore(int amount)
    {
        _overideHighScore = amount;
    }

    public int HighScore(AbilityType abilityType = AbilityType.None)
    {
        if(_overideHighScore != -1)
            return _overideHighScore;
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        if(abilityType == AbilityType.None)
            return saveData.records.Max(h => h.score);
        else
            return saveData.records.Where(a => a.abilityUsed == abilityType).Max(h => h.score);
    }

    public int TotalAbilityUsed(AbilityType abilityType = AbilityType.None)
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        if(abilityType == AbilityType.None)
            return saveData.records.Sum(i => i.totalAbilityUsed);
        else
            return saveData.records.Where(a => a.abilityUsed == abilityType).Sum(i => i.totalAbilityUsed);
    }

    public int HighestCombo(AbilityType abilityType = AbilityType.None)
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        if(abilityType == AbilityType.None)
            return saveData.records.Max(i => i.highestCombo);
        else
            return saveData.records.Where(a => a.abilityUsed == abilityType).Sum(i => i.highestCombo);
    }

    public int TotalAbilityCharge(AbilityType abilityType = AbilityType.None)
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        if(abilityType == AbilityType.None)
            return saveData.records.Sum(i => i.totalAbilityCharge);
        else
            return saveData.records.Where(a => a.abilityUsed == abilityType).Sum(i => i.totalAbilityCharge);
    }

    public int TotalCrits()
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        return saveData.records.Sum(i => i.totalcrit);
    }

    public int TotalRerollsUsed()
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        return saveData.records.Sum(i => i.totalRerollsUsed);
    }

    public int TotalCurrencyGained()
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        return saveData.records.Sum(i => i.totalCurrency);
    }

    public int TotalCardsGained()
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        return saveData.records.Sum(i => i.CardCount);
    }

    public int TotalHealing()
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        return saveData.records.Sum(i => i.totalHealthHealed);
    }

    public int TotalShopsVisited()
    {
        // in case the savedata isn't already loaded, load it
        if(saveData == null)
            LoadSaveData();

        if(saveData.records.Count == 0)
            return 0;

        return saveData.records.Sum(i => i.totalShopsVisited);
    }

    // Score Calculation
    //---------------------------------------------------------------------------------------------
    public int AddScore(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * CalculateFinalScale());

        _currentScore += adjustedAmount;

        OnScoreUpdated?.Invoke(_currentScore);
        OnScoreAdded?.Invoke(adjustedAmount);

        return adjustedAmount;
    }

    public void AddScoreModifier(ScoreScaleModifier mod)
    {
        if (mod == null)
            return;

        modifiers.Add(mod);
    }

    private void ClearModifiers()
    {
        modifiers.Clear();
    }

    private float CalculateFinalScale()
    {
        float result = 1f;

        foreach (ScoreScaleModifier mod in modifiers)
        {
            if (mod == null || !mod.IsActive)
                continue;

            result *= mod.Value;
        }

        return Mathf.Max(result,0f);
    }

    private void HandleRunAbandoned()
    {
        ClearModifiers();
    }
}
