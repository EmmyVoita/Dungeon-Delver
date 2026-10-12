using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Death Save")]
public class IntermediateDeathSave : HealthWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    public int healAmount = 4;


    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{HEAL_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        DeathSaveManager.Instance.Register(new HealDeathSave(healAmount), this);
    }

}
