using UnityEngine;

public class CurrencyChallengeReward : IChallengeReward
{
    public int Priority => 0;
    public float AppearancePercentage => _appearancePercentage;
    public int MaxUses => _maxUses;
    public int UsesRemaining => _usesRemaining;

    public int StackCount => _stackCount;
    public int CurrencyReward => _baseCurrencyReward * _stackCount;

    private readonly int _baseCurrencyReward;
    private readonly float _appearancePercentage;
    private readonly int _maxUses;

    private int _usesRemaining;
    private int _stackCount;

    public CurrencyChallengeReward(
        int currencyReward,
        int maxUses = 999,
        float appearancePercentage = 1f,
        int stackCount = 1)
    {
        _baseCurrencyReward = currencyReward;
        _maxUses = maxUses;
        _usesRemaining = maxUses;
        _appearancePercentage = appearancePercentage;
        _stackCount = Mathf.Max(1, stackCount);
    }

    public void AddStack(int amount = 1)
    {
        _stackCount += Mathf.Max(0, amount);
    }

    public bool ShouldGrantReward(int damageTaken)
    {
        return damageTaken <= 0;
    }

    public bool GrantReward(int damageTaken)
    {
        CurrencyManager.Instance.AddCurrency(
            CurrencyReward,
            "Perfect Gilded Challenge"
        );

        _usesRemaining = Mathf.Max(0, _usesRemaining - 1);
        return true;
    }

    public IRuntimeModifier Clone()
    {
        return new CurrencyChallengeReward(
            _baseCurrencyReward,
            _maxUses,
            _appearancePercentage,
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