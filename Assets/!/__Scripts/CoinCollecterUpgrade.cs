using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/CoinCollectorUpgrade")]
public class CoinCollectorUpgrade : UpgradeBase
{
    public int currencyAmount = 25;
    public float appearancePercentage = 0.25f;

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{CURRENCY_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{currencyAmount.ToString("N0")}</color>")
            .Replace("{APPEARANCE_PERCENTAGE}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{appearancePercentage.ToString("P0")}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace("{CURRENCY_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{currencyAmount.ToString("N0")}</color>")
            .Replace("{APPEARANCE_PERCENTAGE}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{appearancePercentage.ToString("P0")}</color>");
    }

    public override void Apply()
    {
        ChallengeRewardManager.Instance.RegisterOrStackCurrencyReward(currencyAmount,  this, 999, appearancePercentage: appearancePercentage);
        //ChallengeRewardManager.Instance.RegisterRenewing(new CurrencyChallengeReward(currencyAmount,appearancePercentage:appearancePercentage));
    }
}
