using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/CurrencyDeath Save")]
public class CurrencyDeathSaveUpgrade : UpgradeBase
{
    public int healAmount = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", healAmount.ToString("N0"));
    }

    public override void Apply()
    {
        DeathSaveManager.Instance.RegisterRenewing(new CurrencyDeathSave(healAmount, true));
    }

}
