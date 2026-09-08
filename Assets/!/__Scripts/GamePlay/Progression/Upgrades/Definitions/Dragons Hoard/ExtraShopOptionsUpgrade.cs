using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/ExtraShopOptionsUpgrade")]
public class ExtraShopOptionsUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] [Range(0,1)] private float chance = 0.25f;
    
    public override void Apply()
    {
        RunStateManager.Instance.AddShopOptionChance(chance);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{CHANCE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{chance:P0}</color>";

         base.AddReplacements(replacements);
    }
}
