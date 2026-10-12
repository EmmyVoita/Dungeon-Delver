using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/AbilityUpgrade1")]
public class AbilityUpgrade1 : AbilityUpgradeUpgradeBase
{

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        base.AddReplacements(replacements, isInventory);
    }
    
    public override void Apply()
    {
        Player.Instance.CurrentAbility.AddUpgrade();
    }
}
