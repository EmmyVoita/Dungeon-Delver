using System.Collections.Generic;
using UnityEngine;

public class SimpleInventory : InventoryBase
{
    public override void Close()
    {
        
        base.Close();
        InputFocusManager.Release(this);
    }

    protected override void FillInventory()
    {
        Debug.Log("[SIMPLE INVENTORY] FillInventory CALLED");

        Dictionary<UpgradeBase, int> allCards =
            ShopManager.Instance.FilteredPurchasedCards;

        Debug.Log(
            $"[SIMPLE INVENTORY] Found {allCards.Count} cards"
        );

        foreach (var kvp in allCards)
        {
            if (kvp.Key == null)
            {
                Debug.LogError(
                    "[SIMPLE INVENTORY] Null upgrade!"
                );

                continue;
            }

            Debug.Log(
                $"[SIMPLE INVENTORY] Adding " +
                $"{kvp.Key.DisplayName} x{kvp.Value}"
            );

            AddCard(kvp.Key, kvp.Value);
        }
    }

    protected override string GetDescription(UpgradeCardUI cardUI)
    {
        string prefix = $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${cardUI.Base.Cost.ToString("N0")}</color>] ";
        return prefix + cardUI.GetDescription();
    }
}