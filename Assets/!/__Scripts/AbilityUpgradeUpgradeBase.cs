using System.Collections.Generic;
using UnityEngine;

public class AbilityUpgradeUpgradeBase : UpgradeBase
{
    [SerializeField] private int upgradeTier = 1;

    public override string DisplayName => GetAbilityUpgradeBase() == null ? "Error" : GetAbilityUpgradeBase().displayName;
    public override Sprite CenterIcon => GetAbilityUpgradeBase() == null ? null : GetAbilityUpgradeBase().icon;

    private AbilityUpgradeBase GetAbilityUpgradeBase()
    {
        AbilityBase abilityBase = GetAbilityBase();

        if(abilityBase == null)
            return null;

        List<AbilityUpgradeBase> upgradeData = abilityBase.UpgradeData;

        int tier = Mathf.Max(0, upgradeTier-1);

        if(tier > upgradeData.Count)
        {
            Debug.LogError("UPGRADE TEIR OUTSIDE OF VALID RANGE");
            return null;
        }
        
        return upgradeData[tier];
    }

    private AbilityBase GetAbilityBase()
    {
        if(Player.Instance)
        {
            return Player.Instance.CurrentAbility;
        }
        else if(LeaderBoardMenuNavigator.Instance)
        {   
            AbilityData abilityData = LeaderBoardMenuNavigator.Instance.GetCurrentAbilityData();
            
            if(!abilityData)
                return null;

            return abilityData.abilityPrefab;
        }
        else
        {
            return null;
        }
    }

    public override string GetFormattedDescription(bool isInventory)
    {
        string description = GetAbilityUpgradeBase() == null ? "Error" : GetAbilityUpgradeBase().description;
        return FormatText(description, isInventory);
    }

    public override string GetDetails()
    {
        string details = GetAbilityUpgradeBase() == null ? "Error" : GetAbilityUpgradeBase().details;
        return FormatText(details);
    }
}