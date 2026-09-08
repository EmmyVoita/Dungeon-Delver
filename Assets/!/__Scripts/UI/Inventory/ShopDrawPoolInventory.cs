using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopDrawPoolInventory : InventoryBase
{
    private ShopPagesController _shopPagesController;
    private ShopPagesController.ShopPage _targetExitShopPage;

    public void SetVisibilityData(ShopPagesController shopPagesController, ShopPagesController.ShopPage targetExitShopPage)
    {
        _shopPagesController = shopPagesController;
        _targetExitShopPage = targetExitShopPage;
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

    protected override void AddCard(UpgradeBase upgradeBase, int count = 1)
    {
        if (_upgradeCards.ContainsKey(upgradeBase))
        {
            _upgradeCards.TryGetValue(upgradeBase, out UpgradeCardUI cardUI);

            if (cardUI != null)
                cardUI.AddStack(count);

            return;
        }

        GameObject card = Instantiate(cardPrefab, content);
        UpgradeCardUI newCardUI = card.GetComponent<UpgradeCardUI>();

        if (!newCardUI)
        {
            Destroy(card);
            return;
        }

        newCardUI.Setup(upgradeBase, categoryDatabase, true);

        if (count > 1)
            newCardUI.AddStack(count - 1);

        _cards.Add(newCardUI);
        _upgradeCards.Add(upgradeBase, newCardUI);
    }
    
    protected override void FillInventory()
    {
        UpgradeCategory category = ShopManager.Instance.CurrentUpgradeCategory;
        List<UpgradeBase> drawPool = ShopManager.Instance.CreateDrawPool(category, hideCantAppear: false);

        drawPool = drawPool.OrderByDescending(t => t.CanAppear()).ToList();

        if(drawPool.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }

        foreach(UpgradeBase card in drawPool)
        {   
            AddCard(card, 1);
        }

        foreach(var kvp in _upgradeCards)
        {
            kvp.Value.ShowDrawPercentage();
        }
    }
}