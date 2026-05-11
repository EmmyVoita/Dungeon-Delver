using UnityEngine;
using System.Collections;

public class SlowTimeDiceBuff : UpgradeEffectBase
{
    public float slowAmount = 0.75f;
    public float lerpDuration = 0.4f;

    public override void Apply(Player target)
    {
        base.Apply(target);
        Debug.Log("🌀 Slow-Time Dice Buff Active!");

        //TimeManager.Instance.SetModifier(slowAmount, lerpDuration);
    }


    public override void Remove()
    {
        Debug.Log("💔 Slow-Time Dice Buff Expired!");
        //TimeManager.Instance.SetModifier(1.0f,0.3f);
        base.Remove();
    }
}
