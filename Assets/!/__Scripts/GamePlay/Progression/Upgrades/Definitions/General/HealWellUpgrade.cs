using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Heal Well")]
public class HealWellUpgrade : HealthWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private List<RewardDefinition> rewards;
    
    public override void Apply()
    {
        WishingWellController.Instance.PlaySequence(rewards);
    }
}
