using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "Intermediate Effects/Money Discounts Ability Cost")]
public class MoneyDiscountsAbilityCost : CurrencyWeightedUpgradeBase
{
    [SerializeField, Min(0)]
    private int abilityCostAmount = 1;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{ABILITY_COST_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{abilityCostAmount:N0}</color>";
        replacements["{CURRENCY_STEP}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>${currencyStep:N0}</color>";

        base.AddReplacements(replacements,isInventory);
    }

    public override void Apply()
    {
        AbilityChargeManager.Instance.RegisterPermanent(new CurrencyAbilityCostUpgrade(currencyStep,abilityCostAmount), this);
    }
}