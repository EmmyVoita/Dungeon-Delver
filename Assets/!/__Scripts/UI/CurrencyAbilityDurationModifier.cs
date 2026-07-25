using UnityEngine;

public class CurrencyAbilityDurationModifier : IAbilityDurationModifier
{
    public int Priority => 0;
    public int StackCount => _stackCount;

    private readonly int _currencyStep;
    private readonly float _durationPerTier;
    private int _stackCount;

    public CurrencyAbilityDurationModifier(
        int currencyStep,
        float durationPerTier,
        int stackCount = 1)
    {
        _currencyStep = Mathf.Max(1, currencyStep);
        _durationPerTier = Mathf.Max(0f, durationPerTier);
        _stackCount = Mathf.Max(1, stackCount);
    }

    public void AddStack(int amount = 1)
    {
        _stackCount += Mathf.Max(0, amount);
    }

    public float ModifyDuration(float currentDuration)
    {
        int currency = CurrencyManager.Instance.CurrentCurrency;
        int tiers = currency / _currencyStep;

        float durationIncrease =
            tiers *
            _durationPerTier *
            _stackCount;

        return currentDuration + durationIncrease;
    }

    public IRuntimeModifier Clone()
    {
        return new CurrencyAbilityDurationModifier(
            _currencyStep,
            _durationPerTier,
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