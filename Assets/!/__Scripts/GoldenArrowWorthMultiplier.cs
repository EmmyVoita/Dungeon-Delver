using UnityEngine;

public class DragonsCrownGoldenArrowWorthMultiplier : IGoldenArrowWorthModifier
{
    private readonly float multiplier;

    public DragonsCrownGoldenArrowWorthMultiplier(float multiplier)
    {
        this.multiplier = multiplier;
    }

    public int ModifyGoldenArrowWorth(int baseWorth)
    {   
        return Mathf.RoundToInt(baseWorth * DragonHoardManager.Instance.GoldenArrowMultiplier);
    }
}