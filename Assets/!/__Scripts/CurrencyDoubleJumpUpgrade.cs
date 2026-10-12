using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Currency Double Jump Upgrade")]
public class CurrencyDoubleJumpUpgrade : CurrencyWeightedUpgradeBase
{
    public int currencyAmount = 25;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{DJ}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>Double Jump</color>";
        replacements["{CURRENCY_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Red)}>${currencyAmount.ToString("N0")}</color>";

        base.AddReplacements(replacements,isInventory);
    }

    public override void Apply()
    {
        JumpEffectsManager.Instance.RegisterPermanent(new CurrencyDoubleJump(currencyAmount), this);
    }
}
