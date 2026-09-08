using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Currency on Heal")]
public class CurrencyOnHealUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private int currencyAmount;
    [SerializeField] private int healAmountRequired;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{CURRENCY}", $"<color=#{UIColors.ToHex(UIColors.Green)}>${currencyAmount}</color>")
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{healAmountRequired}</color>");
    }

    public override string GetDetails()
    {
         return detailsTemplate
            .Replace("{CURRENCY}", $"<color=#{UIColors.ToHex(UIColors.Green)}>${currencyAmount}</color>")
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{healAmountRequired}</color>")
            .Replace("{MAX_STACKS}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{MaxStacks}</color>");
    }

    public override void Apply()
    {
        HealthChangeEffectsManager.Instance.RegisterPermanent(new CurrencyOnHeal(currencyAmount,healAmountRequired), this);
    }
}
