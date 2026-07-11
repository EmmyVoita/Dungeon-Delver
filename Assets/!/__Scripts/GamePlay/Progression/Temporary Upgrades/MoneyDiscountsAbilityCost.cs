using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/MoneyDiscountsAbilityCost")]
public class MoneyDiscountsAbilityCost : UpgradeBase, IAbilityCostModifier
{
    public int currencyStep = 200;
    public float tempAbilityCostMult = 0.95f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{TEMP_ABILITY_COST_MULT}", (tempAbilityCostMult-1).ToString("P0"));
    }

    public override void Apply()
    {
        /*
        UpgradeManager.Instance.AddTemporaryModifier(
            new MoneyAbilityCostModifier(
                currencyStep,
                tempAbilityCostMult));
        */
    }

    public float ModifyCost(float baseCost)
    {
        int currency = CurrencyManager.Instance.CurrentCurrency;

        int tiers = currency / currencyStep;

        float multiplier =
            Mathf.Pow(tempAbilityCostMult, tiers);

        return baseCost * multiplier;
    }
}
