using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/HealAndAbilityCharge")]
public class HealAndAbilityChargeUpgrade : UpgradeBase
{
    public int healAmount = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>");
    }

    public override void Apply()
    {
        Player.Instance.HealPlayer(healAmount);
        Player.Instance.AbilityCharge = Player.Instance.MaxAbilityCharge;
    }
}
