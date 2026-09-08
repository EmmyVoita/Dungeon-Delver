using System.Collections.Generic;
using UnityEngine;

public class SynergyWeightedUpgradeBase : UpgradeBase
{   
    [Header("Shop Weight")]

    [SerializeField]
    private List<UpgradeBase> synergyUpgrades = new();

    [SerializeField, Min(0f)]
    private float weightPerSynergyStack = 0.5f;
    


    public override float GetOfferWeight()
    {
        float weight = base.GetOfferWeight();

        if (Player.Instance == null)
            return weight;

        int synergyStacks = 0;

        // foreach stack of a synery upgrade add a flat weight value
        foreach (UpgradeBase upgrade in synergyUpgrades)
        {
            if (upgrade == null)
                continue;

            synergyStacks += ShopManager.Instance.GetPurchasedCardCount(upgrade);
        }

        weight += synergyStacks * weightPerSynergyStack;

        return Mathf.Max(0f, weight);
    }
}