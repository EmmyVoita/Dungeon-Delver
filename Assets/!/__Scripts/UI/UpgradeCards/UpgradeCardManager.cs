using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;
using System.Collections;

public class UpgradeCardManager : MonoBehaviour
{
    public static UpgradeCardManager Instance { get; private set; }

    public static event Action UpgradeSelectionComplete;

    [SerializeField] private float cardWidth = 350f;
    [SerializeField] private float cardFlipDelay = 0.5f;
    [SerializeField] private float cardFlipDuration = 0.3f;
    [SerializeField] private SoundEffect flipSound;


    [Header("References")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private GameObject upgradeIconPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private DescriptionPanelController descriptionPanel;
    [SerializeField] private List<GameObject> dependentObjects;

    [SerializeField] private List<RectTransform> cardContainers;


    [Header("Available Upgrades")]
    [SerializeField] private List<UpgradeBase> cards;



    private List<UpgradeBase> selectedCards = new ();
    private List<UpgradeCardUI> currentCards = new();
    private List<UpgradeCardUI> nextCards = new();
    private Dictionary<UpgradeBase, int> cardHistory = new();
    public Dictionary<UpgradeBase, int> AllChosenCards => cardHistory;
    private int selectedIndex = 0;
    private bool _rerollLock;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
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
    }




    public void MarkCardSelected(UpgradeBase card)
    {
        if (card == null)
            return;

        selectedCards.Add(card);
    }


    public void ShowCardChoices(int count = 3)
    {
        selectedIndex = 0;

        Cleanup();

        foreach(GameObject obj in dependentObjects)
        {
            obj.SetActive(true);
        }
        

        List<UpgradeOption> pool = new();

        foreach (var effect in cards)
        {
            if (effect == null)
                continue;

            pool.Add(new UpgradeOption(effect));
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning("UpgradeCardManager => No available cards to show!.");
            SkipUpgradeAndHeal();
            return;
        }

        List<UpgradeOption> picked = pool.PickUnique(count * 2); 

        for(int i = 0; i < count; i++)
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
                    currentCards.Add(ui);
                else
                {
                    nextCards.Add(ui);
                }
            }

            cardContainers.Add(rectT);
        }

        HighlightCard();
    }

    private void DrawNextCards()
    {
        nextCards.Clear();

        List<UpgradeOption> pool = new();

        foreach(var effect in cards)
        {
            if(effect == null)
                continue;

            pool.Add(new UpgradeOption(effect));
        }

        List<UpgradeOption> picked =
            pool.PickUnique(currentCards.Count);

        for(int i = 0; i < currentCards.Count; i++)
        {
            GameObject uiObj = Instantiate(
                cardUIPrefab,
                cardContainers[i]
            );

            uiObj.SetActive(false);

            UpgradeCardUI ui =
                uiObj.GetComponent<UpgradeCardUI>();

            ui.Setup(picked[i]);

            nextCards.Add(ui);
        }
    }



    private void Update()
    {
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
            selectedIndex = (selectedIndex - 1 + currentCards.Count) % currentCards.Count;
            HighlightCard();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            selectedIndex = (selectedIndex + 1) % currentCards.Count;
            HighlightCard();    
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm) && 
                 GameStateManager.Instance.CurrentState == GameState.UpgradeSelection)
        {
            SelectCurrentOption();
        }

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Jump) && Player.Instance.Health < Player.Instance.MaxHealth)
        {
            SkipUpgradeAndHeal();
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

        // Create the next hidden cards
        DrawNextCards();

        HighlightCard();

        yield return new WaitForSeconds(
                0.5f
        );

        _rerollLock = false;
    }

    

    private void SelectCurrentOption()
    {
        if (selectedIndex < 0 || selectedIndex >= currentCards.Count)
            return;

        UpgradeCardUI cardUI = currentCards[selectedIndex];

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

        
        if (cardHistory.ContainsKey(cardUI.Option.Base))
        {
            cardHistory[cardUI.Option.Base]++;
        }
        else
        {
            cardHistory[cardUI.Option.Base] = 1;
        }
        

        cardUI.PlaySelectAnimation(() =>
        {
            FinishSelection(cardUI.Option);
        });
    }

    private void FinishSelection(UpgradeOption option)
    {
        if (option == null)
        {
            Debug.LogError("UpgradeCardManager => Selected UpgradeOption was null in FinishSelection().");
            Cleanup();
            UpgradeSelectionComplete?.Invoke();
            return;
        }

        Cleanup();
      
        option.OnSelected();   

        UpgradeSelectionComplete?.Invoke();
    }



    private void SkipUpgradeAndHeal()
    {
        Cleanup();
        Player.Instance.HealPlayer(1);
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        UpgradeSelectionComplete?.Invoke();
    }


    private void HighlightCard()
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        
        for (int i = 0; i < currentCards.Count; i++)
        {
            currentCards[i].SetHighlighted(i == selectedIndex);
            nextCards[i].SetHighlighted(i == selectedIndex);

            if (i == selectedIndex)
            {
                cardName.text = currentCards[i].Option.DisplayName.ToUpper();
                descriptionPanel.Show(currentCards[i].GetDescription());
            }
        }
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
