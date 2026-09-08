using System.Collections.Generic;
using UnityEngine;

public class YourCardsFilteredInventory : InventoryBase
{
    private ShopPagesController _shopPagesController;
    private ShopPagesController.ShopPage _targetExitShopPage;

    public void SetVisibilityData(ShopPagesController shopPagesController, ShopPagesController.ShopPage targetExitShopPage)
    {
        _shopPagesController = shopPagesController;
        _targetExitShopPage = targetExitShopPage;
    }

    public override bool TryOpen(KeyPromptController keyPromptController, InventoryOpenSettings settings = null)
    {
        if (ShopManager.Instance.FilteredPurchasedCards.Count <= 0)
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative,transform.position);
            return false;
        }

        return base.TryOpen(keyPromptController, settings);
    }

    protected override void Update()
    {
        if(_shopPagesController)
        {
            if(_shopPagesController.IsTransitioning)
                return;
        }

        base.Update();
    }   

    public override void Close()
    {
        if(_shopPagesController)
        {
            _shopPagesController.ShowPage(_targetExitShopPage, () =>
            {
                InputFocusManager.Release(this);
            });
        }

        base.Close();
    }

    protected override string GetDescription(UpgradeCardUI cardUI)
    {
        string prefix = $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${cardUI.Base.Cost.ToString("N0")}</color>] ";

        return prefix + cardUI.GetDescription();
    }
    
    protected override void FillInventory()
    {
        Dictionary<UpgradeBase, int> allCards = ShopManager.Instance.FilteredPurchasedCards;

        if(allCards.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }

        foreach(var kvp in allCards)
        {   
            if(kvp.Key.Duration != UpgradeDuration.None)
                AddCard(kvp.Key, kvp.Value);
        }
    }
}