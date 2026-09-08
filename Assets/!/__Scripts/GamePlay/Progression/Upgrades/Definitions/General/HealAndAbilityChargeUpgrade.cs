using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/HealAndAbilityCharge")]
public class HealAndAbilityChargeUpgrade : HealthWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    public int healAmount = 1;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>")
            .Replace("{FULL_ABILITY_CHARGE}", $"<color=#{UIColors.ToHex(UIColors.Green)}>Full Ability Charge</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>")
            .Replace("{FULL_ABILITY_CHARGE}", $"<color=#{UIColors.ToHex(UIColors.Green)}>Full Ability Charge</color>");
    }

    public override void Apply()
    {
        Player.Instance.HealPlayer(healAmount);
        Player.Instance.AbilityCharge = Player.Instance.MaxAbilityCharge;
    }
}
