using UnityEngine;

public class MoneyAbilityCostModifier : IAbilityCostModifier
{
    private readonly int currencyStep;
    private readonly float reductionPerStep;

    public MoneyAbilityCostModifier(
        int currencyStep,
        float reductionPerStep)
    {
        this.currencyStep = currencyStep;
        this.reductionPerStep = reductionPerStep;
    }

    public float ModifyCost(float baseCost)
    {
        int currency = CurrencyManager.Instance.CurrentCurrency;

        int tiers = currency / currencyStep;

        float multiplier =
            Mathf.Pow(reductionPerStep, tiers);

        return baseCost * multiplier;
    }
}