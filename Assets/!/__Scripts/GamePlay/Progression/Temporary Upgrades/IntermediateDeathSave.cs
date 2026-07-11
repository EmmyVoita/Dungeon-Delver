using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Death Save")]
public class IntermediateDeathSave : UpgradeBase
{
    public int healAmount = 4;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", healAmount.ToString("N0"));
    }

    public override void Apply()
    {
        DeathSaveManager.Instance.Register(new HealDeathSave(healAmount));
    }

}
