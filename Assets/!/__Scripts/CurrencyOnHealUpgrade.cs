using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Currency on Heal")]
public class CurrencyOnHealUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
     [SerializeField] private List<int> currencyPerStack;
    [SerializeField] private int healAmountRequired;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        string currencyText = isInventory ? InventoryText() : $"${GetCurrencyText()}";
        replacements["{CURRENCY}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>{currencyText}</color>";
        replacements["{HEAL_AMOUNT}"] =  $"<color=#{UIColors.ToHex(UIColors.Yellow)}>{healAmountRequired}</color>";

        base.AddReplacements(replacements, isInventory);
    }

    private string InventoryText()
    {
        string text = "(";

        for(int i = 0; i < currencyPerStack.Count; i++)
        {
            string prefix = i == 0 ? "" : "/";
            text += $"{prefix}${currencyPerStack[i]}";
        }
        text += ")";

        return text;
    }

     private int GetCurrencyText()
    {
        int stackCount = ShopManager.Instance ? ShopManager.Instance.GetOwnedStackCount(this) : 0;

        if (currencyPerStack.Count == 0)
            return 0;

        int index = Mathf.Clamp(stackCount, 0, currencyPerStack.Count - 1);

        return currencyPerStack[index];
    }

    private int GetCurrencyAmount()
    {
        int stackCount = ShopManager.Instance ? ShopManager.Instance.GetOwnedStackCount(this) : 0;

        if (currencyPerStack.Count == 0)
            return 0;

        int index = Mathf.Clamp(stackCount - 1, 0, currencyPerStack.Count - 1);

        return currencyPerStack[index];
    }

    public override void Apply()
    {
        HealthChangeEffectsManager.Instance.RegisterPermanent(new CurrencyOnHeal(GetCurrencyAmount(),healAmountRequired), this);
    }
}
