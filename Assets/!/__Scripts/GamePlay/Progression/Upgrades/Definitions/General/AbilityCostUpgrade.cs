using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability Cost Modifier")]
public class AbilityCostUpgrade : UpgradeBase
{
    public int rerollAmount = 1;
    public float tempAbilityCostMult = 0.95f;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{ABILITY_COST}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{(1-tempAbilityCostMult).ToString("P0")}</color>";
        replacements["{REROLLS}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{rerollAmount}</color>";

        base.AddReplacements(replacements,isInventory);
    }


    public override void Apply()
    {
        BasicAbilityCostUpgrade upgrade = new BasicAbilityCostUpgrade(tempAbilityCostMult);
        AbilityChargeManager.Instance.Register(upgrade, this);
        RunStateManager.Instance.GrantShopReroll(rerollAmount);
    }
}
