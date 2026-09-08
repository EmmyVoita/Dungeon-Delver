using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/AbilityUpgrade1")]
public class AbilityUpgrade1 : AbilityUpgradeUpgradeBase
{

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        base.AddReplacements(replacements);
    }
    
    public override void Apply()
    {
        Player.Instance.CurrentAbility.AddUpgrade();
    }
}
