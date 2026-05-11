using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Add Temp Score Mult")]
public class IntermediateAddTempScoreMult : UpgradeBase
{
    public float tempScoreMultAmount = 0.05f;
    public float goldenValueMultiplier = 2.0f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{TEMP_SCORE_MULT_AMOUNT}", tempScoreMultAmount.ToString("P0"))
            .Replace("{GOLDEN_VALUE_MULTIPLIER}", (goldenValueMultiplier-1).ToString("P0"));
    }

    public override void Apply()
    {
        UpgradeManager.Instance.AddTemporaryModifier(
            new TempArrowScoreMultiplier(tempScoreMultAmount)
        );
        
          UpgradeManager.Instance.AddTemporaryModifier(
            new TempGoldenValueMulti(goldenValueMultiplier - 1)
        );
    }
}
