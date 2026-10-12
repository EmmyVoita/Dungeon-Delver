using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Combo Shield Upgrade")]
public class ComboShieldUpgrade : UpgradeBase
{
    [SerializeField] private int hitBlockCharges = 1;
    [SerializeField] private int abilityCharge = 0;
    [SerializeField] private GameObject shieldPrefab;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{ABILITY_CHARGE}"] =
            $"<color=#{UIColors.ToHex(UIColors.Green)}>" +
            $"{abilityCharge}</color>";
        
        base.AddReplacements(replacements, isInventory);
    }
    
    public override void Apply()
    {
        DamageSaveManager.Instance.Register(new ShieldDamageSave(true, abilityChargeAmount:abilityCharge, shieldPrefab:shieldPrefab), this);
        //ComboManager.Instance.PreventNextComboBreak();
    }
}
