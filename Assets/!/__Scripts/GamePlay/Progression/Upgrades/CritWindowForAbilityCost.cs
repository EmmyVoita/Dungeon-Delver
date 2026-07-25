using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Crit Window For Cost")]
public class CritWindowForAbilityCost : UpgradeBase, ICritWindowModifier
{
    public float critWindowMultiplier = 1.25f;
    public float abilityCostMultiplier = 1.5f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CRIT_WINDOW_MULTIPLIER}", (critWindowMultiplier - 1).ToString("P0"))
            .Replace("{ABILITY_COST_MULTIPLIER}", (abilityCostMultiplier - 1).ToString("P0"));
    }

    public float ModifyCritWindow(float current)
        => current * critWindowMultiplier;

    public float ModifyCost(float baseCost)
        => baseCost * abilityCostMultiplier;
}
