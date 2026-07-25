using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Golden Arrow")]
public class GoldenArrowUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int comboRequired = 25;

    [Header("Reward")]
    [SerializeField] private int goldenArrowsGranted = 1;

  
    public int ComboRequired => comboRequired;
    public int GoldenArrowsGranted => goldenArrowsGranted;
    
    public override void Apply()
    {
        GoldenArrowManager.Instance.AddGoldenArrowUpgrade(this);
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{COMBO_REQUIRED}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{ComboRequired}</color>")
            .Replace("{GOLDEN_ARROWS}", $"<color=#{UIColors.ToHex(UIColors.Green)}>Golden Arrows</color>")
            .Replace("{ARROWS_GRANTED}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{GoldenArrowsGranted}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{COMBO_REQUIRED}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{ComboRequired}</color>")
            .Replace("{GOLDEN_ARROWS}", $"<color=#{UIColors.ToHex(UIColors.Green)}>Golden Arrows</color>")
            .Replace("{ARROWS_GRANTED}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{GoldenArrowsGranted}</color>")
            .Replace("{STACK_COUNT}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{MaxStacks}</color>");
    }
}
