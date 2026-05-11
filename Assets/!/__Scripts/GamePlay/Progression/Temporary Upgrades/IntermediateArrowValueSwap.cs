using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Arrow Value Swap")]
public class IntermediateArrowValueSwap: UpgradeBase
{
    public float normalArrowMultiplier = 0.9f;
    public float critArrowMultiplier = 1.1f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{NORMAL_ARROW_MULTIPLIER}", (normalArrowMultiplier-1).ToString("P0"))
            .Replace("{CRIT_ARROW_MULTIPLIER}", (critArrowMultiplier-1).ToString("P0"));
    }

    public override void Apply()
    {
        UpgradeManager.Instance.AddTemporaryModifier(
            new TempNormalArrowMultiplier(normalArrowMultiplier)
        );
        UpgradeManager.Instance.AddTemporaryModifier(
            new TempCritArrowMultiplier(critArrowMultiplier)
        );
    }
}
