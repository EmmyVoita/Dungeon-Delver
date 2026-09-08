using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/PassiveAbilityChargeUpgrade")]
public class PassiveAbilityChargeUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private float chargeRate = 1f;

    protected override void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{CHARGE_RATE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{chargeRate}</color>";

        base.AddReplacements(replacements);
    }
    
    public override void Apply()
    {
        PassiveEffectsManager.Instance.RegisterRenewing(new PassiveAbilityChargeEffect(chargeRate), this);
    }
}