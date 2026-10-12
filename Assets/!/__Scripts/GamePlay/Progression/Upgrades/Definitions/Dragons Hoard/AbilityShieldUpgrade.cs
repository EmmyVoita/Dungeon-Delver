using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability/AbilityShieldUpgrade")]
public class AbilityShieldUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private float shieldDuration = 1f;
    [SerializeField] private float abilityCostModifier = 1.5f;
    [SerializeField] private GameObject shieldPrefab;

    public override void Apply()
    {
        OnAbilityUsedRMM.Instance.RegisterPermanent(new AbilityShieldModifier(shieldDuration, shieldPrefab), this);
        AbilityChargeManager.Instance.RegisterPermanent(new BasicAbilityCostUpgrade(abilityCostModifier), this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{DURATION}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{shieldDuration}</color>";

        replacements["{ABILITY_COST_MOD}"] =
            $"<color=#{UIColors.ToHex(UIColors.Red)}>" +
            $"{(abilityCostModifier-1).ToString("P0")}</color>";

         base.AddReplacements(replacements, isInventory);
    }
}
