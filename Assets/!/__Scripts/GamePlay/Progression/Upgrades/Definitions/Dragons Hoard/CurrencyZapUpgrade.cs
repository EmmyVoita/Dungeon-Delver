using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/CurrencyZapUpgrade")]
public class CurrencyZapUpgrade : CurrencyWeightedUpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int currencyRequired = 100;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float zapChance = 0.5f;
    [SerializeField] private float enhancedZapChance = 0.5f;
    
    [SerializeField] private Gradient gradient;
    [SerializeField] private Gradient empoweredGradient;
    
    public override void Apply()
    {
        CoinCollectEffectsManager.Instance.RegisterPermanent(new CoinCollectZap(gradient, 
                                                                                empoweredGradient, 
                                                                                currencyRequired, 
                                                                                true, 
                                                                                radius,
                                                                                zapChance,
                                                                                enhancedZapChance), this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{CURRENCY_UPGRADE_REQUIRED}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyRequired.ToString("N0")}</color>";
        replacements["{APPEARANCE_PERCENTAGE}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{zapChance.ToString("P0")}</color>";
        replacements["{APPEARANCE_PERCENTAGE_2}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{enhancedZapChance.ToString("P0")}</color>";

        base.AddReplacements(replacements, isInventory);
    }
}
