using UnityEngine;

[CreateAssetMenu(
    menuName = "Intermediate Effects/Money Discounts Ability Cost")]
public class MoneyDiscountsAbilityCost : UpgradeBase
{
    [SerializeField, Min(1)]
    private int currencyStep = 200;

    [SerializeField, Min(0)]
    private int abilityCostAmount = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace(
                "{ABILITY_COST_AMOUNT}",
                $"<color=#{UIColors.ToHex(UIColors.Green)}>{abilityCostAmount:N0}</color>")
            .Replace(
                "{CURRENCY_STEP}",
                $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyStep:N0}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace(
                "{ABILITY_COST_AMOUNT}",
                $"<color=#{UIColors.ToHex(UIColors.Green)}>{abilityCostAmount:N0}</color>")
            .Replace(
                "{CURRENCY_STEP}",
                $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyStep:N0}</color>");
    }

    public override void Apply()
    {
        AbilityChargeManager.Instance.RegisterPermanent(
            new CurrencyAbilityCostUpgrade(
                currencyStep,
                abilityCostAmount));
        
    }
}