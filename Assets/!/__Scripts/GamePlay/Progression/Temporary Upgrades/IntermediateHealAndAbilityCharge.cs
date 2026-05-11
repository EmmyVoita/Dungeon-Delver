using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/HealAndAbilityCharge")]
public class IntermediateHealAndAbilityChargeSO : UpgradeBase
{
    public int immediateHealAmount = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", immediateHealAmount.ToString("N0"));
    }

    public override void Apply()
    {
        Player.Instance.HealPlayer(immediateHealAmount);
        Player.Instance.AbilityCharge = Player.Instance.MaxAbilityCharge;
    }
}
