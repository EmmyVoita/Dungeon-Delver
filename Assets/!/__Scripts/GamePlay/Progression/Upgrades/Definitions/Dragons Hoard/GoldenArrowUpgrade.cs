using System;
using System.Collections.Generic;
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

    protected override void AddReplacements(Dictionary<string, string> replacements,  bool isInventory)
    {
        replacements["{COMBO_REQUIRED}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{ComboRequired}</color>";
        replacements["{ARROWS_GRANTED}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{GoldenArrowsGranted}</color>";

        base.AddReplacements(replacements, isInventory);
    }
}
