using UnityEngine;

[CreateAssetMenu(
    menuName = "Intermediate Effects/Ability Duration Upgrade")]
public class CurrencyAbilityDurationUpgrade : UpgradeBase
{
    [SerializeField, Min(1)]
    private int currencyStep = 200;

    [SerializeField, Min(.1f)]
    private float abilityDurationMod = 1.1f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace(
                "{ABILITY_DURATION_MOD}",
                $"<color=#{UIColors.ToHex(UIColors.Green)}>{(abilityDurationMod- 1):P0}</color>")
            .Replace(
                "{CURRENCY_STEP}",
                $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyStep:N0}</color>");
    }

    public override string GetDetails()
    {
        return detailsTemplate
            .Replace(
                "{ABILITY_DURATION_MOD}",
                $"<color=#{UIColors.ToHex(UIColors.Green)}>{(1.0f-abilityDurationMod):P0}</color>")
            .Replace(
                "{CURRENCY_STEP}",
                $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{currencyStep:N0}</color>");
    }

    public override void Apply()
    {
        AbilityDurationManager.Instance.RegisterPermanent(
            new CurrencyAbilityDurationModifier(
                currencyStep,
                abilityDurationMod));
        
    }
}