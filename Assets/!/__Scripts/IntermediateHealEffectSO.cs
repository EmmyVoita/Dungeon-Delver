using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Heal")]
public class IntermediateHealEffectSO : IntermediateEffectSO
{
    public int healAmount = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", healAmount.ToString("N0"));
    }

    public override void Apply()
    {
        Player.Instance.HealPlayer(healAmount);
    }
}
