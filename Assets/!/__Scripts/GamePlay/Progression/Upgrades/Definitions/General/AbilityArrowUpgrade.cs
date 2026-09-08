using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability/AbilityArrowUpgrade")]
public class AbilityArrowUpgrade : UpgradeBase
{
    [SerializeField] private float spawnChance = 0.01f;

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{SPAWN_CHANCE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{spawnChance:P0}</color>";
        
        base.AddReplacements(replacements);
    }

    public override void Apply()
    {
        OnArrowSpawnedRMM.Instance.RegisterPermanent(new AbilityEchoArrowModifier(spawnChance), this);
    }
}
