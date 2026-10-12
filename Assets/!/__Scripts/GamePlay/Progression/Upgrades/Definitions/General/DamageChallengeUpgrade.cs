using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Damage Challenge")]
public class DamageChallengeUpgrade : UpgradeBase, IDamageModifier
{
    [SerializeField] private int additionalDamage = 1;
    [SerializeField] private int currencyAmount = 5000;

     protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{DAMAGE}"] = $"<color=#{UIColors.ToHex(UIColors.Red)}>{additionalDamage}</color>";
        replacements["{CURRENCY_AMOUNT}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>${currencyAmount}</color>";

        base.AddReplacements(replacements, isInventory);
    }

    public override void Apply()
    {
        StatModifierManager.Instance.AddTemporaryModifier(this);
        CurrencyManager.Instance.AddCurrency(currencyAmount, silent: true);
    }

    public int ModifyDamageTaken(int baseDamage)
    {
        return baseDamage + additionalDamage;
    }
}
