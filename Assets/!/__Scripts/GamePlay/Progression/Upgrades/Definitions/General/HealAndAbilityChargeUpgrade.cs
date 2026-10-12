using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/HealAndAbilityCharge")]
public class HealAndAbilityChargeUpgrade : HealthWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    public int healAmount = 1;


    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{HEAL_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>";
        replacements["{FULL_ABILITY_CHARGE}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>Full Ability Charge</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        Player.Instance.HealPlayer(healAmount);
        Player.Instance.AbilityCharge = Player.Instance.MaxAbilityCharge;
    }
}
