using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Refresh Charge")]
public class RefreshChargeUpgrade : RerollWeightedUpgradeBase
{
    public int refreshAmount = 1;


    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{REFRESH_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{refreshAmount}</color>";
        base.AddReplacements(replacements,isInventory);
    }

    public override void Apply()
    {
        RunStateManager.Instance.GrantShopReroll(refreshAmount);
    }
}
