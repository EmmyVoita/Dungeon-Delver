using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/CurrencyDamageSaveUpgrade")]
public class CurrencyDamageSaveUpgrade : CurrencyWeightedUpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int currencyRequired = 100;
    [SerializeField] private GameObject shieldPrefab;
    
    public override void Apply()
    {
        DamageSaveManager.Instance.RegisterRenewing(new CurrencyDamageSave(currencyRequired, true, shieldPrefab), this);
    }

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        replacements["{REQUIRED_COST}"] = $"<color=#{UIColors.ToHex(UIColors.Red)}>${currencyRequired}</color>";

        base.AddReplacements(replacements, isInventory);
    }
}
