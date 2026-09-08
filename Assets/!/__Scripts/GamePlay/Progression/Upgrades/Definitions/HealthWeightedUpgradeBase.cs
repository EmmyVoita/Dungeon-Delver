using System.Collections.Generic;
using UnityEngine;

public class HealthWeightedUpgradeBase : UpgradeBase
{   
    [Header("Shop Weight")]

    [SerializeField]
    private List<UpgradeBase> healthSynergyUpgrades = new();

    [SerializeField, Min(0f)]
    private float weightPerSynergyStack = 0.5f;
    
    [SerializeField, Min(0f)]
    private float maximumHealthWeightBonus = 1.5f;

    [SerializeField, Min(1f)]
    private int startingWeighingHealthAt = 10;

    public override float GetOfferWeight()
    {
        float weight = base.GetOfferWeight();

        if (Player.Instance == null)
            return weight;

        int synergyStacks = 0;

        // foreach stack of a synery upgrade add a flat weight value
        foreach (UpgradeBase upgrade in healthSynergyUpgrades)
        {
            if (upgrade == null)
                continue;

            synergyStacks += ShopManager.Instance.GetPurchasedCardCount(upgrade);
        }

        weight += synergyStacks * weightPerSynergyStack;

        // Create a weight value from how much health we are missing, adding a limit.
        if (Player.Instance != null)
        {
            float playerHealthRatio = Mathf.Clamp01((float)Player.Instance.Health / startingWeighingHealthAt);
            float missingHealthRatio = 1f - playerHealthRatio;
            float healthBonus = missingHealthRatio * maximumHealthWeightBonus;

            weight += healthBonus;
        }

        return Mathf.Max(0f, weight);
    }
}