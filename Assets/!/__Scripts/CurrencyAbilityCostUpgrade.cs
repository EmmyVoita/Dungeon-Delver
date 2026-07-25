using UnityEngine;

public class CurrencyAbilityCostUpgrade : IAbilityCostModifier
{
    public int Priority => 0;
    public int StackCount => _stackCount;

    private readonly int _currencyStep;
    private readonly int _abilityCostAmount;

    private int _stackCount;

    public CurrencyAbilityCostUpgrade(
        int currencyStep,
        int abilityCostAmount,
        int stackCount = 1)
    {
        _currencyStep = Mathf.Max(1, currencyStep);
        _abilityCostAmount = Mathf.Max(0, abilityCostAmount);
        _stackCount = Mathf.Max(1, stackCount);
    }

    public void AddStack(int amount = 1)
    {
        _stackCount += Mathf.Max(0, amount);
    }

    public float ModifyCost(float currentCost)
    {
        int currency = CurrencyManager.Instance.CurrentCurrency;
        int tiers = currency / _currencyStep;

        int chargeReduction =
            tiers *
            _abilityCostAmount *
            _stackCount;

        return Mathf.Max(0f, currentCost - chargeReduction);
    }

    public IRuntimeModifier Clone()
    {
        return new CurrencyAbilityCostUpgrade(
            _currencyStep,
            _abilityCostAmount,
            _stackCount
        );
    }

    public void OnDestroy()
    {
        
    }

     public void OnActivate()
    {
        
    }
}