using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Extend Golden On Crit")]
public class ExtendGoldenOnCritUpgrade : UpgradeBase
{
    public int maxStack = 3;
    public int arrowsPerCrit = 1;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{ARROWS_PER_CRIT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{arrowsPerCrit}</color>";
        replacements["{MAX_STACKS}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{maxStack}</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        GoldenArrowManager.Instance.AddGoldenExtension(this);
    }

}
