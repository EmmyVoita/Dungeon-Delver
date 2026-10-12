using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/RerollOnComboUpgrade")]
public class RerollOnComboUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int comboRequired = 25;

    [Header("Reward")]
    [SerializeField] private int rerollsGranted = 1;
    [SerializeField] private float rerollChance = 0.5f;
    [SerializeField] private float zapChance = 0.25f;

    [Header("Visuals")]
    [SerializeField] private Gradient lightningGradient;
  
    public override void Apply()
    {
        PassiveEffectsManager.Instance.RegisterPermanent(new RerollOnComboEffect(comboRequired,rerollsGranted,rerollChance,zapChance, lightningGradient), this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{COMBO}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{comboRequired}</color>";

        replacements["{REROLLS}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{rerollsGranted}</color>";

        replacements["{REROLL_CHANCE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{rerollChance.ToString("P0")}</color>";

        replacements["{ZAP_CHANCE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{zapChance.ToString("P0")}</color>";

         base.AddReplacements(replacements, isInventory);
    }
}
