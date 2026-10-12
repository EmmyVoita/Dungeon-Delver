using UnityEngine;

public class UpgradeCategoryCostModifier : IUpgradeCostModifier
{
    public int Priority => 0;

    private UpgradeCategory _targetCategory;
    private float _costModifier;

    public UpgradeCategoryCostModifier(UpgradeCategory category, float costMod)
    {
        _targetCategory = category;
        _costModifier = costMod;
    }

    public int ModifyCost(UpgradeBase upgrade, int currentCost)
    {
        //Debug.LogError($"Modify cost for category => {_targetCategory}, in current cost {currentCost}. COst modifier => {_costModifier}");
        UpgradeCategory category = upgrade.Category;

        if(category != _targetCategory)
            return currentCost;

        return Mathf.Max(0,Mathf.FloorToInt(currentCost * _costModifier));
    }

    public IRuntimeModifier Clone()
    {
        return new UpgradeCategoryCostModifier(_targetCategory, _costModifier);
    }

    public void Destroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}