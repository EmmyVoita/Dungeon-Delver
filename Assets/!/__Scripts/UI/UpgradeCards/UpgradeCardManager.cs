using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;
using System.Collections;
using System.Linq;

public class UpgradeCardManager : MonoBehaviour
{
    public static UpgradeCardManager Instance { get; private set; }

    public static event Action UpgradeSelectionComplete;
    public static event Action<UpgradeOption> OnCardPurchased;

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
    [SerializeField] private DescriptionPanelController descriptionPanel;
    [SerializeField] private List<GameObject> dependentObjects;
    [SerializeField] private UIPanelNudge panelNudge;

    [SerializeField] private List<RectTransform> cardContainers;


    [Header("Available Upgrades")]
    [SerializeField] private List<UpgradeBase> cards;
    [SerializeField] private int _numCardsToDisplay = 3;


    private List<UpgradeBase> selectedCards = new ();
    private List<UpgradeCardUI> currentCards = new();
    private List<UpgradeCardUI> nextCards = new();
    public Dictionary<UpgradeBase, int> AllChosenCards => _purchasedCards;
    private int selectedIndex = 0;
    private bool _rerollLock;
    private int _numPurchasedCards;


    private HashSet<int> _purchasedIndices = new();
    private HashSet<UpgradeBase> _purchasedCardsRound = new();
    private Dictionary<UpgradeBase, int> _purchasedCards = new();
    private Dictionary<UpgradeBase, UpgradeIconUI> activeUpgradeIcons = new();
    private Dictionary<UpgradeCardUI, RectTransform> _cardToContainer = new();

    public int PurchasedCardsCount => _numPurchasedCards;

    public bool HasCard(UpgradeOption cardOption)
    {
        return _purchasedCards.ContainsKey(cardOption.Base);
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
        if(previousState == GameState.Paused || newState == GameState.Paused) return;

        if(newState == GameState.UpgradeSelection)
        {
            ShowCardChoices();
        }
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
                $"{nameof(UpgradeCardManager)}: upgradeIconPrefab is not assigned"
            );

        #endif

