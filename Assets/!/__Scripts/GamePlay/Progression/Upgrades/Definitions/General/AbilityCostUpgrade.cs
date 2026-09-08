using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability Cost Modifier")]
public class AbilityCostUpgrade : UpgradeBase
{
    public int rerollAmount = 1;
    public float tempAbilityCostMult = 0.95f;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{ABILITY_COST}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{(1-tempAbilityCostMult).ToString("P0")}</color>")
            .Replace("{REROLLS}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{rerollAmount}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
           .Replace("{ABILITY_COST}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{(1-tempAbilityCostMult).ToString("P0")}</color>");
    }


    public override void Apply()
    {
        BasicAbilityCostUpgrade upgrade = new BasicAbilityCostUpgrade(tempAbilityCostMult);
        AbilityChargeManager.Instance.Register(upgrade, this);
        RunStateManager.Instance.GrantShopReroll(rerollAmount);
    }
}
