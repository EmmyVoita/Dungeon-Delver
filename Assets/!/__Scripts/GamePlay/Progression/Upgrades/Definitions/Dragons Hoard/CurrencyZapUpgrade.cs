using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/CurrencyZapUpgrade")]
public class CurrencyZapUpgrade : CurrencyWeightedUpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int currencyRequired = 100;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float zapChance = 0.5f;
    
    public override void Apply()
    {
        CoinCollectEffectsManager.Instance.RegisterPermanent(new CoinCollectZap(currencyRequired, true, radius,zapChance), this);
    }

    public override string GetFormattedDescription()
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
}
