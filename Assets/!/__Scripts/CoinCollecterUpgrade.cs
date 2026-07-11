using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/CoinCollectorUpgrade")]
public class CoinCollectorUpgrade : UpgradeBase
{
    public int currencyAmount = 25;
    public float appearancePercentage = 0.25f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CURRENCY_AMOUNT}", currencyAmount.ToString("N0"));
    }

    public override void Apply()
    {
        ChallengeRewardManager.Instance.RegisterRenewing(new CurrencyChallengeReward(currencyAmount,appearancePercentage:appearancePercentage));
    }
}
