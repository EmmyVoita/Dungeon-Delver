using UnityEngine;

public class BasicHeal : UpgradeEffectBase
{
    public int healAmount = 2;

    public override void Apply(Player player)
    {
        player.Health = Mathf.Min(player.MaxHealth, player.Health + healAmount);
        Destroy(this); // only needed once
    }
}

