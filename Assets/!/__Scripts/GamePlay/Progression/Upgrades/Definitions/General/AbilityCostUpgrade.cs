using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability Cost Modifier")]
public class AbilityCostUpgrade : UpgradeBase
{
    public float tempAbilityCostMult = 0.95f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{TEMP_ABILITY_COST_MULT}", (tempAbilityCostMult-1).ToString("P0"));
    }

    public override void Apply()
    {
        BasicAbilityCostUpgrade upgrade = new BasicAbilityCostUpgrade(tempAbilityCostMult);
        AbilityChargeManager.Instance.Register(upgrade);
    }
}
