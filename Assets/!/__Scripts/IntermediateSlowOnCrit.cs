using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Slow On Crit")]
public class IntermediateSlowOnCrit : IntermediateEffectSO
{
    public float normalArrowMultiplier = 0.25f;
    public float slowMultiplier = 0.85f;
    public float easeIn = 0.05f;
    public float hold = 0.08f;
    public float easeOut = 0.12f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{NORMAL_ARROW_MULTIPLIER}", (normalArrowMultiplier-1).ToString("P0"));
    }

    public override void Apply()
    {
        var listener = new SlowOnCritListener(
            slowMultiplier,
            easeIn,
            hold,
            easeOut
        );

        UpgradeManager.Instance.AddTemporaryModifier(listener);

        UpgradeManager.Instance.AddTemporaryModifier(
            new TempNormalArrowMultiplier(normalArrowMultiplier)
        );
    }
}
