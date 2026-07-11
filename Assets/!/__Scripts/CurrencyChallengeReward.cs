using UnityEngine;

public class CurrencyChallengeReward : IChallengeReward
{
    public int Priority => 0;
    public float AppearancePercentage => _appearancePercentage;
    public int MaxUses => _maxUses;
    public int UsesRemaining => _usesRemaining;
  
    int _currencyReward;
    int _usesRemaining;
    float _appearancePercentage;
    int _maxUses;

    public CurrencyChallengeReward(int currencyReward, int maxUses = 999, float appearancePercentage = 1.0f)
    {
        _currencyReward = currencyReward;
        _maxUses = maxUses;
        _usesRemaining = _maxUses;
        _appearancePercentage = appearancePercentage;
    }

    public bool ShouldGrantReward(int damage)
    {
        return damage <= 0;
    }

    public bool GrantReward(int damage)
    {
        CurrencyManager.Instance.AddCurrency(_currencyReward, "Perfect Challenge");
        _usesRemaining = Mathf.Max(0, _usesRemaining - 1);
        return true;
    }

    public IChallengeReward Clone()
    {
        return new CurrencyChallengeReward(_currencyReward, _maxUses, _appearancePercentage);
    }
}