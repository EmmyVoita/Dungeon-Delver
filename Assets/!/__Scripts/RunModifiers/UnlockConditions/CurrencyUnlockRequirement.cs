using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Currency Requirement")]
public class CurrencyUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int currencyAmount;

    public override string GetRequirementRatioText()
    {
        return $"{RunHistoryManager.Instance.TotalCurrencyGained.ToString("N0")}/{currencyAmount.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Gain Currency";
    }

    public override bool IsUnlocked()
    {
        return RunHistoryManager.Instance.TotalCurrencyGained >= currencyAmount;
    }
}