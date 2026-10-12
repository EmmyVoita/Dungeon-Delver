using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/ExtraFreeCardPickUpgrade")]
public class ExtraFreeCardPickUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int amount = 1;
    
    public override void Apply()
    {
        RunStateManager.Instance.AddGuaranteedFreePickCardAmount(amount);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{AMOUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{amount}</color>";

         base.AddReplacements(replacements, isInventory);
    }
}