        _numPurchasedCards = 0;
        _purchasedCardsRound.Clear();
        _purchasedIndices.Clear();
    }




    public void MarkCardSelected(UpgradeBase card)
    {
        if (card == null)
            return;

        selectedCards.Add(card);
    }

    private void UpdateFooterIcon(UpgradeBase upgrade)
    {
        if (!upgrade.displayInFooter)
            return;

        if (activeUpgradeIcons.TryGetValue(upgrade, out var iconUI))
        {
            iconUI.AddStack();
            return;
        }

        UpgradeIconUI newIcon = Instantiate(
            upgradeIconPrefab,
            upgradeIconFooter
        ).GetComponent<UpgradeIconUI>();

        newIcon.Initialize(
            upgrade.icon,
            upgrade.iconMaterial,
            1
        );

        activeUpgradeIcons.Add(upgrade, newIcon);
    }


    public void ShowCardChoices()
    {
        selectedIndex = 0;
        _numPurchasedCards = 0;
        _purchasedIndices.Clear();
        _purchasedCardsRound.Clear();
        _cardToContainer.Clear();


        Cleanup();

        foreach(GameObject obj in dependentObjects)
        {
            obj.SetActive(true);
        }
        

        List<UpgradeOption> pool = CreateDrawPool();

        if (pool.Count == 0 || pool.Count < _numCardsToDisplay * 2)
        {
            Debug.LogError("UpgradeCardManager => No available cards to show!.");
            SkipUpgrade();
            return;
        }

        List<UpgradeOption> picked = pool.PickUnique(_numCardsToDisplay * 2); 

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
                ui.Setup(picked[i * 2 + j]);

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

    private List<UpgradeOption> CreateDrawPool(bool eliminateRoundPurchases = false)
    {
        List<UpgradeOption> pool = new();

        foreach(var effect in cards)
        {
            if(effect == null)
                continue;

            if(!CanAppear(effect))
                continue;

            if(_purchasedCardsRound.Contains(effect) && eliminateRoundPurchases)
                continue;

            pool.Add(new UpgradeOption(effect));
        }

        return pool;
    }

    private void DrawNextCards()
    {
        nextCards.Clear();

        List<UpgradeOption> pool = CreateDrawPool();
        List<UpgradeOption> picked = pool.PickUnique(currentCards.Count);

        for(int i = 0; i < currentCards.Count; i++)
        {
            GameObject uiObj = Instantiate(
                cardUIPrefab,
                cardContainers[i]
            );

            uiObj.SetActive(false);

            UpgradeCardUI ui = uiObj.GetComponent<UpgradeCardUI>();
            ui.Setup(picked[i]);

            nextCards.Add(ui);
        }
    }



    private void Update()
    {
        if (InputFocusManager.CurrentOwner != null)
            return;

        if (OverlayManager.Instance.IsPaused)
            return;
            
        if (currentCards.Count == 0 || GameStateManager.Instance.CurrentState != GameState.UpgradeSelection) return;

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Interact) && RunStateManager.Instance.CanRerollShop && !_rerollLock)
        {
            bool goThroughWithReroll = RunStateManager.Instance.ConsumeShopReroll();
            if(goThroughWithReroll)
            {
                StartCoroutine(RerollSequence());
            } 
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            //selectedIndex = (selectedIndex - 1 + currentCards.Count) % currentCards.Count;
            if(MoveSelection(-1))
                panelNudge.NudgeLeft();


            HighlightCard();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            //selectedIndex = (selectedIndex + 1) % currentCards.Count;
            if(MoveSelection(1))
                panelNudge.NudgeRight();
            
            HighlightCard();    
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm) && 
                 GameStateManager.Instance.CurrentState == GameState.UpgradeSelection)
        {
            TryPurchaseCard();
        }

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Jump))
        {
            if(Player.Instance.Health < Player.Instance.MaxHealth && _numPurchasedCards == 0)
            {
                Player.Instance.HealPlayer(1);
            }

            // Continue
            SkipUpgrade();
        }
    }

    private IEnumerator RerollSequence()
    {
        _rerollLock = true;
        for(int i = 0; i < cardContainers.Count; i++)
        {
            AudioHelpers.PlaySoundEffect(flipSound, transform.position);

            yield return cardContainers[i]
                .DOLocalRotate(
                    new Vector3(0,90,0),
                    cardFlipDuration/2f
                )
                .WaitForCompletion();

            Destroy(currentCards[i].gameObject);

            currentCards[i] = nextCards[i];

            currentCards[i].gameObject.SetActive(true);

            yield return cardContainers[i]
                .DOLocalRotate(
                    Vector3.zero,
                    cardFlipDuration/2f
                )
                .WaitForCompletion();

            yield return new WaitForSeconds(
                cardFlipDelay
            );
        }

 

        _purchasedIndices.Clear();

        // Create the next hidden cards
        DrawNextCards();

        HighlightCard();

        /*
        foreach(RectTransform rectT in cardContainers)
        {
            rectT.gameObject.SetActive(true);
        }
        */

        yield return new WaitForSeconds(
                0.5f
        );

        _rerollLock = false;
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

        bool transactionAllowed = cardUI.Option != null;

        if(!transactionAllowed)
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        if(CurrencyManager.Instance.TrySpendCurrency(currentCards[selectedIndex].Option.Base.Cost))
        {
            cardUI.Option.OnSelected(); 
        }
        else
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }
       
       
        // If the transaction goes through, then we incremnent the number of purchased cards and handle that logic.

        UpgradeBase upgradeBase = cardUI.Option.Base;

        _numPurchasedCards++;


        /*
        // Get the card parent thats associated with the card and set it to inactive
        RectTransform cardParent;
        _cardToContainer.TryGetValue(cardUI, out cardParent);

        if(cardParent != null)
            cardParent.gameObject.SetActive(false);
        */

        
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
        

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.purchase, transform.position);

        // We store a history of all the cards that we have purchased for a few things (like displaying all the cards
        // the player chose at the end of the game). We use a dictionary here so that we can store the card with how
        // many of that card the player has purchased.
        RegisterPurchase(upgradeBase);

        // We have a little animation when the player selects the card
        cardUI.PlaySelectAnimation();

        // Just an event that another class might listen to for when a card is purchased. What I was using
        // This for was to say that the player can get a small heal when they skip purchasing cards for 
        // the round if they haven't bought anything
        OnCardPurchased?.Invoke(cardUI.Option);
        
        UpdateFooterIcon(upgradeBase);
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


    
    private void SkipUpgrade()
    {
        Cleanup();
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        UpgradeSelectionComplete?.Invoke();
    }


    private void HighlightCard()
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        
        for (int i = 0; i < currentCards.Count; i++)
        {
            currentCards[i].SetHighlighted(i == selectedIndex, useOutline: true);
            nextCards[i].SetHighlighted(i == selectedIndex);

            if (i == selectedIndex)
            {
                cardName.text = currentCards[i].Option.DisplayName.ToUpper();
                descriptionPanel.Show($"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${currentCards[i].Option.Base.Cost.ToString("N0")}</color> / " +
                                      $"<color=#{UIColors.ToHex(UIColors.Lavender)}>${CurrencyManager.Instance.CurrentCurrency.ToString("N0")}</color>] : " +
                                      currentCards[i].GetDescription());
            }
        }
    }

    private void HandleCurrencyChanged(int newValue)
    {
        if(GameStateManager.Instance.CurrentState == GameState.UpgradeSelection)
        descriptionPanel.ShowImmediate($"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${currentCards[selectedIndex].Option.Base.Cost.ToString("N0")}</color> / " +
                                      $"<color=#{UIColors.ToHex(UIColors.Lavender)}>${CurrencyManager.Instance.CurrentCurrency.ToString("N0")}</color>] : " +
                                      currentCards[selectedIndex].GetDescription());
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
