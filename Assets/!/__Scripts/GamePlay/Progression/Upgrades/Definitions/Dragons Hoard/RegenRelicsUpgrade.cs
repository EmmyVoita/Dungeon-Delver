using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Regen Relics Upgrade")]
public class RegenRelicsUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int count = 1;
    [SerializeField] private int currencyAmount = 5;
    [SerializeField] private float regenerationInterval = 10f;
    
    public override void Apply()
    {
        TimedEffectManager.Instance.RegisterRenewing(new RelicRegenerationEffect(currencyAmount, regenerationInterval), this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        base.AddReplacements(replacements);

        replacements["{REGEN_INTERVAL}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{regenerationInterval:0.##}</color>";

        replacements["{CURRENCY}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"${currencyAmount:N0}</color>";
        
        replacements["{COUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{count:N0}</color>";
    }
}
