using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Speed Challenge")]
public class SpeedChallengeUpgrade : UpgradeBase
{
    [SerializeField] private int currencyAmount = 3000;
    [SerializeField] private float modifier;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{SCORE_AMOUNT}", currencyAmount.ToString("N0"))
            .Replace("{TIME_SCALE}", (-(1-modifier)).ToString("P0"));
    }

    private void OnDisable()
    {
    }

    public override void Apply()
    {
        CurrencyManager.Instance.AddCurrency(currencyAmount);


        var timeModifier = new TimeScaleModifier(
            $"speedChallenge_{Guid.NewGuid()}",
            modifier
        );

        TimeManager.Instance.AddLevelModifier(timeModifier);
    }
}
