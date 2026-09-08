using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Refresh Charge")]
public class RefreshChargeUpgrade : RerollWeightedUpgradeBase
{
    public int refreshAmount = 1;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{REFRESH_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{refreshAmount}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{REFRESH_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{refreshAmount}</color>");
    }

    public override void Apply()
    {
        RunStateManager.Instance.GrantShopReroll(refreshAmount);
    }
}
