using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability/AbilityShieldUpgrade")]
public class AbilityShieldUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private float shieldDuration = 1f;
    [SerializeField] private GameObject shieldPrefab;

    public override void Apply()
    {
        OnAbilityUsedRMM.Instance.RegisterPermanent(new AbilityShieldModifier(shieldDuration, shieldPrefab), this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{DURATION}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{shieldDuration}</color>";

         base.AddReplacements(replacements);
    }
}
