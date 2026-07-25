using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/HealRoulette")]
public class HealRoulette : UpgradeBase
{
    [SerializeField] private List<RewardDefinition> rewards;
    
    public override void Apply()
    {
        RouletteManager.Instance.OpenRoulette(rewards);
    }

    /*
    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CURRENCY_UPGRADE_REQUIRED}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyRequired.ToString("N0")}</color>")
            .Replace("{APPEARANCE_PERCENTAGE}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{zapChance.ToString("P0")}</color>");;
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{CURRENCY_UPGRADE_REQUIRED}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyRequired.ToString("N0")}</color>")
            .Replace("{APPEARANCE_PERCENTAGE}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{zapChance.ToString("P0")}</color>");;
    }
    */
}
