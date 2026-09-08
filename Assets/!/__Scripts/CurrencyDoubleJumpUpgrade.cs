using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Currency Double Jump Upgrade")]
public class CurrencyDoubleJumpUpgrade : CurrencyWeightedUpgradeBase
{
    public int currencyAmount = 25;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{DJ}", $"<color=#{UIColors.ToHex(UIColors.Green)}>Double Jump</color>")
            .Replace("{CURRENCY_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Red)}>${currencyAmount.ToString("N0")}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{DJ}", $"<color=#{UIColors.ToHex(UIColors.Green)}>Double Jump</color>")
            .Replace("{CURRENCY_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Red)}>${currencyAmount.ToString("N0")}</color>");

    }

    public override void Apply()
    {
        JumpEffectsManager.Instance.RegisterPermanent(new CurrencyDoubleJump(currencyAmount), this);
    }
}
