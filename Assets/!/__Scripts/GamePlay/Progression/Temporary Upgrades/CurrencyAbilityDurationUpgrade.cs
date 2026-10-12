using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Ability Duration Upgrade")]
public class CurrencyAbilityDurationUpgrade : CurrencyWeightedUpgradeBase
{
    [SerializeField, Min(.1f)]
    private float abilityDurationMod = 1.1f;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{ABILITY_DURATION_MOD}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{(1.0f-abilityDurationMod):P0}</color>";
        replacements["{CURRENCY_STEP}"] = $"<color=#{UIColors.ToHex(UIColors.Yellow)}>${currencyStep:N0}</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        AbilityDurationManager.Instance.RegisterPermanent(
            new CurrencyAbilityDurationModifier(
                currencyStep,
                abilityDurationMod), this);
        
    }
}