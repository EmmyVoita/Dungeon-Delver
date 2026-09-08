using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Extend Golden On Crit")]
public class ExtendGoldenOnCritUpgrade : UpgradeBase
{
    public int maxStack = 3;
    public int arrowsPerCrit = 1;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{GOLDEN_ARROW}", $"<color=#{UIColors.ToHex(UIColors.Purple)}>Golden Arrow</color>")
            .Replace("{MAX_STACK}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{maxStack}</color>")
            .Replace("{ARROWS_PER_CRIT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{arrowsPerCrit}</color>");
    }

    public override void Apply()
    {
        GoldenArrowManager.Instance.AddGoldenExtension(this);
    }

}
