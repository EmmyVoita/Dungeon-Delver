using UnityEngine;

public class HealOnFullAbilityUpgradeEffect : IAbilityCostModifier
{
    public int Priority => 0;

    private float _abilityCostModifier;
    private int _healAmount = 1;

    public HealOnFullAbilityUpgradeEffect(float abilityCostModifier, int healAmount)
    {
        _abilityCostModifier = Mathf.Max(0,abilityCostModifier);
        _healAmount = Mathf.Max(0,healAmount);
    }


    public float ModifyCost(float currentCost)
    {
        return _abilityCostModifier * currentCost;
    }

    public IRuntimeModifier Clone()
    {
        return new HealOnFullAbilityUpgradeEffect(_abilityCostModifier, _healAmount);
    }

    public void OnDestroy()
    {
        Player.OnAbilityFilled -= HandleAbilityFilled;
    }

    public void OnActivate()
    {
        Player.OnAbilityFilled += HandleAbilityFilled;
    }

    private void HandleAbilityFilled()
    {
        Player.Instance.HealPlayer(_healAmount);
    }
}