using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Ability Heal")]
public class FullAbilityHeal : SynergyWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float abilityCostMult = 1.5f;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{HEAL_VALUE}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>";
        replacements["{ABILITY_COST_MULT}"] = $"<color=#{UIColors.ToHex(UIColors.Red)}>{(abilityCostMult-1).ToString("P0")}</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        AbilityChargeManager.Instance.RegisterPermanent(new HealOnFullAbilityUpgradeEffect(abilityCostMult, healAmount), this);
    }
}
