using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/CurrencyZapUpgrade")]
public class CurrencyZapUpgrade : UpgradeBase
{
    [Header("Trigger Rules")]
    [SerializeField] private int currencyRequired = 100;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float zapChance = 0.5f;
    
    public override void Apply()
    {
        CoinCollectEffectsManager.Instance.Register(new CoinCollectZap(currencyRequired, true, radius,zapChance));
    }

    public override string GetDescription()
    {
        return descriptionTemplate;
    }
}
