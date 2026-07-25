using UnityEngine;

public class BasicAbilityCostUpgrade : IAbilityCostModifier
{
    public int Priority => 0;

    private readonly float _abilityCostModifier;

    public BasicAbilityCostUpgrade(float abilityCostModifier)
    {
        _abilityCostModifier = Mathf.Max(0,abilityCostModifier);
    }


    public float ModifyCost(float currentCost)
    {
        return _abilityCostModifier * currentCost;
    }

    public IRuntimeModifier Clone()
    {
        return new BasicAbilityCostUpgrade(_abilityCostModifier);
    }

    public void OnDestroy()
    {
        
    }

     public void OnActivate()
    {
        
    }
}