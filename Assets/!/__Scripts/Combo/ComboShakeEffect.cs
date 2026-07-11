using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ScreenShakeTier
{
    public int combo;
    public float shakeDuration;
    public float shakeMagnitude;
}


[CreateAssetMenu(menuName = "ComboEffect/ComboShakeEffect")]
public class ComboShakeEffect : ComboEffect
{
    [SerializeField] private List<ScreenShakeTier> screenShakeTiers;
    [SerializeField] private Vector2 direction;
    [SerializeField] private bool directional;
    [SerializeField] private bool unscaled;

    public override void Initialize()
    {
        screenShakeTiers.Sort((x,y) => x.combo.CompareTo(y.combo));
    }

    public override bool ShouldTrigger(int comboCount)
    {
        if (screenShakeTiers.Count == 0)
            return false;
            
        return comboCount >= screenShakeTiers[0].combo;
    }

    public override void Execute(int comboCount)
    {
        ScreenShakeTier teir = PullTier(comboCount);

        if(teir.shakeDuration == 0 || teir.shakeMagnitude == 0)
            return;

        ScreenShakeRequest request = new ScreenShakeRequest(
                                         teir.shakeDuration,
                                         teir.shakeMagnitude,
                                         direction,
                                         directional,
                                         unscaled);
        
        ScreenShakeManager.Instance.Shake(request);
    }

    private ScreenShakeTier PullTier(int comboCount)
    {
        ScreenShakeTier result = new();

        foreach (var tier in screenShakeTiers)
        {
            if (comboCount >= tier.combo)
                result = tier;
            else
                break;
        }

        return result;
    }
}