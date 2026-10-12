using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ShopDrawPoolInventory : InventoryBase
{
    private PagesController _pagesController;
    private int _targetExitShopPage;
    private UpgradeCategory _upgradeCategory;

    [Header("Percentage Visibility")]
    [SerializeField] private bool showDrawPoolPercentages = false;

    public void SetVisibilityData(PagesController pagesController, int targetExitPage)
    {
        _pagesController = pagesController;
        _targetExitShopPage = targetExitPage;
    }

    public void SetTargetCategory(UpgradeCategory upgradeCategory)
    {
        _upgradeCategory = upgradeCategory;
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
            _pagesController.ShowPage(_targetExitShopPage, () =>
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
        List<UpgradeBase> drawPool = ShopManager.Instance.CreateDrawPool(_upgradeCategory, 
                                                                        hideCantAppear: false, 
                                                                        exclusions: ShopManager.Instance.CurrentExclusions);

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
        
        if(showDrawPoolPercentages)
        {
            foreach(var kvp in _upgradeCards)
            {
                kvp.Value.ShowDrawPercentage(_upgradeCategory);
            }
        }
       
    }
}