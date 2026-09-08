using System;
using System.Collections.Generic;
using UnityEngine;

public class DuplicateCardFilteredInventory : InventoryBase
{
    public static event Action OnDuplicateSelectionComplete;

    [Header("Input")]
    [SerializeField] private InputActionType selectKey = InputActionType.UseAbility;

    
    //[Header("Visibility")]
    //[SerializeField] private CanvasGroup rootCanvasGroup;

    protected override void Awake()
    {
        //rootCanvasGroup.alpha = 0;
        base.Awake();
    }


    protected override void Update()
    {
        base.Update();

        if(_isOpen && InputBindingManager.Instance.GetKeyDown(selectKey))
        {
            SelectCard();
        } 
    }   

    public override void Close()
    {
        base.Close();

        InputFocusManager.Release(this);
    }
    
    protected override void FillInventory()
    {
        Dictionary<UpgradeBase, int> allCards = ShopManager.Instance.FilteredPurchasedCards;

        Dictionary<UpgradeBase, int> filtredCards = new();

        foreach(var kvp in allCards)
        {
            if(kvp.Value < kvp.Key.MaxStacks)
            {
                filtredCards[kvp.Key] = 1;
            }
        }

        if(allCards.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }

        foreach(var kvp in filtredCards)
        {   
            if(kvp.Key.Duration != UpgradeDuration.None)
                AddCard(kvp.Key, kvp.Value);
        }
    }

    private void SelectCard()
    {
        UpgradeCardUI cardUI = _cards[_selectedIndex].GetComponent<UpgradeCardUI>();
        ShopManager.Instance.DuplicateCard(cardUI.Base);
        Close();
        OnDuplicateSelectionComplete?.Invoke();
    }
}