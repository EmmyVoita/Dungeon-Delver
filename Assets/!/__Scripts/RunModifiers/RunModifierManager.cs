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
    [Range(0,1)][SerializeField] private float goldenArrowChargeStep = 0.01f;
    [SerializeField] private int abilityChargeForCurrencyStep = 1;
    [SerializeField] private int startingCurrencyStep = 500;
    [SerializeField] private float additionalGeneralShopWeight = 1.0f;

    [SerializeField] private int critsLoseCurrencyStep = 1;

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
                case RunModifierType.RandomGoldenArrow:
                    GameSceneLoader.PendingConfig.GoldenArrowChance = (int)GetValue(type) * goldenArrowChargeStep;
                    break;
                case RunModifierType.AbilityChargeOnCurrencyGain:
                    GameSceneLoader.PendingConfig.AbilityChargeOnCurrency = (int)GetValue(type) * abilityChargeForCurrencyStep;
                    break;
                case RunModifierType.CritsTakeAbilityCharge:
                    GameSceneLoader.PendingConfig.CritsAbilityChargeMod = -(int)GetValue(type);
                    break;
                case RunModifierType.CritsLoseCurrency:
                    GameSceneLoader.PendingConfig.CritsLoseCurrencyAmount = (int)GetValue(type) * critsLoseCurrencyStep;
                    break;
                case RunModifierType.RotateBoard:
                    GameSceneLoader.PendingConfig.RotateBoard = (int)GetValue(type) > 0;
                    break;
                case RunModifierType.AdditionalGeneralShop:
                    GameSceneLoader.PendingConfig.AdditionalGeneralShopWeight = (int)GetValue(type) * additionalGeneralShopWeight;
                    break;
            }
        }

        GameSceneLoader.PendingConfig.ScoreMultiplier = GetScoreMultiplier();
    }

    public string GetValueString(RunModifierType type)
    {
        switch (type)
        {
            case RunModifierType.CurrencyGain:
                return FormatValueString("+" + $"{currencyGainStep * 100}%", UIColors.Green);
            case RunModifierType.LevelSpeed:
                return FormatValueString("+" + $"{gameSpeedStep * 100}%", UIColors.Red);
            case RunModifierType.NormalArrowAbilityCharge:
                return FormatValueString("+" + $"{normalArrowAbilityChargeStep * 100}%", UIColors.Green);
            case RunModifierType.StartingAbilityCharge:
                return FormatValueString("+" + $"{startingAbilityChargeStep* 100}%", UIColors.Green);
            case RunModifierType.StartingCurrency:
                return FormatValueString("+" + startingCurrencyStep.ToString(), UIColors.Green);
            case RunModifierType.RandomGoldenArrow:
                return FormatValueString("+" + $"{goldenArrowChargeStep * 100}%", UIColors.Green);
            case RunModifierType.AbilityChargeOnCurrencyGain:
                return FormatValueString("+" + abilityChargeForCurrencyStep.ToString("N0"), UIColors.Green);
            case RunModifierType.CritsTakeAbilityCharge:
                return FormatValueString("-1",UIColors.Red);
            case RunModifierType.CritsLoseCurrency:
                return FormatValueString("+" + critsLoseCurrencyStep.ToString("N0"), UIColors.Red);
            case RunModifierType.ExtraDamageTaken:
                return FormatValueString("+1",UIColors.Red);
            case RunModifierType.AdditionalGeneralShop:
                return FormatValueString("x" + (1.0f + additionalGeneralShopWeight).ToString("N0"), UIColors.Red);
            default:
                return FormatValueString("+1", UIColors.Green);
        }
    }

    private string FormatValueString(string input, Color color)
    {
        return $"<color=#{UIColors.ToHex(color)}>{input}</color>";
    }

    public string GetFinalValue(RunModifierType type)
    {
        float val = GetValue(type);

        string prefix = val >= 0 ? "+ " : "- ";

        switch (type)
        {
            case RunModifierType.BonusHealing:
                return FormatValueString(prefix + val.ToString("N0"), UIColors.Green);
            case RunModifierType.EndOfRoundHeal:
                return  FormatValueString(prefix +val.ToString("N0"), UIColors.Green);
            case RunModifierType.CurrencyGain:
                return  FormatValueString(prefix + $"{val * currencyGainStep * 100}%", UIColors.Green);
            case RunModifierType.RerollsPerLevel:
                return FormatValueString(prefix + val.ToString("N0"), UIColors.Green);
            case RunModifierType.StartingRandomCards:
                return FormatValueString(prefix + val.ToString("N0"), UIColors.Green);
            case RunModifierType.ExtraDamageTaken:
                return FormatValueString(prefix + val.ToString("N0"), UIColors.Red);
            case RunModifierType.RewardRoute:
                return FormatValueString(prefix + val.ToString("N0"), UIColors.Green);
            case RunModifierType.LevelSpeed:
                return FormatValueString(prefix + $"{val * gameSpeedStep * 100}%", UIColors.Red);
            case RunModifierType.AbilityDamage:
                return FormatValueString($"{val * 100}%",UIColors.Red);
            case RunModifierType.NormalArrowAbilityCharge:
                return FormatValueString(prefix + $"{val * normalArrowAbilityChargeStep * 100}%",UIColors.Green);
            case RunModifierType.StartingAbilityCharge:
                return FormatValueString(prefix + $"{val * startingAbilityChargeStep * 100}%",UIColors.Green);
            case RunModifierType.StartingCurrency:
                return FormatValueString(prefix + (val * startingCurrencyStep).ToString(),UIColors.Green);
            case RunModifierType.RandomGoldenArrow:
                return FormatValueString(prefix + $"{val * goldenArrowChargeStep * 100f}%",UIColors.Green);
            case RunModifierType.AbilityChargeOnCurrencyGain:
                return FormatValueString(prefix + (val *  abilityChargeForCurrencyStep).ToString("N0"),UIColors.Green);
            case RunModifierType.CritsTakeAbilityCharge:
                return FormatValueString(val > 0 ? "Disabled" : "Enabled",UIColors.Red);
            case RunModifierType.CritsLoseCurrency:
                return FormatValueString(prefix + (val * critsLoseCurrencyStep).ToString("N0"),UIColors.Red);
            case RunModifierType.RotateBoard:
                return FormatValueString(val > 0 ? "Enabled" : "Disabled",UIColors.Red);
            case RunModifierType.AdditionalGeneralShop:
                return FormatValueString("x" + (1.0f + (val * additionalGeneralShopWeight)).ToString("N0"),UIColors.Green);
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