using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/GoldenArrowAbilityCharge")]
public class GoldenArrowAbilityChargeUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private int amount = 1;

   
    public override void Apply()
    {
        Debug.Log("Recovery arrow apply!");

        GoldenArrowManager.Instance.AddCatchEffect(new GoldenArrowAbilityChargeModifier(amount));
    }

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{AMOUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{amount:N0}</color>";

        base.AddReplacements(replacements);
    }
}
