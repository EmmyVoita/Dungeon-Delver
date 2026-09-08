using System.Collections.Generic;
using UnityEngine;

public class AbilityUpgradeUpgradeBase : UpgradeBase
{
    [SerializeField] private int upgradeTier = 1;

    public override string DisplayName => GetAbilityUpgradeBase().displayName;

    private AbilityUpgradeBase GetAbilityUpgradeBase()
    {
        if(!Player.Instance)
        {
            Debug.LogError("PLAYER.INSTANCE IS NULL");
            return null;
        }
            

        AbilityBase abilityBase = Player.Instance.CurrentAbility;

        if(!abilityBase)
        {
            Debug.LogError("PLAYER.INSTANCE.CURRENT ABILITY IS NULL");
            return null;
        }
            
        
        List<AbilityUpgradeBase> upgradeData = abilityBase.UpgradeData;

        int tier = Mathf.Max(0, upgradeTier-1);

        if(tier > upgradeData.Count)
        {
            Debug.LogError("UPGRADE TEIR OUTSIDE OF VALID RANGE");
            return null;
        }
        
        return upgradeData[tier];
    }

    public override string GetFormattedDescription()
    {
        return FormatText(GetAbilityUpgradeBase().description);
    }

    public override string GetDetails()
    {
        return FormatText(GetAbilityUpgradeBase().details);
    }
}