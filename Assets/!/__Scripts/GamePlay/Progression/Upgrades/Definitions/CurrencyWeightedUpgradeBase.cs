using System.Collections.Generic;
using UnityEngine;

public class CurrencyWeightedUpgradeBase : UpgradeBase
{   
    [Header("Shop Weight")]


    [Space(5)]
    [Header("Currency Scaling")]
    [SerializeField,Min(1)] protected int currencyStep;
    [SerializeField, Min(0f)] private float weightPerCurrencyStep = 0.25f;
    [SerializeField, Min(0f)] private float maximumCurrencyWeightBonus = 1.5f;


    [Space(10)]
    [Header("Synergy Scaling")]
    [SerializeField] private List<UpgradeBase> currencySynergyUpgrades = new();
    [SerializeField, Min(0f)] private float weightPerSynergyStack = 0.5f;

   

    public override float GetOfferWeight()
    {
        float weight = base.GetOfferWeight();

        if (ShopManager.Instance == null)
            return weight;

        int synergyStacks = 0;

        // foreach stack of a synery upgrade add a flat weight value
        foreach (UpgradeBase upgrade in currencySynergyUpgrades)
        {
            if (upgrade == null)
                continue;

            synergyStacks += ShopManager.Instance.GetPurchasedCardCount(upgrade);
        }

        weight += synergyStacks * weightPerSynergyStack;

        // Create a weight value from how much currency we own, adding a limit.
        if (CurrencyManager.Instance != null)
        {
            int currentCurrency = CurrencyManager.Instance.CurrentCurrency;

            int completedSteps = currentCurrency / currencyStep;

            float currencyBonus = Mathf.Min(completedSteps * weightPerCurrencyStep, maximumCurrencyWeightBonus);

            weight += currencyBonus;
        }

        Debug.Log($"Money Discounts Ability Cost final weight => {weight}");

        return Mathf.Max(0f, weight);
    }
}