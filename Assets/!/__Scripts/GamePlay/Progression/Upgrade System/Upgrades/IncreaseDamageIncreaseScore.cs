using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Increase Damage Increase Score")]
public class IncreaseDamageIncreaseScore : UpgradeBase, IDamageModifier
{
    [SerializeField] private int additionalDamage = 1;
    [SerializeField] private int currencyAmount = 5000;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{COLOR_LAVENDER}", $"<color=#{UIColors.ToHex(UIColors.Lavender)}>")
            .Replace("{ADDITIONAL_DAMAGE}", additionalDamage.ToString())
            .Replace("{CURRENCY_AMOUNT}", currencyAmount.ToString("N0"));
    }

    public override void Apply()
    {
        UpgradeManager.Instance.AddTemporaryModifier(this);
        CurrencyManager.Instance.AddCurrency(currencyAmount, silent: true);
    }

    public int ModifyDamageTaken(int baseDamage)
    {
        return baseDamage + additionalDamage;
    }

}
