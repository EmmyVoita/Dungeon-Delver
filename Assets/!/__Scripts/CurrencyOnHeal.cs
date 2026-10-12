using UnityEngine;

public class CurrencyOnHeal : IHealthChangeEffect
{
    public int Priority => 0;

    private int _healThreshold;
    private int _currencyAmount;

    public CurrencyOnHeal(int currencyAmount, int healThreshold)
    {
        _healThreshold = healThreshold;
        _currencyAmount = currencyAmount;
    }

    public bool CanTriggerEffect(int changeAmount)
    {
        return changeAmount >= _healThreshold;
    }

    public bool TriggerEffect(int amount)                  
    {
        CurrencyManager.Instance.AddCurrency(_currencyAmount * amount);
        return true;
    }

    public IRuntimeModifier Clone()
    {
        return new CurrencyOnHeal(_currencyAmount, _healThreshold);
    }

    public void Destroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}