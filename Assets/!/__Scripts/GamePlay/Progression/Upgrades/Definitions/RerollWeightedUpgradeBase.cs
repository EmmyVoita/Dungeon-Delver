using System.Collections.Generic;
using UnityEngine;

public class RerollWeightedUpgradeBase : UpgradeBase
{   
    [Header("Shop Weight")]

    [SerializeField]
    private List<UpgradeBase> rerollSynergyUpgrades = new();

    [SerializeField, Min(0f)]
    private float weightPerSynergyStack = 0.5f;
    
    [SerializeField, Min(0f)]
    private float maximumRerollsWeightBonus = 0.5f;

    [SerializeField, Min(1f)]
    private int startingWeighingRerollsAt = 3;

    public override float GetOfferWeight()
    {
        float weight = base.GetOfferWeight();

        if (Player.Instance == null)
            return weight;

        int synergyStacks = 0;

        // foreach stack of a synery upgrade add a flat weight value
        foreach (UpgradeBase upgrade in rerollSynergyUpgrades)
        {
            if (upgrade == null)
                continue;

            synergyStacks += ShopManager.Instance.GetPurchasedCardCount(upgrade);
        }

        weight += synergyStacks * weightPerSynergyStack;

        // Create a weight value from how much health we are missing, adding a limit.
        if (Player.Instance != null)
        {
            float shopRerollRatio = Mathf.Clamp01((float)RunStateManager.Instance.ShopRerollsRemaining / startingWeighingRerollsAt);
            float missingRerollRatio = 1f - shopRerollRatio;
            float rerollBonus = missingRerollRatio * maximumRerollsWeightBonus;

            weight += rerollBonus;
        }

        return Mathf.Max(0f, weight);
    }
}