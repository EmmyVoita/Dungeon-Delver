using System.Collections;
using UnityEngine;

public class SlowTimeWaveUpgrade : UpgradeEffectBase
{
    public AbilityUpgradeBase upgrade;
    
    public override void Apply(Player player)
    {
        Player.Instance.CurrentAbility.ApplyUpgrade(upgrade);
    }
}
