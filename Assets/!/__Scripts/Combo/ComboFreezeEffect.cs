using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FreezeFrameTier
{
    public int combo;
    public float freezeDuration;
}


[CreateAssetMenu(menuName = "ComboEffect/ComboFreezeEffect")]
public class ComboFreezeEffect : ComboEffect
{
    [SerializeField] private List<FreezeFrameTier> freezeFrameTiers;
    [SerializeField] private float targetTimeScale = 0;

    public override void Initialize()
    {
        freezeFrameTiers.Sort((x,y) => x.combo.CompareTo(y.combo));
    }

    public override bool ShouldTrigger(int comboCount)
    {
        if (freezeFrameTiers.Count == 0)
            return false;
            
        return comboCount >= freezeFrameTiers[0].combo;
    }

    public override void Execute(int comboCount)
    {
        FreezeFrameTier tier = PullTier(comboCount);

        if(tier.freezeDuration == 0)
            return;

        TimeScaleModifier modifier = new TimeScaleModifier("FreezeFrame", targetTimeScale);

        TimeManager.Instance.AddTemporaryModifier(modifier, tier.freezeDuration);
    }

    private FreezeFrameTier PullTier(int comboCount)
    {
        FreezeFrameTier result = new();

        foreach (var tier in freezeFrameTiers)
        {
            if (comboCount >= tier.combo)
                result = tier;
            else
                break;
        }

        return result;
    }
}