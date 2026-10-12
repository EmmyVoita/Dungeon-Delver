using UnityEngine;

public class RerollOnFullAbilityEffect : IAbilityCostModifier
{
    public int Priority => 0;

    private float _abilityCostModifier;
    private int _rerollAmount = 1;

    public RerollOnFullAbilityEffect(float abilityCostModifier, int rerollAmount)
    {
        _abilityCostModifier = Mathf.Max(0,abilityCostModifier);
        _rerollAmount = Mathf.Max(0,rerollAmount);
    }


    public float ModifyCost(float currentCost)
    {
        return _abilityCostModifier * currentCost;
    }

    public IRuntimeModifier Clone()
    {
        return new RerollOnFullAbilityEffect(_abilityCostModifier, _rerollAmount);
    }

    public void Destroy()
    {
        Player.OnAbilityFilled -= HandleAbilityFilled;
    }

    public void OnActivate()
    {
        Player.OnAbilityFilled -= HandleAbilityFilled;
        Player.OnAbilityFilled += HandleAbilityFilled;
    }

    private void HandleAbilityFilled()
    {
        CurrencyManager.Instance.PlayRerollPopup(_rerollAmount);
        RunStateManager.Instance.GrantShopReroll(_rerollAmount);
    }
}