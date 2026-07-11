using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    
    public static ScoreManager Instance { get; private set; }

    public static event Action<int> OnScoreAdded;
    public static event Action<int> OnScoreUpdated;
    public static event Action<Color> OnScoreFlashColor;
    public static event Action<int> OnHighScoreUpdated;

    
    [SerializeField] private int currentScore;
    [SerializeField] private int highScore;
    [SerializeField] private int totalScore;
    [SerializeField] private RunStatsTracker runStats;
    public float abilityChargeScorePerUnit = 500f;

    private Dictionary<ScoreSource, int> breakdown = new();

    public IReadOnlyDictionary<ScoreSource, int> GetBreakdown()
        => breakdown;

    private SaveData saveData;
    private string saveFilePath;

    public int RoundScoreTotal => CalculateTotalScore();
    public int RunScoreTotal => runStats.TotalScore;
    public SaveData SaveData => saveData;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleGameStateChange;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleGameStateChange;
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
        LoadData();
    }

    private void Update()
    {
      
    }

    private void HandleGameStateChange(GameState previous, GameState newState)
    {
        if(newState == GameState.RunLoad)
        {
            ResetScore();
        }

        if (previous == GameState.RoundResultsExit && newState != GameState.RoundResultsExit)
        {
            ResetBreakdown();
        }

        if(newState == GameState.GameOverResults)
        {
            SaveRunRecord();
        }
    }

    public bool TrySpendScore(int amount)
    {
        if (currentScore < amount)
            return false;

        currentScore -= amount;

        OnScoreUpdated?.Invoke(currentScore);

        OnScoreAdded?.Invoke(-amount);

        return true;
    }

    public void ResetBreakdown()
    {
        breakdown.Clear();
        //totalScore = 0;
    }

    public int CalculateTotalScore()
    {
        totalScore = 0;
        foreach (var entry in breakdown)
        {
            totalScore += entry.Value;
        }
        return totalScore;
    }

    public int AddScore(int amount, ScoreSource source)
    {
        float multiplier = UpgradeManager.Instance.ModifyGlobalScoreMultiplier(1f);
        int adjustedAmount = Mathf.RoundToInt(amount * multiplier);

        currentScore += Mathf.RoundToInt(adjustedAmount);
        OnScoreUpdated?.Invoke(currentScore);

        OnScoreAdded?.Invoke(adjustedAmount);


        if (!breakdown.ContainsKey(source))
            breakdown[source] = 0;

        breakdown[source] += adjustedAmount;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            saveData.highScore = highScore;
            SaveDataToFile();

            OnHighScoreUpdated?.Invoke(highScore);
        }

        return adjustedAmount;
    }

    public void FlashScoreColor(Color flashColor)
    {
        Debug.Log("Score color flashed to: " + flashColor);
        OnScoreFlashColor?.Invoke(flashColor);
    }

    public void SetHighScore(int newHighScore)
    {
        highScore = newHighScore;
        saveData.highScore = highScore;
        SaveDataToFile();
        OnHighScoreUpdated?.Invoke(highScore);
    }

    public void ResetHighScore()
    {
        highScore = 0;
        saveData.highScore = highScore;
        SaveDataToFile();
        OnHighScoreUpdated?.Invoke(highScore);
    }

    public void SaveCurrentScore()
    {
        // Add current score to total (if you track cumulative points)
        saveData.totalScore += currentScore;

        // Save high score if necessary
        if (currentScore > highScore)
        {
            highScore = currentScore;
            saveData.highScore = highScore;
            OnHighScoreUpdated?.Invoke(highScore);
        }

        // Save everything to disk
        SaveDataToFile();

        // Reset current run score for next play
        currentScore = 0;

        Debug.Log($"💾 Score saved and reset. HighScore: {highScore}, TotalScore: {saveData.totalScore}");
    }


    public void ResetScore() => currentScore = 0;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int TotalScore => totalScore;

    // -----------------------------
    // 🔄 Save / Load Logic
    // -----------------------------
    private void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
            highScore = saveData.highScore;
            totalScore = saveData.totalScore;


            Debug.Log($"Loaded High Score: {highScore}");
        }
        else
        {
            saveData = new SaveData();
            highScore = 0;
        }
    }

    private void SaveDataToFile()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Saved High Score to {saveFilePath}");
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
                upgradeId = key.upgradeId,
                upgradeName = key.displayName,
                count = val
            });
        }

        return records;
    }

    
    public void SaveRunRecord()
    {
        if(runStats == null)
        {
           runStats = FindAnyObjectByType<RunStatsTracker>();
        }

        RunRecord record = new RunRecord
        {
            score = currentScore,
            abilityUsed = AbilitySelection.SelectedAbility,
            upgrades = ConvertUpgrades(UpgradeCardManager.Instance.AllChosenCards),
            damageTaken = runStats.TotalDamageTaken,
            highestCombo = runStats.HighestCombo,
            accuracy = runStats.RunAccuracy,
            critAccuracy = runStats.RunCritRate,
            noDamageRun = runStats.TotalDamageTaken == 0,
            timestamp = DateTime.Now.ToString("o") 
        };

        Debug.Log(record.ToString());

        saveData.leaderboard.Add(record);

        // update derived stats 
        saveData.timesPlayed++;

        saveData.totalScore += record.score;

        if (record.score > saveData.highScore)
            saveData.highScore = record.score;

        SaveDataToFile();
    }

    public SaveData GetSaveData()
    {
        string json = File.ReadAllText(saveFilePath);
        saveData = JsonUtility.FromJson<SaveData>(json);
        return saveData;
    }
    
}
