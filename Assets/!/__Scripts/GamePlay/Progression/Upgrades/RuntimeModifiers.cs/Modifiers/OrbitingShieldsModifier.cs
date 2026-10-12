using UnityEngine;

public class OrbitingShieldsModifier : IAbilityEffectModifier
{
    public int Priority => 0;

    private int _shieldCount;
    private int _currencyAmount;

    public OrbitingShieldsModifier(int shieldCount = 1, int currencyAmount = 1)
    {
        _shieldCount = Mathf.Max(1,shieldCount);
        _currencyAmount = currencyAmount;
    }

    public void TriggerEffect()
    {
        OrbitingTreasureController.Instance.AddRelics(_shieldCount, _currencyAmount);
    }

    public IRuntimeModifier Clone()
    {
        return new OrbitingShieldsModifier(_shieldCount, _currencyAmount);
    }

    public void Destroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}