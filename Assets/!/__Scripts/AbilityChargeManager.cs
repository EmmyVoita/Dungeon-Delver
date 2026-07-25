
using UnityEngine;
using System;

public class AbilityChargeManager : RuntimeModifierManager<IAbilityCostModifier>
{
    public static AbilityChargeManager Instance;

    //[SerializeField] private float _abilityCostModifier = 1.0f;

    //public float AbilityCostModifier => _abilityCostModifier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //_abilityCostModifier = PullAbilityChargeModifier();
    }

    protected override void Subscribe()
    {
        CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
    }

    protected override void Unsubscribe()
    {
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
        base.Unsubscribe();
    }

    private void HandleCurrencyChanged(int amount)
    {
    }

    protected override void OnGameStateChanged(
        GameState previousState,
        GameState newState)
    {
    }

    public int GetModifiedAbilityCost(int baseCost)
    {
        float currentCost = baseCost;

        activeModifiers.Sort(
            (a, b) => b.Priority.CompareTo(a.Priority)
        );

        foreach (IAbilityCostModifier modifier in activeModifiers)
        {
            currentCost = modifier.ModifyCost(currentCost);
        }

        return Mathf.RoundToInt(currentCost);
    }
}