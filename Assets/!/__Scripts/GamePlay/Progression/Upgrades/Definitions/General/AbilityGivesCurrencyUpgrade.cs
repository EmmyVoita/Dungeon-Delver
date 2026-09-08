using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Currency Build/AbilityGivesCurrencyUpgrade")]
public class AbilityGivesCurrencyUpgrade : UpgradeBase
{
    [SerializeField] private int currencyAmount = 1;
    [SerializeField] private float timer = 3f;

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{CURRENCY_AMOUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"${currencyAmount}</color>";

        replacements["{TIMER}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{timer}</color>";
        
        base.AddReplacements(replacements);
    }

    public override void Apply()
    {
        OnAbilityUsedRMM.Instance.RegisterPermanent(new AbilityGivesCurrencyModifier(currencyAmount, timer), this);
    }
}
