using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;


public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public static event Action UpgradeSelectionComplete;
    public static event Action<UpgradeBase> OnCardPurchased;
    public static event Action<UpgradeBase> OnCardExpired;
    public static event Action<UpgradeBase, int> OnCardStackChanged;
    public static event Action<ShopType> OnShopOpened;

    [SerializeField] private float cardWidth = 350f;
    [SerializeField] private float cardFlipDelay = 0.5f;
    [SerializeField] private float cardFlipDuration = 0.3f;
    [SerializeField] private SoundEffect flipSound;

    [Header("On Purchase")]
    [SerializeField] private ScreenShakeRequest ssRequest;


    [Header("References")]
    [SerializeField] private RectTransform upgradeIconFooter;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private GameObject upgradeIconPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private List<GameObject> dependentObjects;
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private TextTypewriter typewriter;

    [SerializeField] private List<RectTransform> cardContainers;
    [SerializeField] private RectTransform shopRootRect;
    [SerializeField] private UpgradeCategoryDatabase categoryDatabase;
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private PagesController pagesController;
    [SerializeField] private CanvasGroup shopHideCanvasGroup;
    
    [Header("Shop Mode UI")]
    [SerializeField] private GameObject currencyDisplay;
    [SerializeField] private TextMeshProUGUI shopTitle;


    [Header("Inventory")]
    [SerializeField] private int shopPageIndex = 0;
    [SerializeField] private int inventoryPageIndex = 0;
    [SerializeField] private int drawPoolPageIndex = 0;

    [SerializeField] private YourCardsFilteredInventory inventory;
    [SerializeField] private ShopDrawPoolInventory drawPoolInventory;
    [SerializeField] private InventoryOpenSettings inventoryOpenSettings;
    [SerializeField] private InventoryOpenSettings drawPoolOpenSettings;

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;


    [Header("Available Upgrades")]
    [SerializeField] private UpgradeDatabase upgradeDatabase;
    [SerializeField] private float scaleSubPerExtraCard = 0.1f;
    [SerializeField] private int baseCardsToDisplay = 3;

    [Header("Input")]
    [SerializeField] private InputActionType continueKey = InputActionType.Jump;
    [SerializeField] private InputActionType purchaseKey = InputActionType.Confirm;
    [SerializeField] private InputActionType rerollKey = InputActionType.Reroll;
    [SerializeField] private InputActionType viewDrawPoolKey = InputActionType.UseAbility;
    [SerializeField] private InputActionType inventoryKey = InputActionType.ViewUpgrades;
    [SerializeField] private float continueInputDelay = 0.5f;

    [Header("SetCardsPool")]
    [SerializeField] private List<UpgradeBase> setCardPool;

    private List<UpgradeBase> selectedCards = new ();
    private List<UpgradeCardUI> currentCards = new();
    private List<UpgradeCardUI> nextCards = new();
    //public Dictionary<UpgradeBase, int> AllChosenCards => _purchasedCards;
    private int selectedIndex = 0;
    private bool _rerollLock;
    private int _numPurchasedCards;
    private bool _canContinue;
    private UpgradeCategory _currentShopCategory;

    //Pick Free shop mode
    private ShopMode _shopMode = ShopMode.Normal;
    private int _maxPurchases = int.MaxValue;
    private bool _drawWithCardWeights;
    private int _numCardsToDisplay;
    private Vector3 _rootBaseScale;
    private List<UpgradeCategory> _currentExclusions;


    private HashSet<int> _purchasedIndices = new();
    private HashSet<UpgradeBase> _purchasedCardsRound = new();
    private Dictionary<UpgradeBase, int> _purchasedCards = new();
    private Dictionary<UpgradeBase, int> _filteredPurchasedCards = new();
    private Dictionary<UpgradeBase, UpgradeIconUI> activeUpgradeIcons = new();
    private Dictionary<UpgradeCardUI, RectTransform> _cardToContainer = new();

    public int PurchasedCardsCount => _numPurchasedCards;
    public UpgradeCategoryDatabase CategoryDatabase => categoryDatabase;
    public Dictionary<UpgradeBase, int> AllPurchasedCards => _purchasedCards;
    public Dictionary<UpgradeBase, int> FilteredPurchasedCards => _filteredPurchasedCards;
    public UpgradeCategory CurrentUpgradeCategory => _currentShopCategory;
    public List<UpgradeCategory> CurrentExclusions => _currentExclusions;

    private List<UpgradeBase> Cards => upgradeDatabase.Upgrades;

    public void HideUpgradeUI()
    {
        shopHideCanvasGroup.DOFade(0,0.2f);
        keyPromptController.HideUI();
    }

    public void ShowUpgradeUI()
    {
        shopHideCanvasGroup.DOFade(1,0.2f);
        keyPromptController.ShowUI();
    }

    public float GetTotalDrawPoolWeight(UpgradeCategory category)
    {
        List<UpgradeBase> drawPool = CreateDrawPool(category);

        float totalWeight = 0;

        foreach(UpgradeBase upgradeBase in drawPool)
        {
            totalWeight += upgradeBase.GetOfferWeight();
        }

        return totalWeight;
    }

    public bool HasAnyDuplicatableCard()
    {
        foreach (var pair in _filteredPurchasedCards)
        {
            UpgradeBase upgrade = pair.Key;

            if (upgrade == null)
                continue;

            if (CanDuplicate(upgrade))
                return true;
        }

        return false;
    }

    public bool CanDuplicate(UpgradeBase upgrade)
    {
        if (upgrade == null)
            return false;

        int ownedStacks = GetPurchasedCardCount(upgrade);

        return ownedStacks < upgrade.MaxStacks;
    }

    public bool HasCard(UpgradeOption cardOption)
    {
        return _purchasedCards.ContainsKey(cardOption.Base);
    }

    public int GetOwnedStackCount(UpgradeBase cardOption)
    {
        bool containsKey = _purchasedCards.ContainsKey(cardOption);
        if(!containsKey)
            return 0;

        return _purchasedCards[cardOption];
    }

    public bool HasCards(List<UpgradeRequirement> requiredCards)
    {
        if (requiredCards == null || requiredCards.Count == 0)
            return true;

        foreach (UpgradeRequirement requirement in requiredCards)
        {
            if (requirement.upgrade == null)
                continue;

            int ownedStacks = GetPurchasedCardCount(requirement.upgrade);

            if (ownedStacks < requirement.minimumStacks)
                return false;
        }

        return true;
    }


    private bool IsSelectable(int index)
    {
        return !_purchasedIndices.Contains(index);
    }

    public void RegisterPurchase(UpgradeBase card)
    {
        if (_purchasedCards.ContainsKey(card))
            _purchasedCards[card]++;
        else
            _purchasedCards[card] = 1;

        if(_filteredPurchasedCards.ContainsKey(card))
            _filteredPurchasedCards[card]++;
        else
            _filteredPurchasedCards[card] = 1;
    }

    public int GetPurchasedCardCount(UpgradeBase card)
    {
        return _purchasedCards.GetValueOrDefault(card, 0);
    }

    public bool CanAppear(UpgradeBase card)
    {
        int count = _purchasedCards.GetValueOrDefault(card, 0);

        return count < card.MaxStacks;
    }


    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(previousState == GameState.Paused || newState == GameState.Paused) 
            return;
    }

    /*
    void HandleShopOpenSequence(UpgradeCategory category = UpgradeCategory.General)
    {
        _currentShopCategory = category;
        shopUIgroup.DOFade(1,0);
        keyPromptController.PushPrompts(keyPrompts);
        StartCoroutine(InputDelay());
        OpenShop(category);
    }
    */

    private IEnumerator InputDelay()
    {
        _canContinue = false;

        yield return new WaitForSeconds(continueInputDelay);

        _canContinue = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        #if UNITY_EDITOR
            UnityEngine.Assertions.Assert.IsNotNull(
                upgradeIconPrefab,
                $"{nameof(ShopManager)}: upgradeIconPrefab is not assigned"
            );

        #endif

        shopRootRect.gameObject.SetActive(false);

        _numPurchasedCards = 0;
        _purchasedCardsRound.Clear();
        _purchasedIndices.Clear();
        _rootBaseScale = cardParent.localScale;
    }




    public void MarkCardSelected(UpgradeBase card)
    {
        if (card == null)
            return;

        selectedCards.Add(card);
    }

    public void OpenFreePickShop(string shopTitle, 
                                 UpgradeCategory category = UpgradeCategory.General, 
                                 int choices = 3, 
                                 int maxPurchases = 1,
                                 List<UpgradeCategory> exclusions = null)
    {
        _shopMode = ShopMode.FreePick;
        _maxPurchases = maxPurchases;
        _numCardsToDisplay = choices;
        _drawWithCardWeights = false;

        OnShopOpened?.Invoke(ShopType.FreePick);
        OpenShopInternal(shopTitle, category,exclusions);
    }

    

    public void OpenShop(string shopTitle, UpgradeCategory category = UpgradeCategory.General)
    {
        _shopMode = ShopMode.Normal;
        _maxPurchases = int.MaxValue;
        _drawWithCardWeights = true;
        _numCardsToDisplay = RunStateManager.Instance.GetShopOptionCount(baseCardsToDisplay);

        OnShopOpened?.Invoke(ShopType.Normal);
        OpenShopInternal(shopTitle, category);
    }

    private void OpenShopInternal(string title, UpgradeCategory category, List<UpgradeCategory> exclusions = null)
    {
        // your existing OpenShop logic

        pagesController.Initialize();
        
        _currentShopCategory = category;
        _currentExclusions = exclusions;
        selectedIndex = 0;
        _numPurchasedCards = 0;
        _purchasedIndices.Clear();
        _purchasedCardsRound.Clear();
        _cardToContainer.Clear();
        cardParent.localScale = _rootBaseScale - Vector3.one * (Mathf.Max(_numCardsToDisplay - baseCardsToDisplay, 0) * scaleSubPerExtraCard);
        ShowUpgradeUI();


        Cleanup();

        ConfigureShopModeUI();
        shopTitle.text = title;


        shopRootRect.gameObject.SetActive(true);
        keyPromptController.PushPrompts(keyPrompts);
        StartCoroutine(InputDelay());

        foreach(GameObject obj in dependentObjects)
        {
            obj.SetActive(true);
        }
        

        List<UpgradeBase> pool = CreateDrawPool(category, exclusions: exclusions);

        if (pool.Count < _numCardsToDisplay)
        {
            Debug.LogError(
                $"UpgradeCardManager => Not enough available cards to show! " +
                $"Card Pool size => {pool.Count}. " +
                $"Pool requirement => {_numCardsToDisplay}"
            );

            Close();
            return;
        }

        List<UpgradeBase> frontCards = SelectCards(pool, _numCardsToDisplay);
        List<UpgradeBase> backCards = SelectCards(pool, _numCardsToDisplay); 

        for(int i = 0; i < _numCardsToDisplay; i++)
        {
            GameObject cardContainer = new GameObject(name: $"CardParent_{i}");
            cardContainer.transform.parent = cardParent;

            RectTransform rectT = cardContainer.AddComponent<RectTransform>();

            rectT.SetParent(cardParent, false);
            rectT.localScale = Vector3.one;
            

            rectT.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                cardWidth
            );

            for(int j = 0; j < 2; j++)
            {
                GameObject uiObj = Instantiate(cardUIPrefab, parent: cardContainer.transform);
                RectTransform rectT_ = cardContainer.GetComponent<RectTransform>();

                uiObj.SetActive(j == 0);

                rectT_.transform.position = new Vector3(rectT_.transform.position.x,rectT_.transform.position.y);

                UpgradeCardUI ui = uiObj.GetComponent<UpgradeCardUI>();
                UpgradeBase upgrade = j == 0 ? frontCards[i] : backCards[i];

                ui.Setup(upgrade, categoryDatabase);

                if(j == 0)
                {
                    currentCards.Add(ui);
                    
                }
                else
                {
                    nextCards.Add(ui);
                }

                _cardToContainer.Add(ui, rectT);
            }

            cardContainers.Add(rectT);
        }

        HighlightCard();
    }

    private void ConfigureShopModeUI()
    {
        switch (_shopMode)
        {
            case ShopMode.Normal:
                currencyDisplay.SetActive(true);
                break;

            case ShopMode.FreePick:
                currencyDisplay.SetActive(false);
                break;
        }
    }



    public List<UpgradeBase> CreateDrawPool(UpgradeCategory category, 
                                            bool eliminateRoundPurchases = false, 
                                            bool hideCantAppear = true, 
                                            bool limitToPermanentDuration = false,
                                            List<UpgradeCategory> exclusions = null)
    {
        List<UpgradeBase> pool = new();

        foreach(UpgradeBase card in Cards)
        {
            if(card == null)
                continue;

            

            if(exclusions != null) 
            {
                if(exclusions.Contains(card.Category))
                    continue;
            }
                

            if(card.Category != category && category != UpgradeCategory.General)
                continue;

            if(!card.CanAppear() && hideCantAppear)
                continue;

            if(limitToPermanentDuration && card.Duration != UpgradeDuration.Permanent)
                continue;


            if(_purchasedCardsRound.Contains(card) && eliminateRoundPurchases)
                continue;

            

            pool.Add(card);
        }

        

        return pool;
    }


    private UpgradeBase SelectWeightedUpgrade(List<UpgradeBase> pool)
    {
        if (pool.Count == 0)
            return null;

        float totalWeight = 0;
        
        foreach (UpgradeBase upgrade in pool)
        {
            float weight = upgrade.GetOfferWeight();
            weight = Mathf.Max(0f, weight);

            totalWeight += weight;
        }

        if (totalWeight <= 0f)
            return pool[UnityEngine.Random.Range(0,pool.Count)];

        float roll = UnityEngine.Random.Range(0f, totalWeight);

        // you can image that we are creating a number line where each 
        // weight takes up its own portion on the line.
        // We move from left to right on the line by subtracting values
        // until our roll falls below 0 at that tells which section it belongs to.
        // This only works because we use the same order of adding the weights
        // as subtracting them

        foreach (UpgradeBase upgrade in pool)
        {
            roll -= Mathf.Max(0f, upgrade.GetOfferWeight());

            if (roll <= 0f)
                return upgrade;
        }

        return pool[^1];
    }

    private List<UpgradeBase> SelectCards(List<UpgradeBase> pool, int amount)
    {
        List<UpgradeBase> results = new();

        List<UpgradeBase> availiablePool = new(pool);

        int selectionCount = Mathf.Min(amount,availiablePool.Count);


        for (int i = 0; i < selectionCount; i++)
        {
            UpgradeBase selected;

            if(_drawWithCardWeights)
                selected = SelectWeightedUpgrade(availiablePool);
            else
                selected = availiablePool.GetRandom();

            if (selected == null)
                break;

            results.Add(selected);

            availiablePool.Remove(selected);
        }

        return results;
    }


    private void DrawNextCards()
    {
        nextCards.Clear();

        List<UpgradeBase> pool = CreateDrawPool(_currentShopCategory, exclusions: _currentExclusions);

        if (pool.Count < _numCardsToDisplay)
        {
            Debug.LogError(
                $"UpgradeCardManager => Not enough available cards to show! " +
                $"Card Pool size => {pool.Count}. " +
                $"Pool requirement => {_numCardsToDisplay}"
            );

            Close();
            return;
        }

        List<UpgradeBase> picked = SelectCards(pool, currentCards.Count);

        for(int i = 0; i < currentCards.Count; i++)
        {
            GameObject uiObj = Instantiate(
                cardUIPrefab,
                cardContainers[i]
            );

            uiObj.SetActive(false);

            UpgradeCardUI ui = uiObj.GetComponent<UpgradeCardUI>();
            ui.Setup(picked[i], categoryDatabase);

            nextCards.Add(ui);
        }
    }



    private void Update()
    {
        if (InputFocusManager.CurrentOwner != null)
        {
            Debug.LogWarning($"Shop is open current owner is not null . Current owner =>{InputFocusManager.CurrentOwner}");
            return;
        }

        if (OverlayManager.Instance.IsPaused)
            return;
            
        if (currentCards.Count == 0)
            return;
            
        if(GameStateManager.Instance.CurrentState != GameState.RoundReward)
            return;

        if(HandleShopCloseInput())
            return;
        if(HandlePurchaseInput())
            return;
        if(HandleRerollInput())
            return;
        if(HandleInventoryInput())
            return;
        if(HandleViewDrawPoolInput())
            return;

        HandleNaviagationInput();
    }

    // Update Loop Input
    // --------------------------------------------------------------------------------------------

    private bool HandleRerollInput()
    {
        if(_rerollLock)
            return false;

        if(!RunStateManager.Instance.CanRerollShop)
            return false;

        if(!InputBindingManager.Instance.GetKeyDown(rerollKey))
            return false;

        if(RunStateManager.Instance.ConsumeShopReroll())
            StartCoroutine(RerollSequence());

        return true;
    }

    private bool HandlePurchaseInput()
    {
        if (!InputBindingManager.Instance.GetKeyDown(purchaseKey))
            return false;
        
        TryPurchaseCard();
        return true;
    }



    private bool HandleShopCloseInput()
    {
        if(!_canContinue)
            return false;

        if(!InputBindingManager.Instance.GetKeyDown(continueKey))
            return false;

        switch(_shopMode)
        {
            case ShopMode.Normal:
                    _canContinue = false;
                    if(Player.Instance.Health < Player.Instance.MaxHealth && _numPurchasedCards == 0)
                        Player.Instance.HealPlayer(1);
                    Close();
                break;
            case ShopMode.FreePick:
                break;
        }

        return true;
    }

    private bool HandleInventoryInput()
    {
        if(!InputBindingManager.Instance.GetKeyDown(inventoryKey))
            return false;

        inventory.SetVisibilityData(pagesController, shopPageIndex);

        if(inventory.TryOpen(keyPromptController,inventoryOpenSettings))
            pagesController.ShowPage(inventoryPageIndex);
        
        return true;
    }

    private bool HandleViewDrawPoolInput()
    {
        if(!InputBindingManager.Instance.GetKeyDown(viewDrawPoolKey))
            return false;
        
        drawPoolInventory.SetTargetCategory(CurrentUpgradeCategory);
        drawPoolInventory.SetVisibilityData(pagesController, shopPageIndex);

        if(drawPoolInventory.TryOpen(keyPromptController,drawPoolOpenSettings))
            pagesController.ShowPage(drawPoolPageIndex);
        
        return true;
    }

    private void HandleNaviagationInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            if(MoveSelection(-1))
                panelNudge.NudgeLeft();

            HighlightCard();
        }

        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            if(MoveSelection(1))
                panelNudge.NudgeRight();
            
            HighlightCard();    
        }
    }

    private IEnumerator RerollSequence()
    {
        _rerollLock = true;
        for(int i = 0; i < cardContainers.Count; i++)
        {
            yield return StartCoroutine(currentCards[i].CleanUp());

            AudioHelpers.PlaySoundEffect(flipSound, transform.position);

            yield return cardContainers[i]
                .DOLocalRotate(
                    new Vector3(0,90,0),
                    cardFlipDuration/2f
                )
                .WaitForCompletion();

            Destroy(currentCards[i].gameObject);

            RefreshInvalidNextCards();

            currentCards[i] = nextCards[i];

            currentCards[i].gameObject.SetActive(true);

            yield return cardContainers[i]
                .DOLocalRotate(
                    Vector3.zero,
                    cardFlipDuration/2f
                )
                .WaitForCompletion();

            yield return new WaitForSeconds(cardFlipDelay);
        }

 

        _purchasedIndices.Clear();

        // Create the next hidden cards
        DrawNextCards();

        HighlightCard();



        yield return new WaitForSeconds(0.5f);

        _rerollLock = false;
    }

    private void RefreshInvalidNextCards()
    {
        List<UpgradeCardUI> invalidCards = nextCards
            .Where(card =>
                card == null ||
                card.Base == null ||
                !CanAppear(card.Base))
            .ToList();

        if (invalidCards.Count == 0)
            return;

        HashSet<UpgradeBase> validCardsToAvoid = nextCards
            .Where(card =>
                card != null &&
                card.Base != null &&
                CanAppear(card.Base))
            .Select(card => card.Base)
            .ToHashSet();

        List<UpgradeBase> pool = CreateDrawPool(_currentShopCategory, exclusions: _currentExclusions)
            .Where(option => !validCardsToAvoid.Contains(option))
            .ToList();

        if (pool.Count < invalidCards.Count)
        {
            Debug.LogError(
                $"Not enough valid cards to replace invalid cards. " +
                $"Needed {invalidCards.Count}, found {pool.Count}."
            );

            return;
        }

        List<UpgradeBase> picked = SelectCards(pool, invalidCards.Count);

        for (int i = 0; i < invalidCards.Count; i++)
        {
            invalidCards[i].Setup(picked[i], categoryDatabase);
        }
    }

    public void DuplicateCard(UpgradeBase upgrade)
    {
        _purchasedCardsRound.Add(upgrade);
        upgrade.OnSelected(); 
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        RegisterPurchase(upgrade);
    }

    private void TryPurchaseCard()
    {
        if (_rerollLock)
            return;

        if (selectedIndex < 0 || selectedIndex >= currentCards.Count)
            return;

        if (!IsSelectable(selectedIndex))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        // The UpfradeCardUI class contains a reference to the UpgradeOption class "Option" we need to make 
        // sure that several things are true before giving the player the card. First, the upgrade 
        // option within the class must not be null. Then, the player must posses enough currency.

        UpgradeCardUI cardUI = currentCards[selectedIndex];

        //bool transactionAllowed = cardUI.Base != null;

        if(cardUI.Base == null || !cardUI.Base.CanAppear())
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

    
        if(_numPurchasedCards >= _maxPurchases)
            return;

        bool transactionSucceeded = false;
            
        switch(_shopMode)
        {
            case ShopMode.FreePick:
                transactionSucceeded = true;
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
                break;
            case ShopMode.Normal:

                int finalCost = UpgradeCostManager.Instance != null ? UpgradeCostManager.Instance.GetModifiedCost(currentCards[selectedIndex].Base) :
                                                             currentCards[selectedIndex].Base.Cost;
                
                if(CurrencyManager.Instance.TrySpendCurrency(finalCost))
                {
                    transactionSucceeded = true;
                    AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.purchase, transform.position);
                }
                    
                break;

        }

        if (!transactionSucceeded)
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative,transform.position);
            return;
        }

        // If the transaction goes through, then we incremnent the number of purchased cards and handle that logic.
        UpgradeBase upgradeBase = cardUI.Base;
        _numPurchasedCards++;

        
        // Handle some visual logic when the card is purchased. Making it greyed out, etc.
        cardUI.SetPurchased();
        

        // We store which indicies have been purchased so that the player can't navigate over any of the
        // 3 cards that have been purchased that are currently in the shop. We reset this hash set when we 
        // reroll the shop
        _purchasedIndices.Add(selectedIndex);

        // We store the current shop purchased cards so that we can allow for making it so that
        // we have unique cards every shop reroll. Im not currently using that however, but is an option
        // When drawing cards
        _purchasedCardsRound.Add(upgradeBase);
        //_filteredPurchasedCards.Add(upgradeBase);
        

      
        // We store a history of all the cards that we have purchased for a few things (like displaying all the cards
        // the player chose at the end of the game). We use a dictionary here so that we can store the card with how
        // many of that card the player has purchased.
        RegisterPurchase(upgradeBase);

        // Apply after the stack count has been updated.
        upgradeBase.OnSelected();

        // We have a little animation when the player selects the card
        cardUI.PlaySelectAnimation();

        // Just an event that another class might listen to for when a card is purchased. What I was using
        // This for was to say that the player can get a small heal when they skip purchasing cards for 
        // the round if they haven't bought anything
        OnCardPurchased?.Invoke(cardUI.Base);

        if(_numPurchasedCards == _maxPurchases)
            Close();
        
    }

    private bool MoveSelection(int direction)
    {
        if (currentCards.Count == 0)
            return false;

        int start = selectedIndex;

        do
        {
            selectedIndex =
                (selectedIndex + direction + currentCards.Count)
                % currentCards.Count;

            HighlightCard();
            return true;

            /*
            if (IsSelectable(selectedIndex))
            {
                HighlightCard();
                return true;
            }
            else
            {
                return false;
            }
            */

        } while (selectedIndex != start);
    }


    
    private void Close()
    {
        if(shopRootRect)
            shopRootRect.gameObject.SetActive(false);

        keyPromptController.PopPrompts();
        Cleanup();
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        UpgradeSelectionComplete?.Invoke();

        InputBindingManager.Instance.BlockUntilRelease(continueKey);
        InputBindingManager.Instance.BlockUntilRelease(purchaseKey);
    }


    private void HighlightCard()
    {
        AudioHelpers.PlaySoundEffect(
            AudioLibrary.Instance.Database.navigateMajor,
            transform.position
        );

        for (int i = 0; i < currentCards.Count; i++)
        {
            currentCards[i].SetHighlighted(
                i == selectedIndex,
                useOutline: true
            );

            nextCards[i].SetHighlighted(i == selectedIndex);

            if (i != selectedIndex)
                continue;

            UpgradeCardUI selectedCard = currentCards[i];

            cardName.text = selectedCard.Base.DisplayName.ToUpper();

            if (_shopMode == ShopMode.FreePick)
            {
                typewriter.StartTyping(
                    selectedCard.GetDescription()
                );
            }
            else
            {
                int finalCost =
                    UpgradeCostManager.Instance != null
                        ? UpgradeCostManager.Instance.GetModifiedCost(selectedCard.Base)
                        : selectedCard.Base.Cost;

                typewriter.StartTyping(
                    $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${finalCost}</color>] : " + 
                    selectedCard.GetDescription()
                );
            }
        }
    }

   private void HandleCurrencyChanged(int newValue)
    {
        if (GameStateManager.Instance.CurrentState != GameState.RoundReward)
            return;

        RefreshSelectedCardText();
    }

    private void RefreshSelectedCardText()
    {
        if (currentCards.Count == 0)
            return;

        if (selectedIndex < 0 ||
            selectedIndex >= currentCards.Count)
            return;

        UpgradeCardUI selectedCard =
            currentCards[selectedIndex];

        if (selectedCard?.Base == null)
            return;

        cardName.text = selectedCard.Base.DisplayName.ToUpper();

        if (_shopMode == ShopMode.FreePick)
        {
            typewriter.SetInstant(
                selectedCard.GetDescription()
            );

            return;
        }

        int finalCost =
            UpgradeCostManager.Instance != null
                ? UpgradeCostManager.Instance.GetModifiedCost(selectedCard.Base)
                : selectedCard.Base.Cost;

        typewriter.SetInstant(
            $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${finalCost:N0}</color>] : " +
            selectedCard.GetDescription()
        );
    }


    public void RemoveUpgrade(UpgradeBase upgrade)
    {
        if (upgrade == null)
            return;

        //Debug.LogError($"TRYING TO REMOVE A CARD => {upgrade.name}");
        if (!_filteredPurchasedCards.TryGetValue(upgrade,out int stackCount))
            return;

        stackCount--;

        if (stackCount <= 0)
        {
            _filteredPurchasedCards.Remove(upgrade);
            OnCardExpired?.Invoke(upgrade);
        }
        else
        {
            _filteredPurchasedCards[upgrade] = stackCount;
            OnCardStackChanged?.Invoke(upgrade,stackCount);
        }

        /*
        if (!AllChosenCards.TryGetValue(
                upgrade,
                out int stackCount))
        {
            return;
        }

        stackCount--;

        if (stackCount <= 0)
        {
            AllChosenCards.Remove(upgrade);
            OnCardExpired?.Invoke(upgrade);
        }
        else
        {
            AllChosenCards[upgrade] = stackCount;

            OnCardStackChanged?.Invoke(
                upgrade,
                stackCount
            );
        }
        */
    }

    public List<UpgradeBase> GrantSetCards(int amount)
    {
        List<UpgradeBase> grantedCards = new();

        amount = Mathf.Clamp(amount, 0, setCardPool.Count);

        for (int i = 0; i < amount; i++)
        {
            UpgradeBase selected = setCardPool[i];

            // Register FIRST so OnSelected sees the new stack count.
            RegisterPurchase(selected);

            selected.OnSelected();

            OnCardPurchased?.Invoke(selected);

            grantedCards.Add(selected);
        }

        return grantedCards;
    }

    public List<UpgradeBase> GrantRandomCards(int amount,UpgradeCategory category = UpgradeCategory.General,bool useWeights = true)
    {
        List<UpgradeBase> grantedCards = new();
        HashSet<UpgradeBase> grantedThisBatch = new();

        if (amount <= 0)
            return grantedCards;

        for (int i = 0; i < amount; i++)
        {
            // Rebuild the pool every roll so CanAppear() uses
            // the CURRENT purchased-card state.
            List<UpgradeBase> pool = CreateDrawPool(category,limitToPermanentDuration: true);

            // Don't give the same card twice in this batch.
            pool.RemoveAll(card =>card == null ||grantedThisBatch.Contains(card) ||!card.CanAppear());

            if (pool.Count == 0)
                break;

            UpgradeBase selected = useWeights ? SelectWeightedUpgrade(pool) : pool.GetRandom();

            if (selected == null)
                break;

            // Defensive final check immediately before granting.
            if (!selected.CanAppear())
            {
                i--;
                continue;
            }

            // Register FIRST so OnSelected sees the new stack count.
            RegisterPurchase(selected);

            selected.OnSelected();

            OnCardPurchased?.Invoke(selected);

            grantedCards.Add(selected);
            grantedThisBatch.Add(selected);
        }

        return grantedCards;
    }

    private void Cleanup()
    {
        
        // --- Cleanup UI cards safely ---
        foreach (Transform child in cardParent)
        {
            DOTween.Kill(child, complete: false);
            Destroy(child.gameObject);
        }

        currentCards.Clear();

        foreach(GameObject obj in dependentObjects)
        {
            obj.SetActive(false);
        }

        
        nextCards.Clear();
        cardContainers.Clear();
    }



}
