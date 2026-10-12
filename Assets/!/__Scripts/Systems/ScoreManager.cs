using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public static event Action<int> OnScoreAdded;
    public static event Action<int> OnScoreUpdated;
    public static event Action<int> OnHighScoreUpdated;

    [SerializeField] private float abilityChargeScorePerUnit = 500f;
    
    private int _currentScore;
    private readonly List<ScoreScaleModifier> modifiers = new();


    public int CurrentScore => _currentScore;
    public float AbilityChargeScorePerUnit => abilityChargeScorePerUnit;

    // Score Calculation
    //---------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        RunLifecycle.OnRunAbandoned += HandleRunAbandoned;
        RunLifecycle.OnRunRestart += HandleRunRestart;
    }

    private void OnDisable()
    {
        RunLifecycle.OnRunAbandoned -= HandleRunAbandoned;
        RunLifecycle.OnRunRestart -= HandleRunRestart;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void HandleRunAbandoned()
    {
        Reset();
    }

    private void HandleRunRestart()
    {
        Reset();
    }
    
    public void AddScoreModifier(ScoreScaleModifier mod)
    {
        if (mod == null)
            return;

        modifiers.Add(mod);
    }

    public void Reset()
    {
        _currentScore = 0;
        modifiers.Clear();
        OnScoreUpdated?.Invoke(_currentScore);
    }


    public int AddScore(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * CalculateFinalScale());

        _currentScore += adjustedAmount;

        OnScoreUpdated?.Invoke(_currentScore);
        OnScoreAdded?.Invoke(adjustedAmount);

        return adjustedAmount;
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

}