using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/CurrencyDeath Save")]
public class CurrencyDeathSaveUpgrade : CurrencyWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    public int healAmount = 1;
    public int cost = 500;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{COST}", $"<color=#{UIColors.ToHex(UIColors.Red)}>${cost}</color>")
            .Replace("{HEALTH}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>1 Health</color>")
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>");
    }

    public override string GetDetails()
    {
         return detailsTemplate
            .Replace("{COST}", $"<color=#{UIColors.ToHex(UIColors.Red)}>${cost}</color>")
            .Replace("{HEAL_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>");
    }

    public override void Apply()
    {
        DeathSaveManager.Instance.RegisterRenewing(new CurrencyDeathSave(healAmount,cost, true), this);
    }

}
