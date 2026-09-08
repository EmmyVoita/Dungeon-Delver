using System;
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

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
        .Replace("{COST}", $"<color=#{UIColors.ToHex(UIColors.Red)}>${currencyRequired}</color>");
    }
}
