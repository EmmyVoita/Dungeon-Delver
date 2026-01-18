using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public static event Action<int> OnScoreUpdated;
    public static event Action<Color> OnScoreFlashColor;
    public static event Action<int> OnHighScoreUpdated;

    [SerializeField] private int currentScore;
    [SerializeField] private int highScore;
    [SerializeField] private int totalScore;
    public float abilityChargeScorePerUnit = 500f;

    private Dictionary<ScoreSource, int> breakdown = new();

    public IReadOnlyDictionary<ScoreSource, int> GetBreakdown()
        => breakdown;

    private SaveData saveData;
    private string saveFilePath;

    public int RoundScoreTotal => CalculateTotalScore();

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

    private void HandleGameStateChange(GameState previous, GameState current)
    {
        if (previous == GameState.RoundSummaryEnd && current != GameState.RoundSummaryEnd)
        {
            ResetBreakdown();
        }
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
}
