using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Golden Value Multiplier")]
public class IntermediateGoldenMulti : UpgradeBase
{
    public float goldenValueMultiplier = 2.0f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{GOLDEN_VALUE_MULTIPLIER}", (goldenValueMultiplier-1).ToString("P0"));
    }

    public override void Apply()
    {
        StatModifierManager.Instance.AddTemporaryModifier(
            new TempGoldenValueMulti(goldenValueMultiplier - 1)
        );
    }
}
