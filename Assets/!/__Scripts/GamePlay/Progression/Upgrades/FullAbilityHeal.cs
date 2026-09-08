using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Ability Heal")]
public class FullAbilityHeal : SynergyWeightedUpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float abilityCostMult = 1.5f;


    public override string GetFormattedDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_VALUE}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>")
            .Replace("{ABILITY_COST_MULT}", $"<color=#{UIColors.ToHex(UIColors.Red)}>{(abilityCostMult-1).ToString("P0")}</color>");
    }

    public override void Apply()
    {
        AbilityChargeManager.Instance.RegisterPermanent(new HealOnFullAbilityUpgradeEffect(1.5f, healAmount), this);
    }
}
