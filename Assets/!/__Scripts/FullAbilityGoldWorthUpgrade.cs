using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Currency Build/FullAbilityGoldWorthUpgrade")]
public class FullAbilityGoldWorthUpgrade : UpgradeBase
{
    [SerializeField] private float increase = 2;
    [SerializeField] private bool addValue = false;

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{INCREASE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"${increase:N0}</color>";

     
        base.AddReplacements(replacements);
    }

    public override void Apply()
    {
        GoldenArrowManager.Instance.AddWorthModifier(new FullAbilityGoldenWorthMod(increase, addValue));
    }
}
