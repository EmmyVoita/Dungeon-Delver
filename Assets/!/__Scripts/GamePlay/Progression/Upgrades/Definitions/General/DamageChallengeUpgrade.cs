using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Damage Challenge")]
public class DamageChallengeUpgrade : UpgradeBase, IDamageModifier
{
    [SerializeField] private int additionalDamage = 1;
    [SerializeField] private int currencyAmount = 5000;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{ADDITIONAL_DAMAGE}", $"<color=#{UIColors.ToHex(UIColors.Red)}>{additionalDamage}</color>")
            .Replace("{CURRENCY_AMOUNT}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{currencyAmount}</color>");
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
