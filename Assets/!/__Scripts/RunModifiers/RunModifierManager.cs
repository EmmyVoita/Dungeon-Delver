using System;
using System.Collections.Generic;
using UnityEngine;

public class RunModifierManager : MonoBehaviour
{
    public static RunModifierManager Instance { get; private set; }

    public static event Action<RunModifierSelection> OnValueChanged;

    [Header("DefinitionVariables")]
    [SerializeField] private float gameSpeedStep = 0.05f;
    [SerializeField] private float currencyGainStep = 0.25f;
    [SerializeField] private int additionalAbilityDamageStep = 999;
    [Range(0,1)][SerializeField] private float normalArrowAbilityChargeStep = 0.25f;
    [Range(0,1)][SerializeField] private float startingAbilityChargeStep = 0.5f;
    [SerializeField] private int startingCurrencyStep = 500;

    [SerializeField] private List<RunModifierSelection> selections = new();

    public IReadOnlyList<RunModifierSelection> Selections => selections;

    private void Awake()
    {
        Instance = this;
    }

    public float GetValue(RunModifierType type)
    {
        foreach (var selection in selections)
        {
            if (selection.definition.type == type)
                return selection.value;
        }

        return 0f;
    }

    public void SetValue(RunModifierDefinition definition,float value)
    {
        RunModifierSelection selection = selections.Find(x => x.definition == definition);

        if (selection == null)
        {
            selection = new RunModifierSelection
            {
                definition = definition,
                value = definition.minValue
            };

            selections.Add(selection);
        }

        selection.value = definition.ClampValue(value);

        OnValueChanged?.Invoke(selection);
    }

    public float GetScoreMultiplier()
    {
        float scoreMultiplier = 1.0f;

        foreach (RunModifierSelection selection in selections)
        {
            if (selection.definition == null)
                continue;

            float scoreChange = selection.StepCount * selection.definition.scoreMultiplierPerStep;

            switch (selection.definition.category)
            {
                case RunModifierCategory.Assist:
                    scoreMultiplier -= scoreChange;
                    break;

                case RunModifierCategory.Challenge:
                    scoreMultiplier += scoreChange;
                    break;
            }
        }

        return scoreMultiplier;
    }

    public void ApplyModifiers()
    {
        foreach(RunModifierSelection selection in selections)
        {
            RunModifierType type = selection.definition.type;

            
            switch(type)
            {
                case RunModifierType.BonusHealing:
                    GameSceneLoader.PendingConfig.AdditionalHealingModifier = (int)GetValue(type);
                    break;
                case RunModifierType.EndOfRoundHeal:
                    GameSceneLoader.PendingConfig.EndOfRoundHealModifier = (int)GetValue(type);
                    break;
                case RunModifierType.CurrencyGain:
                    GameSceneLoader.PendingConfig.CurrencyMultiplier = 1.0f + (int)GetValue(type) * currencyGainStep;
                    break;
                case RunModifierType.RerollsPerLevel:
                    GameSceneLoader.PendingConfig.RerollsToGrant = (int)GetValue(type);
                    break;
                case RunModifierType.StartingRandomCards:
                    GameSceneLoader.PendingConfig.CardsToGrant = (int)GetValue(type);
                    break;
                case RunModifierType.ExtraDamageTaken:
                    GameSceneLoader.PendingConfig.AdditionalDamageModifier = (int)GetValue(type);
                    break;
                case RunModifierType.LevelSpeed:
                    GameSceneLoader.PendingConfig.GameSpeedMultiplier = 1.0f + GetValue(type) * gameSpeedStep;
                    break;
                case RunModifierType.RewardRoute:
                    GameSceneLoader.PendingConfig.AdditionalRewardRoutes = (int)GetValue(type);
                    break;
                case RunModifierType.AbilityDamage:
                    GameSceneLoader.PendingConfig.AdditionalAbilityDamage = (int)GetValue(type) * additionalAbilityDamageStep;
                    break;
                case RunModifierType.NormalArrowAbilityCharge:
                    GameSceneLoader.PendingConfig.NormalArrowAbilityChargeRoll = (int)GetValue(type) * normalArrowAbilityChargeStep;
                    break;
                case RunModifierType.StartingAbilityCharge:
                    GameSceneLoader.PendingConfig.StartingAbilityCharge = GetValue(type) * startingAbilityChargeStep;
                    break;
                case RunModifierType.StartingCurrency:
                    GameSceneLoader.PendingConfig.StartingCurrency = (int)GetValue(type) * startingCurrencyStep;
                    break;
            }
        }

        GameSceneLoader.PendingConfig.ScoreMultiplier = GetScoreMultiplier();
    }

    public string GetFinalValue(RunModifierType type)
    {
        float val = GetValue(type);

        string prefix = val >= 0 ? "+ " : "- ";

        switch (type)
        {
            case RunModifierType.BonusHealing:
                return prefix + val.ToString("N0");

            case RunModifierType.EndOfRoundHeal:
                return prefix +val.ToString("N0");

            case RunModifierType.CurrencyGain:
                // Each step is +50% currency.
                // 3 => 150
                return prefix + $"{val * currencyGainStep * 100}%";

            case RunModifierType.RerollsPerLevel:
                return prefix + val.ToString("N0");

            case RunModifierType.StartingRandomCards:
                return prefix + val.ToString("N0");

            case RunModifierType.ExtraDamageTaken:
                return prefix + val.ToString("N0");
            case RunModifierType.RewardRoute:
                return prefix + val.ToString("N0");

            case RunModifierType.LevelSpeed:
                // Each step is +5% speed.
                // 3 => 15
                return prefix + $"{val * gameSpeedStep * 100}%";
            case RunModifierType.AbilityDamage:
                return prefix + val.ToString("N0");
            case RunModifierType.NormalArrowAbilityCharge:
                return prefix + $"{val * normalArrowAbilityChargeStep * 100}%";
            case RunModifierType.StartingAbilityCharge:
                return prefix + $"{val * startingAbilityChargeStep * 100}%";
            case RunModifierType.StartingCurrency:
                return prefix + (val * startingCurrencyStep).ToString();
        }

        return val.ToString();
    }

    public int GetSpentAssistPoints()
    {
        int spent = 0;

        foreach (RunModifierSelection selection in Selections)
        {
            if (selection.definition.category != RunModifierCategory.Assist)
                continue;

            spent += (int) selection.value;
        }

        return spent;
    }
}