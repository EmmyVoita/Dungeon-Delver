using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    public enum InventoryMode
    {
        Gameplay,
        GameOver
    }

    public static event Action<bool> OnInventoryStateChanged;

    [Header("Core")]
    [SerializeField] private InputActionType openKey = InputActionType.Reroll;
    [SerializeField] private InventoryOpenSettings gameOverInventorySettings;
    [SerializeField] private InventoryOpenSettings gameplayInventorySettings;
    [SerializeField] private RectTransform rect;
   

    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<GameObject> cards = new();
    [SerializeField] private DescriptionPanelController descriptionPanel;
    [SerializeField] protected CardTitleTooltip titleTooltip;
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private RectTransform layoutRoot;
    [SerializeField] private TextMeshProUGUI inventoryTitleText;
    [SerializeField] private UpgradeDatabase upgradeDatabase;
    [SerializeField] private UpgradeCategoryDatabase categoryDatabase;
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private TextTypewriter descriptionTypewriter;
    [SerializeField] private TextTypewriter detailsTypewriter;
    [SerializeField] private CanvasGroup mainCanvasGroup;

    
    [Header("Optional")]
    [SerializeField] private ResultsPageController resultsPageController;
    [SerializeField] private ResultsPageController.ResultsPage targetPage;
  
    
    [Header("Fade Animation")]
    [SerializeField] private bool fadeCanvasGroup = true;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;
    [SerializeField] private bool canOpenAnywhere = true;


    [Header("Card Grid Settings")]
    [SerializeField] private int columns = 4;


    [Header("Scroll")]
    [SerializeField] private float bottomSafePadding = 180f;


    private Tween _scrollTween;
    private InventoryOpenSettings _currentSettings;
    private int _selectedIndex;
    private bool _isOpen = false;
    private Dictionary<UpgradeBase, UpgradeCardUI> _upgradeCards = new();
    private readonly Dictionary<KeyPromptType, GameObject> keyPromptLookup = new();

    public InputActionType OpenInventoryButton => openKey;
    public InventoryOpenSettings GameOverInventorySettings => gameOverInventorySettings;

    /*
    private void Awake()
    {
        //BuildLookups();
        Clear();
        Close();

        if(!fadeCanvasGroup && mainCanvasGroup)
        {
            mainCanvasGroup.alpha = 1f;
        }
    }

    private void OnEnable()
    {
        _selectedIndex = 0;

        //ShopManager.OnCardPurchased += HandleCardPurchased;
        ShopManager.OnCardExpired += HandleCardExpired;
        ShopManager.OnCardStackChanged += HandleCardStackChanged;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        //ShopManager.OnCardPurchased -= HandleCardPurchased;
        ShopManager.OnCardExpired -= HandleCardExpired;
        ShopManager.OnCardStackChanged -= HandleCardStackChanged;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void OnDestroy()
    {
        rect.DOKill();
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.GameOverResults)
        {
            Clear();
            FillAllBoughtCards();
        }
    }

    private void HandleCardStackChanged(UpgradeBase upgrade, int stackCount)
    {
        if (!_upgradeCards.TryGetValue(upgrade, out UpgradeCardUI cardUI))
        {
            return;
        }

        cardUI.SetStackAmount(stackCount);
    }

    private void HandleCardExpired(UpgradeBase upgrade)
    {
        if (!_upgradeCards.TryGetValue(upgrade, out UpgradeCardUI cardUI))
        {
            return;
        }

        GameObject cardObject = cardUI.gameObject;

        _upgradeCards.Remove(upgrade);
        cards.Remove(cardObject);

        Destroy(cardObject);

        if (cards.Count == 0)
        {
            _selectedIndex = 0;

            descriptionPanel.Hide();
            titleTooltip.Hide();
            detailsText.text = "";

            return;
        }

        _selectedIndex = Mathf.Clamp(
            _selectedIndex,
            0,
            cards.Count - 1
        );

        if (!_isOpen)
            return;

        StartCoroutine(RebuildAfterCardRemoval());
    }

    private IEnumerator RebuildAfterCardRemoval()
    {
        yield return null;

        RebuildCanvas();

        if (cards.Count > 0)
            RefreshSelection();
    }

    
    private void HandleCardPurchased(UpgradeBase upgrade)
    {
        if(upgrade.Duration == UpgradeDuration.None)
            return;

        AddCard(upgrade);
    }
    

    public void FillFilteredPurchasedCards()
    {
        Dictionary<UpgradeBase, int> allCards = ShopManager.Instance.AllPurchasedCards;

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

    private void AddCard(UpgradeBase upgradeBase, int count = 1)
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

        newCardUI.Setup(upgradeBase, categoryDatabase);

        if (count > 1)
            newCardUI.AddStack(count - 1);

        cards.Add(card);
        _upgradeCards.Add(upgradeBase, newCardUI);
    }

    private void Update()
    {
        if(resultsPageController)
        {
            if(resultsPageController.IsTransitioning)
                return;
        }

        if(shopPagesController)
        {
            if(shopPagesController.IsTransitioning)
                return;
        }

        if(InputBindingManager.Instance.GetKeyDown(openKey) )
        {
            Debug.LogError($"ON INVENTORYU OPEN KEY. ISOPEN => {_isOpen}");
            if(_isOpen)
            {
                if(_currentSettings.CanCloseWithOpenKey)
                    Close();

                return;
            }

            if(!InputFocusManager.HasFocus(this))
                return;
            if(!canOpenAnywhere)
                return;

            if(GameStateManager.Instance.InGameOverState)
                TryOpen(gameOverInventorySettings);
            else
                TryOpen(gameplayInventorySettings);

            return;
        }

        if(_isOpen &&
        InputBindingManager.Instance.GetKeyDown(InputActionType.Back) &&
        _currentSettings.CanCloseWithBack)
        {
            Debug.LogWarning("INVENTORY BACK PRESSED");
            InputBindingManager.Instance.BlockUntilRelease(InputActionType.Back);
            Close();
            return;
        }

        if(!_isOpen)
            return;

        if(!InputFocusManager.HasFocus(this))
            return;

        if (cards.Count == 0)
            return;
        
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            if(Move(-1) && panelNudge) 
                panelNudge.NudgeLeft();
        }
            

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            if(Move(1) && panelNudge)
                panelNudge.NudgeRight();
        }
            
    
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
            Move(-columns);

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
            Move(columns);

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            OpenCardDetails(true);
        }
    }

    public void TryOpen(InventoryOpenSettings settings)
    {
        if (_isOpen)
            return;

        _currentSettings = settings;
        _isOpen = true;
        _selectedIndex = 0;

        // Claim input focus
        InputFocusManager.Claim(this);

        // set the panel title
        inventoryTitleText.text = _currentSettings.Title;

        // Optionally fade in the screen using a canvas group
        if(fadeCanvasGroup)
        {
            mainCanvasGroup.DOKill();
            mainCanvasGroup.DOFade(1, fadeInDuration).SetUpdate(true);
        }

        // Add the key prompts to the bottom
        keyPromptController.PushPrompts(keyPrompts);

        OnInventoryStateChanged?.Invoke(true);

        if (cards.Count == 0)
            return;

        StartCoroutine(InitializeOpenLayout());
    }

    private void Close()
    {
        if (!_isOpen)
            return;

        _isOpen = false;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);

        if(resultsPageController)
        {
            resultsPageController.ShowPage(targetPage, () =>
            {
                InputFocusManager.Release(this);
            });
        }
        else if(shopPagesController)
        {
            shopPagesController.ShowPage(targetShopPage, () =>
            {
                InputFocusManager.Release(this);
            });
        }
        else 
        {
            InputFocusManager.Release(this);
        }
           
        
        if(fadeCanvasGroup)
        {
            mainCanvasGroup.DOKill();
            mainCanvasGroup.DOFade(0,fadeOutDuration).SetUpdate(true);
        }
           
        // Hide the card title popup
        titleTooltip.Hide();
       
        // Cleanup bottom keyprompts
        keyPromptController.PopPrompts();

        // Cleanup typewriters
        descriptionTypewriter.StopTyping(true);
        detailsTypewriter.StopTyping(true);

        OnInventoryStateChanged?.Invoke(false);
    }

    private IEnumerator InitializeOpenLayout()
    {
        // Let Destroy() finish and let newly-created UI objects enter the layout.
        yield return null;

        RebuildCanvas();

        // Explicitly start at the top of the inventory.
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 1f;

        Canvas.ForceUpdateCanvases();

        RefreshSelection();
    }

    private void RebuildCanvas()
    {
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

        Canvas.ForceUpdateCanvases();
    }


   

    public void Clear()
    {
        foreach (var cardUI in _upgradeCards.Values)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }

        _upgradeCards.Clear();
        cards.Clear();
    }

    public void FillRecord(List<UpgradeRecord> upgradeRecords)
    {
        Clear();

        if(upgradeRecords.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }
            
        foreach(UpgradeRecord record in upgradeRecords)
        {
            UpgradeBase upgradeBase = upgradeDatabase.GetUpgrade(record.upgradeId);

            if(upgradeBase == null)
                return;

            int count = record.count;
            AddCard(upgradeBase, count);
        }
    }

    public void FillAllCardsByCategoryDrawPool(UpgradeCategory category = UpgradeCategory.General)
    {
        List<UpgradeBase> drawPool = ShopManager.Instance.CreateDrawPool(category);

        if(drawPool.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }

        foreach(UpgradeBase card in drawPool)
        {   
            AddCard(card, 1);
        }
    }

    private void FillAllBoughtCards()
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


    private bool Move(int amount)
    {
        int newIndex = Mathf.Clamp(
            _selectedIndex + amount,
            0,
            cards.Count - 1
        );

        if (newIndex == _selectedIndex)
            return false;


        _selectedIndex = newIndex;
        RefreshSelection();
        return true;
    }

    private void RefreshSelection()
    {
        OpenCardDetails(false);
         
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);

        UpgradeCardUI selectedCardUI = null;

        for (int i = 0; i < cards.Count; i++)
        {
            UpgradeCardUI cardUI = cards[i].GetComponent<UpgradeCardUI>();
            cardUI.SetHighlighted(i == _selectedIndex, false,true, false);

            if(i == _selectedIndex)
            {
                selectedCardUI = cardUI;

                descriptionPanel.Show($"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${cardUI.Base.Cost.ToString("N0")}</color>] " +
                                        cardUI.GetDescription());
            }
        }

        titleTooltip.Hide();
        
        ScrollTo(selectedCardUI.RectTransform,
        () =>
        {
            Debug.Log($"Trying to scroll to selectedCardUI => {selectedCardUI}. The display name is => {selectedCardUI.Base.displayName}");
            if(selectedCardUI)
                titleTooltip.ShowForCard(selectedCardUI.RectTransform, selectedCardUI.Base.displayName);
        });
    }


    private void ScrollTo(RectTransform target, System.Action onComplete = null)
    {
        if(target == null)
        {
            onComplete?.Invoke();
            return;
        }

        Canvas.ForceUpdateCanvases();

        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
        {
            onComplete?.Invoke();
            return;
        }

        float viewportCenterY = viewport.rect.center.y;

        Vector2 targetLocal = viewport.InverseTransformPoint(target.position);

        float safeViewportBottom = viewport.rect.yMin + bottomSafePadding;

        

        float deltaY = targetLocal.y - viewportCenterY;
        //float deltaY = targetLocal.y - safeViewportBottom;

        float desiredY = content.anchoredPosition.y - deltaY;

        float maxY = Mathf.Max(0, contentHeight - viewportHeight);

        desiredY = Mathf.Clamp(desiredY, 0, maxY);

        

        _scrollTween?.Kill();

        _scrollTween = content
            .DOAnchorPosY(desiredY, 0.2f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void OpenCardDetails(bool state)
    {
        UpgradeCardUI cardUI = cards[_selectedIndex].GetComponent<UpgradeCardUI>();

        switch(state)
        {
            case true:
                detailsText.text = cardUI.Base.GetDetails();
                break;
            case false:
                detailsText.text = "";
                break;
        }

        detailsText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            detailsText.rectTransform
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            layoutRoot
        );
    }
    */
}