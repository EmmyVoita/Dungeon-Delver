using System.Collections;
using UnityEngine;

public class ProjectileShieldFreezeUpgrade : UpgradeEffectBase
{
    public AbilityUpgradeBase freezeUpgrade;
    
    public override void Apply(Player player)
    {
        Player.Instance.CurrentAbility.ApplyUpgrade(freezeUpgrade);
    }
}
