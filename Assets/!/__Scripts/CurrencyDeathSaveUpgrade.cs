using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/CurrencyDeath Save")]
public class CurrencyDeathSaveUpgrade : CurrencyWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    public int healAmount = 1;
    public int cost = 500;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{COST}"] = $"<color=#{UIColors.ToHex(UIColors.Red)}>${cost}</color>";
        replacements["{HEAL_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>";
        replacements["{HEALTH}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>1 Health</color>";

        base.AddReplacements(replacements,isInventory);
    }

    public override void Apply()
    {
        DeathSaveManager.Instance.RegisterRenewing(new CurrencyDeathSave(healAmount,cost, true), this);
    }

}
