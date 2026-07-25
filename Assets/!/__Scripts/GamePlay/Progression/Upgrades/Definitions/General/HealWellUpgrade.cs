using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Heal Well")]
public class HealWellUpgrade : UpgradeBase
{
    [SerializeField] private List<RewardDefinition> rewards;
    
    public override void Apply()
    {
        WishingWellController.Instance.PlaySequence(rewards);
    }
}
