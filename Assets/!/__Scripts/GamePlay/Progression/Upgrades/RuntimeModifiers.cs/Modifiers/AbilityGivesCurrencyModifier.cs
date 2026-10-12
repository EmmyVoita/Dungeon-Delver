using UnityEngine;

public class AbilityGivesCurrencyModifier : IAbilityEffectModifier
{
    public int Priority => 0;

    private int _currencyAmount;
    private float _timer;

    public AbilityGivesCurrencyModifier(int currencyAmount, float timer)
    {
        _timer = Mathf.Max(0,timer);
        _currencyAmount = currencyAmount;
    }

    public void TriggerEffect()
    {
        if(Player.Instance.TimeSinceAbilityFullyCharged <= _timer)
        {
            CurrencyManager.Instance.AddCurrency(_currencyAmount);
        }
    }

    public IRuntimeModifier Clone()
    {
        return new AbilityGivesCurrencyModifier(_currencyAmount, _timer);
    }

    public void Destroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}