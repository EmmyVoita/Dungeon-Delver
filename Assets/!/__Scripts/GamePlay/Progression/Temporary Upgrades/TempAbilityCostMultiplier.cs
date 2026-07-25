using UnityEngine;

public class TempAbilityCostMultiplier : MonoBehaviour//IAbilityCostModifier
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
