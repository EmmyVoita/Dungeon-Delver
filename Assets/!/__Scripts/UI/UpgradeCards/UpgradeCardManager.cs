using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class UpgradeCardManager : MonoBehaviour
{
    public static UpgradeCardManager Instance { get; private set; }

    public static event Action UpgradeSelectionComplete;


    [Header("References")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private GameObject upgradeIconPrefab;
    [SerializeField] private Transform upgradeIconParent;
    [SerializeField] private Transform cardParent;
    [SerializeField] private TextTypewriter descriptionTypewriter;
    [SerializeField] private DescriptionPanelController descriptionPanel;
    [SerializeField] private List<GameObject> dependentObjects;


    [Header("Available Upgrades")]
    [SerializeField] private List<UpgradeBase> cards;



    private List<UpgradeBase> selectedCards = new ();
    private List<UpgradeCardUI> currentCards = new();
    private Dictionary<UpgradeBase, int> cardHistory = new();
    public Dictionary<UpgradeBase, int> AllChosenCards => cardHistory;
    private int selectedIndex = 0;

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

            UnityEngine.Assertions.Assert.IsNotNull(
                upgradeIconParent,
                $"{nameof(UpgradeCardManager)}: upgradeIconParent is not assigned"
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

        List<UpgradeOption> picked = pool.PickUnique(count); 

        foreach (var option in picked)
        {
            GameObject uiObj = Instantiate(cardUIPrefab, cardParent);
            UpgradeCardUI ui = uiObj.GetComponent<UpgradeCardUI>();
            ui.Setup(option);
            currentCards.Add(ui);
        }

        HighlightCard();
    }



    private void Update()
    {
        if (currentCards.Count == 0 || GameStateManager.Instance.CurrentState != GameState.UpgradeSelection) return;

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Interact) && RunStateManager.Instance.CanRerollShop)
        {
            bool goThroughWithReroll = RunStateManager.Instance.ConsumeShopReroll();
            if(goThroughWithReroll) ShowCardChoices(3);
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

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Jump))
        {
            SkipUpgradeAndHeal();
        }
    }

    private void SelectCurrentOption()
    {
        if (selectedIndex < 0 || selectedIndex >= currentCards.Count)
            return;

        UpgradeCardUI cardUI = currentCards[selectedIndex];

        AudioSettingsManager.PlaySelectSound();

        
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
        AudioSettingsManager.PlaySelectSound();
        UpgradeSelectionComplete?.Invoke();
    }


    private void HighlightCard()
    {
        AudioSettingsManager.PlayNavigateSound();
        
        for (int i = 0; i < currentCards.Count; i++)
        {
            currentCards[i].SetHighlighted(i == selectedIndex);
            if (i == selectedIndex)
            {
                string description = currentCards[i].GetDescription();
                //descriptionTypewriter.StartTyping(currentCards[i].GetDescription());
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
    }
}
