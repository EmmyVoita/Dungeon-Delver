using System;
using UnityEngine;

public static class BuffHelpers
{
    public static Action OnGoldenArrowSessionStarted;
  
    public static GoldenArrowEffect GetGoldenBuff(GameObject target)
    {
        return target.GetComponent<GoldenArrowEffect>();
    }
    

    public static GoldenArrowEffect GetOrCreateGoldenEffect(
        int addAmount)
    {
        var effect = ArrowEffectManager.Instance.GetEffect<GoldenArrowEffect>();

        if (effect == null)
        {
            effect = new GoldenArrowEffect(addAmount);
            ArrowEffectManager.Instance.AddOrExtend(effect);
        }
        else
        {
            effect.AddArrows(addAmount);
        }

        return effect;
    }

    public static RecoveryArrowBuff GetOrCreateRecoveryArrow(int addAmount)
    {
        var effect = ArrowEffectManager.Instance.GetEffect<RecoveryArrowBuff>();
        if (effect == null)
        {
            effect = new RecoveryArrowBuff(addAmount);
            ArrowEffectManager.Instance.AddOrExtend(effect);
        }
        else
        {
            effect.AddArrows(addAmount);
        }

        return effect;
    }

    public static TimeSlowArrowBuff GetOrCreateTimeSlowArrow(int addAmount)
    {
        var effect = ArrowEffectManager.Instance.GetEffect<TimeSlowArrowBuff>();
        if (effect == null)
        {
            effect = new TimeSlowArrowBuff(addAmount);
            ArrowEffectManager.Instance.AddOrExtend(effect);
        }
        else
        {
            effect.AddArrows(addAmount);
        }

        return effect;
    }
}
