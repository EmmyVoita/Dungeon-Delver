using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Speed Challenge")]
public class SpeedChallengeUpgrade : UpgradeBase
{
    [SerializeField] private int currencyAmount = 3000;
    [SerializeField] private float modifier;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{CURRENCY}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>${currencyAmount}</color>";
        replacements["{TIME_SCALE}"] = $"<color=#{UIColors.ToHex(UIColors.Red)}>{(-(1-modifier)).ToString("P0")}</color>";

        base.AddReplacements(replacements, isInventory);
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
