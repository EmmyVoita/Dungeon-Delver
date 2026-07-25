using UnityEngine;



[CreateAssetMenu(menuName = "Upgrades/Paid Ability Use")]
public class PaidAbilityUseUpgrade : UpgradeBase
{
    [SerializeField] private int usesPerLevel = 1;
    [SerializeField] private int currencyCost = 50;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{USES_PER_LEVEL}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{usesPerLevel:N0}</color>")
            .Replace("{CURRENCY_COST}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>${currencyCost:N0}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{USES_PER_LEVEL}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{usesPerLevel:N0}</color>")
            .Replace("{CURRENCY_COST}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>${currencyCost:N0}</color>");
    }


    public override void Apply()
    {
        PaidAbilityUseManager.Instance.AddPaidUsePerLevel(usesPerLevel,currencyCost);
    }
}