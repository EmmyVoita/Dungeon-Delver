using UnityEngine;

public class HealOnCritBuff : UpgradeEffectBase
{
    public int healAmount = 1;

    public override void Apply(Player target)
    {
        base.Apply(target);
        Debug.Log("❤️ Heal-on-Crit Buff Active!");
        //ArrowBase.OnArrowHitGlobal += HandleArrowCaught;
    }

    private void HandleArrowCaught(ArrowBase arrow, Goal.GoalType goalType)
    {
        Debug.Log("💥 Arrow caught event received in HealOnCritBuff.");
        // If crit was caught, heal
        if (goalType == Goal.GoalType.Critical) // Or check a "wasCrit" event arg
        {
            player.HealPlayer(healAmount);
        }
    }

    public override void Remove()
    {
        Debug.Log("💔 Heal-on-Crit Buff Expired!");
       // ArrowBase.OnArrowHitGlobal -= HandleArrowCaught;
        base.Remove();
    }
}
