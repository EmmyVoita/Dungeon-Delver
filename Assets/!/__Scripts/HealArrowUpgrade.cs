using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/HealArrow",fileName = "HealArrowUpgrade")]
public class HealArrowUpgrade : UpgradeBase
{

    [Header("Trigger")]
    [SerializeField] private int chargeRequired = 3;

    [SerializeField] private int currencyCost = 10;

    [Header("Feedback")]
    [SerializeField] private SoundEffect fullSound;
    [SerializeField] private SoundEffect chargeSound;

    [Header("UI Meter")]
    [SerializeField] private Sprite meterIcon;


    public override void Apply()
    {
        TimwArrowReward reward = new TimwArrowReward(
            chargeRequired,
            currencyCost,
            chargeSound,
            fullSound,
            meterIcon
        );

        CoinCollectEffectsManager.Instance.RegisterRenewing(reward);
    }

     public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CHARGE_REQUIRED}", $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{chargeRequired}</color>")
            .Replace("{COST}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{currencyCost}</color>")
            .Replace("{TIME_ARROW}", $"<color=#{UIColors.ToHex(UIColors.Purple)}>Time Arrow</color>");
    }
}