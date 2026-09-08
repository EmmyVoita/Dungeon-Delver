using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Speed Challenge")]
public class SpeedChallengeUpgrade : UpgradeBase
{
    [SerializeField] private int currencyAmount = 3000;
    [SerializeField] private float modifier;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{CURRENCY}", $"<color=#{UIColors.ToHex(UIColors.Green)}>${currencyAmount}</color>")
            .Replace("{TIME_SCALE}", $"<color=#{UIColors.ToHex(UIColors.Red)}>{(-(1-modifier)).ToString("P0")}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{CURRENCY}", $"<color=#{UIColors.ToHex(UIColors.Green)}>${currencyAmount}</color>")
            .Replace("{TIME_SCALE}", $"<color=#{UIColors.ToHex(UIColors.Red)}>{(-(1-modifier)).ToString("P0")}</color>");
    }

    public override void Apply()
    {
        CurrencyManager.Instance.AddCurrency(currencyAmount);


        var timeModifier = new TimeScaleModifier(
            $"speedChallenge_{Guid.NewGuid()}",
            modifier
        );

        TimeManager.Instance.AddLevelModifier(timeModifier,this);
    }
}
