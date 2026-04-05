using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Ability Cost Mult")]
public class IntermediateMultiAbilityCost : IntermediateEffectSO
{
    public float tempAbilityCostMult = 0.95f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{TEMP_ABILITY_COST_MULT}", (tempAbilityCostMult-1).ToString("P0"));
    }

    public override void Apply()
    {
        UpgradeManager.Instance.AddTemporaryModifier(
            new TempAbilityCostMultiplier(tempAbilityCostMult)
        );
    }
}
