using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using NUnit.Framework.Interfaces;

public class RunHistoryManager : MonoBehaviour
{
    public static event Action<bool> OnSetUnlockAll;
    public static RunHistoryManager Instance { get; private set; }


    [Header("References")]
    [SerializeField] private RunStatsTracker runStats;
    [SerializeField] private RoundStatsTracker roundStats;


    [Header("Testing")]
    [SerializeField] private bool unlockAllOverride;


    private SaveData saveData;
    private string saveFilePath;

   
    public SaveData SaveData => saveData;
    public RunStatsTracker RunStatsTracker => runStats;
    public RoundStatsTracker RoundStatsTracker => roundStats;
    public int RoundScore => roundStats.Score;

    
    public int GetHighScore(AbilityType abilityType = AbilityType.None) { return MaxRecords(i => i.score,abilityType); }
    public int TotalArrowsCaught => SumRecords(i => i.totalHit);
    public int TotalAbilityUsed => SumRecords(i => i.totalAbilityUsed);
    public int HighestCombo => SumRecords(i => i.highestCombo);
    public int TotalAbilityCharge => SumRecords(i => i.totalAbilityCharge);
    public int TotalCrits => SumRecords(i => i.totalcrit);
    public int TotalRerollsUsed => SumRecords(i => i.totalRerollsUsed);
    public int TotalCurrencyGained => SumRecords(i => i.totalCurrency);
    public int TotalCardsGained => SumRecords(i => i.CardCount);
    public int TotalHealing => SumRecords(i => i.totalHealthHealed);
    public int TotalShopsVisited => saveData.records.Sum(i => i.routeRecords.Count(j => j.routeType.IsShop()));


    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleGameStateChange;
        RunLifecycle.OnRunAbandoned += ResetStats;
        Player.OnDeath += HandleDeathEvent;
        RoundManager.OnGameComplete += HandleGameComplete;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleGameStateChange;
        RunLifecycle.OnRunAbandoned -= ResetStats;
        Player.OnDeath -= HandleDeathEvent;
        RoundManager.OnGameComplete -= HandleGameComplete;
    }

    private void HandleGameComplete()
    {
        // We already save the last level when we complete the level in the
        // Round manager, so we dont need to add the roun here, we just
        // need to save it
        SaveRunRecord();
    }

    private void HandleDeathEvent()
    {
        RunStatsTracker.AddRound(roundStats);
        SaveRunRecord();
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
            routeRecords = RunStateManager.Instance.GetRouteHistoryRecords()
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
        saveData = new SaveData();
        SaveDataToFile();
    }

    public void SetUnlockAll(bool value)
    {   
        saveData.unlockAll = value;
        SaveDataToFile();
        OnSetUnlockAll?.Invoke(value);
    }

    public bool GetUnlockAll()
    {
        EnsureSaveDataLoaded();
        return saveData.unlockAll || unlockAllOverride;
    }


    private int SumRecords(Func<RunRecord,int> selector, AbilityType abilityType = AbilityType.None)
    {
        EnsureSaveDataLoaded();

        if(saveData.records.Count == 0)
            return 0;

        if(abilityType == AbilityType.None)
            return saveData.records.Sum(selector);
        else
            return saveData.records.Where(a => a.abilityUsed == abilityType).Sum(selector);
    }

    private int MaxRecords(Func<RunRecord,int> selector, AbilityType abilityType = AbilityType.None)
    {
        EnsureSaveDataLoaded();

        if(saveData.records.Count == 0)
            return 0;

        if(abilityType == AbilityType.None)
            return saveData.records.Max(selector);
        else
            return saveData.records.Where(a => a.abilityUsed == abilityType).Max(selector);
    }

    private void EnsureSaveDataLoaded()
    {
        if (saveData == null)
            LoadSaveData();
    }
}
