using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/PassiveAbilityChargeUpgrade")]
public class PassiveAbilityChargeUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
     [SerializeField] private List<float> chargeRatePerStack;
    [SerializeField] private float chargeRate = 1f;

    protected override void AddReplacements(Dictionary<string, string> replacements, bool isInventory)
    {
        string chargeRateText = isInventory ? InventoryText() : $"{GetChargeRateText():0.##}%";
        replacements["{CHARGE_RATE}"] = $"<color=#{UIColors.ToHex(UIColors.Green)}>" + $"{chargeRateText}</color>" ;

        base.AddReplacements(replacements, isInventory);
    }

    private string InventoryText()
    {
        string text = "(";

        for(int i = 0; i < chargeRatePerStack.Count; i++)
        {
            string prefix = i == 0 ? "" : "/";
            text += $"{prefix}{chargeRatePerStack[i] * 100f:0.##}%";
        }
        text += ")";

        return text;
    }

    private float GetChargeRateText()
    {
        
            
        int stackCount = ShopManager.Instance ? ShopManager.Instance.GetOwnedStackCount(this) : 0;

        if (chargeRatePerStack.Count == 0)
            return 0;

        int index = Mathf.Clamp(stackCount, 0, chargeRatePerStack.Count - 1);

        return chargeRatePerStack[index] * 100f;
    }

    private float GetChargeRate()
    {
        int stackCount = ShopManager.Instance ? ShopManager.Instance.GetOwnedStackCount(this) : 0;

        if (chargeRatePerStack.Count == 0)
            return 0;

        int index = Mathf.Clamp(stackCount - 1, 0, chargeRatePerStack.Count - 1);

        return chargeRatePerStack[index];
    }
    
    public override void Apply()
    {
        PassiveEffectsManager.Instance.RegisterRenewing(new PassiveAbilityChargeEffect(GetChargeRate()), this);
    }
}