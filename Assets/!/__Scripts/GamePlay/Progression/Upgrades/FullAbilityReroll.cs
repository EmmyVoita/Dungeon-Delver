using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Ability Reroll")]
public class FullAbilityReroll : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private int rerollAmount = 1;
    [SerializeField] private float abilityCostMult = 1.5f;


    public override string GetFormattedDescription(bool isInventory)
    {
        return descriptionTemplate
            .Replace("{REROLLS}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{rerollAmount}</color>")
            .Replace("{ABILITY_COST_MULT}", $"<color=#{UIColors.ToHex(UIColors.Red)}>{(abilityCostMult-1).ToString("P0")}</color>");
    }

    public override void Apply()
    {
        AbilityChargeManager.Instance.RegisterPermanent(new RerollOnFullAbilityEffect(1.5f, rerollAmount), this);
    }
}
