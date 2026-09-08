using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Death Save")]
public class IntermediateDeathSave : HealthWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    public int healAmount = 4;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>");
    }

    public override void Apply()
    {
        DeathSaveManager.Instance.Register(new HealDeathSave(healAmount), this);
    }

}
