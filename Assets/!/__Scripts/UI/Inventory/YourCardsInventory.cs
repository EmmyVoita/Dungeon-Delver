using System.Collections.Generic;
using UnityEngine;

public class YourCardsInventory : InventoryBase
{
    private GameOverPagesController _pagesController;
    private GameOverPagesController.GameOverPage _targetExitPage;

    public void SetVisibilityData(GameOverPagesController pagesController, GameOverPagesController.GameOverPage targetExitPage)
    {
        _pagesController = pagesController;
        _targetExitPage = targetExitPage;
    }

    protected override void Update()
    {
        if(_pagesController)
        {
            if(_pagesController.IsTransitioning)
                return;
        }

        base.Update();
    }   

    public override void Close()
    {
        if(_pagesController)
        {
            _pagesController.ShowPage(_targetExitPage, () =>
            {
                InputFocusManager.Release(this);
            });
        }

        base.Close();
    }
    
    protected override void FillInventory()
    {
        Dictionary<UpgradeBase, int> allCards = ShopManager.Instance.AllPurchasedCards;

        if(allCards.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }

        foreach(var kvp in allCards)
        {   
            AddCard(kvp.Key, kvp.Value);
        }
    }
}