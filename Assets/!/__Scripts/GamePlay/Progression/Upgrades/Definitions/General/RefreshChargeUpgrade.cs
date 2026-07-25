using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Refresh Charge")]
public class RefreshChargeUpgrade : UpgradeBase
{
    public int refreshAmount = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{REFRESH_AMOUNT}", refreshAmount.ToString("N0"));
    }

    public override void Apply()
    {
        RunStateManager.Instance.GrantShopReroll(refreshAmount);
    }
}
