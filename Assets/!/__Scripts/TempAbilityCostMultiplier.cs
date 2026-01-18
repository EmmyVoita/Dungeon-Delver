using UnityEngine;

public class TempAbilityCostMultiplier : IAbilityCostModifier
{
    private float amount;

    public TempAbilityCostMultiplier(float amount)
    {
        this.amount = amount;
    }

    public float ModifyCost(float baseCost)
    {
        return baseCost * amount;
    }
}
