using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/CurrencyDamageSaveUpgrade")]
public class CurrencyDamageSaveUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int currencyRequired = 100;
    
    public override void Apply()
    {
        DamageSaveManager.Instance.RegisterRenewing(new CurrencyDamageSave(currencyRequired, true));
    }

    public override string GetDescription()
    {
        return descriptionTemplate;
    }
}
