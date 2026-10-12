using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Orbiting Shields")]
public class OrbitingShieldsUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int shieldCount = 3;
    [SerializeField] private int currencyAmount = 5;
    
    public override void Apply()
    {
        OnAbilityUsedRMM.Instance.RegisterRenewing(new OrbitingShieldsModifier(shieldCount, currencyAmount), this);
    }


    protected override void AddReplacements(Dictionary<string, string> replacements,  bool isInventory)
    {
        base.AddReplacements(replacements, isInventory);

        replacements["{COUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{shieldCount}</color>";

        replacements["{CURRENCY}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"${currencyAmount:N0}</color>";
    }
}
