using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class UpgradeCardManager : MonoBehaviour
{
    public enum UpgradeSelectionError
    {
        None,
        InvalidIndex,
        MissingCard,
        MissingUpgrade,
        MissingIconPrefab,
        MissingIconParent
    }


    public static UpgradeCardManager Instance { get; private set; }

    public static event Action UpgradeSelectionComplete;

    [Header("UI")]
    public GameObject cardUIPrefab;
    public GameObject upgradeIconPrefab;
    public Transform upgradeIconParent;
    public Transform cardParent;
    public Transform descriptionBox;
    public Transform healOptionBox;
    public BackgroundDimmerController backgroundDimmer;
    public TextTypewriter descriptionTypewriter;

    [Header("Available Upgrades")]
    public List<IntermediateEffectSO> intermediateEffects;
    public List<UpgradeCard> allCards;
    private Dictionary<UpgradeCard, UpgradeBase> selectedCards = new Dictionary<UpgradeCard, UpgradeBase>();

    private List<UpgradeCardUI> currentCards = new();
    private int selectedIndex = 0;

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

    private List<UpgradeCard> PickUniqueCards(List<UpgradeCard> source, int count)
    {
        List<UpgradeCard> result = new List<UpgradeCard>();

        if (source.Count <= count)
        {
            // Not enough unique cards — allow duplicates later if needed
            result.AddRange(source);
            return result;
        }

        List<UpgradeCard> pool = new List<UpgradeCard>(source);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    public void MarkCardSelected(UpgradeCard card)
    {
        if (card == null || card.upgrade == null)
            return;

        selectedCards[card] = card.upgrade as UpgradeBase;
    }



    private List<UpgradeCard> GetAvailableUpgrades()
    {
        List<UpgradeCard> available = new List<UpgradeCard>();

        // Add global upgrades first
        foreach (var card in allCards)
        {
            if (card == null) continue;

            ScriptableObject selectedEffect = selectedCards.ContainsKey(card) ? selectedCards[card] : null;

            if (selectedEffect != null && card.canStack)
                available.Add(card);
            else if (selectedEffect == null)
                available.Add(card);
        }

        // Add ability-specific upgrades
        var ability = Player.Instance.CurrentAbility;
        if (ability != null && ability.abilitySpecificUpgrades != null)
        {
            foreach (var abilityCard in ability.abilitySpecificUpgrades)
            {
                if (abilityCard == null) continue;

                // Only add if not already selected
                if (!selectedCards.ContainsKey(abilityCard))
                    available.Add(abilityCard);
            }
        }

        return available;
    }


    // ----------------------------
    //     SPAWN CARD CHOICES
    // ----------------------------
    public void ShowCardChoices(int count = 3)
    {
        Debug.Log("▶ Showing upgrade card choices...");
        ScreenDimmerManager.Instance.AddDimSource("upgrade");
        descriptionBox.gameObject.SetActive(true);

        CleanupCards();

        if(Player.Instance.Health <= Player.Instance.MaxHealth)
        {
            healOptionBox.gameObject.SetActive(true);
            healOptionBox.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text =
                $"Skip Upgrade [{InputBindingManager.Instance.GetKey(InputActionType.Jump)}] and Heal for 1 health";
        }
        else
        {
            healOptionBox.gameObject.SetActive(false);
        }

        selectedIndex = 0;

        if(RoundManager.Instance.IsBossRound)
        {
            // Pick random cards
            List<UpgradeCard> available = GetAvailableUpgrades();
            if (available.Count == 0)
            {
                Debug.LogWarning("No available upgrade cards to show!");
                SkipUpgradeAndHeal();
                return;
            }

            List<UpgradeCard> picked = PickUniqueCards(available, count);

            while (picked.Count < count)
            {
                picked.Add(available[UnityEngine.Random.Range(0, available.Count)]);
            }

            foreach (var card in picked)
            {
                ICardOption option = new UpgradeCardOption(card);

                GameObject uiObj = Instantiate(cardUIPrefab, cardParent);
                UpgradeCardUI ui = uiObj.GetComponent<UpgradeCardUI>();
                ui.Setup(option);
                currentCards.Add(ui);
            }
        }
        else
        {
            List<IntermediateEffectSO> picked = new List<IntermediateEffectSO>();

            while (picked.Count < count)
            {
                picked.Add(intermediateEffects[UnityEngine.Random.Range(0, intermediateEffects.Count)]);
            }

            foreach (var card in picked)
            {
                ICardOption option = new IntermediateEffectOption(card);

                GameObject uiObj = Instantiate(cardUIPrefab, cardParent);
                UpgradeCardUI ui = uiObj.GetComponent<UpgradeCardUI>();
                ui.Setup(option);
                currentCards.Add(ui);
            }
        }

     
        HighlightCard();
    }


    private void Update()
    {
        if (currentCards.Count == 0 || GameStateManager.Instance.CurrentState != GameState.UpgradeSelection) return;

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Interact) && RunStateManager.Instance.CanRerollShop)
        //if(Input.GetKeyDown(KeyCode.R) && RunStateManager.Instance.CanRerollShop)
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
        ScreenDimmerManager.Instance.RemoveDimSource("upgrade");

        cardUI.PlaySelectAnimation(() =>
        {
            FinishSelection(cardUI.Option);
        });
    }

    private void FinishSelection(ICardOption option)
    {
        if (option == null)
        {
            Debug.LogError("❌ Selected option was null.");
            CleanupCards();
            UpgradeSelectionComplete?.Invoke();
            return;
        }

        CleanupCards();
        descriptionBox.gameObject.SetActive(false);

      
        option.OnSelected();   // 🔑 THIS IS THE KEY LINE

        UpgradeSelectionComplete?.Invoke();
    }



    private void SkipUpgradeAndHeal()
    {
        CleanupCards();
        healOptionBox.gameObject.SetActive(false);
        Player.Instance.HealPlayer(1);
        AudioSettingsManager.PlaySelectSound();
        ScreenDimmerManager.Instance.RemoveDimSource("upgrade");
        descriptionBox.gameObject.SetActive(false);
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
                descriptionTypewriter.StartTyping(currentCards[i].GetDescription());
            }
        }
    }

    /*
    public void SelectCard(UpgradeCard card)
    {
        if (card == null)
        {
            Debug.LogError("❌ No card selected!");
            return;
        }

        AudioSettingsManager.PlaySelectSound();
        ScreenDimmerManager.Instance.RemoveDimSource("upgrade");

        UpgradeCardUI cardUI = currentCards[selectedIndex];
        if (cardUI == null)
        {
            Debug.LogError("❌ No card UI found for selected index: " + selectedIndex);
            return;
        }

        //Debug.Log("▶ Playing select animation for card: " + card.cardName);
        cardUI.PlaySelectAnimation(() => FinishSelectionCallback()());
    }

    
    // ----------------------------
    //     FINISH SELECTION
    // ----------------------------
    public Action FinishSelectionCallback() =>
    () =>
    {
        Debug.Log("▶ Finishing upgrade selection...");
        //var error = ValidateSelection(out UpgradeCard card);

        
        if (error != UpgradeSelectionError.None)
        {
            HandleSelectionError(error);
            return;
        }
        

        CleanupCards();

        
        descriptionBox.gameObject.SetActive(false);
       

        GameObject iconObj = Instantiate(upgradeIconPrefab, upgradeIconParent);
        iconObj.GetComponent<UpgradeIconUI>().Initialize(card.upgrade as UpgradeBase);


        UpgradeManager.Instance.AddUpgrade(card.upgrade as UpgradeBase);

        selectedCards[card] = card.upgrade as UpgradeBase;

        UpgradeSelectionComplete?.Invoke();

    };
    */

 



    private void CleanupCards()
    {
        // --- Cleanup UI cards safely ---
        foreach (Transform child in cardParent)
        {
            DOTween.Kill(child, complete: false);
            Destroy(child.gameObject);
        }

        currentCards.Clear();

        healOptionBox.gameObject.SetActive(false);
    }
}
