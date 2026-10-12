using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Support/HealOnShopReroll")]
public class HealOnShopRerollUpgrade : UpgradeBase
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float healChance = 0.25f;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{HEAL_AMOUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{healAmount}</color>";

        replacements["{HEAL_CHANCE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{healChance:P0}</color>";
        
        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        OnShopRerollRMM.Instance.RegisterPermanent(new RerollHealModifier(healChance, healAmount), this);
    }
}
