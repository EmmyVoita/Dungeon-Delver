using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/TimeArrow",fileName = "TimeArrowUpgrade")]
public class TimeArrowUpgrade : CurrencyWeightedUpgradeBase
{

    [Header("Trigger")]
    [SerializeField] private int chargeRequired = 3;

    [SerializeField] private int currencyCost = 10;
    [SerializeField] private int arrowAmount = 1;

    [Header("Feedback")]
    [SerializeField] private SoundEffect fullSound;
    [SerializeField] private SoundEffect chargeSound;

    [Header("UI Meter")]
    [SerializeField] private Sprite meterIcon;


    public override void Apply()
    {
        TimeArrowReward reward = new TimeArrowReward(
            chargeRequired,
            currencyCost,
            chargeSound,
            fullSound,
            meterIcon,
            arrowAmount
        );

        CoinCollectEffectsManager.Instance.RegisterRenewing(reward, this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{CHARGE_REQUIRED}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{chargeRequired}</color>";
        replacements["{COST}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{currencyCost}</color>";
        replacements["{TIME_ARROW}"] = $"<color=#{UIColors.ToHex(UIColors.Purple)}>Time Arrow</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override string GetDetails()
    {
        return detailsTemplate;
    }
        
}