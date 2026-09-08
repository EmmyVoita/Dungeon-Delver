using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Challenges/Ability Damage Challenge")]
public class AbilityDamageChallengeUpgrade : UpgradeBase, IAbilityDamageModifier, IDamageModifier
{
    [SerializeField] private int additionalHealthDamage = 1;
    [SerializeField] private int additionalAbilityDamage = 1;
    [SerializeField] private int currencyAmount = 5000;

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{ABILITY_DAMAGE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Red)}>" +
            $"{additionalAbilityDamage}</color>";

        replacements["{HEALTH_DAMAGE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Red)}>" +
            $"{additionalHealthDamage}</color>";  

        replacements["{CURRENCY_AMOUNT}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{currencyAmount}</color>";

        base.AddReplacements(replacements);
    }

    public override void Apply()
    {
        StatModifierManager.Instance.AddTemporaryModifier(this);
        CurrencyManager.Instance.AddCurrency(currencyAmount, silent: true);
    }

    public int ModifyAbilityDamageTaken(int baseDamage)
    {
        return baseDamage + additionalAbilityDamage;
    }

    public int ModifyDamageTaken(int baseDamage)
    {
        if(Player.Instance.AbilityCharge < additionalAbilityDamage)
            return baseDamage + additionalHealthDamage;
        else
            return baseDamage;
    }
}
