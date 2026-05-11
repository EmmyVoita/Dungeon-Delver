using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class AbilitySelectManager : BaseMenu
{
    public static AbilitySelectManager Instance { get; private set; }
    public static Action OnHoverChanged;

    public static Action OnMenuAbilitySelected;

    [Header("References")]
    [SerializeField] private StartOptionsNavigator startOptions;

    [Header("Transition Settings")]
    [SerializeField] private MenuState backState = MenuState.Main;
    [SerializeField] private MenuState returnCardState = MenuState.Main;


    [Header("UI")]
    //public GameObject titleUI;
    //public GameObject highscoreUI;
    public GameObject cardUIPrefab;
    public RectTransform cardContainer; // 🌀 Scrollable parent
    public Transform cardParent;        // Optional fallback (usually same as cardContainer)
    public Transform descriptionBox;
    public BackgroundDimmerController backgroundDimmer;
    public TextTypewriter descriptionTypewriter;
    public TextMeshProUGUI abilityNameText;
    public TextMeshProUGUI abilityCostText;

    [Header("Carousel Settings")]
    public float slideSpeed = 8f;
    public float cardSpacing = 400f; // Auto-calculated if left at 0
    public float unselectedScale = 0.9f;
    public float selectedScale = 1.15f;
    public float unselectedAlpha = 0.6f;

    [Header("Available Abilities")]
    public List<AbilityData> allAbilities;
    [SerializeField] private List<AbilityCardUI> currentCards = new List<AbilityCardUI>();
    [SerializeField] private int selectedIndex = 0;

    public AbilityType ActiveHover => currentCards[selectedIndex].Card.abilityType;

    // For smooth sliding
    private Vector2 targetPosition;
    private float totalWidth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        MenuManager.Instance.RegisterMenu(this);
    }


    // ----------------------------
    //      SPAWN ABILITY CARDS
    // ----------------------------
    public override void OnOpen()
    {
        base.OnOpen();

        if (backgroundDimmer != null)
            backgroundDimmer.FadeIn();

        if (descriptionBox != null)
            descriptionBox.gameObject.SetActive(true);

        // Clear old
        foreach (Transform child in cardParent)
        {
            DOTween.Kill(child);
            Destroy(child.gameObject);
        }
        currentCards.Clear();
        selectedIndex = Mathf.Min(0, Mathf.Max(allAbilities.Count - 1,0));

        // Spawn all abilities
        foreach (var ability in allAbilities)
        {
            GameObject uiObj = Instantiate(cardUIPrefab, cardParent);
            AbilityCardUI ui = uiObj.GetComponent<AbilityCardUI>();
            ui.Setup(ability);
            currentCards.Add(ui);
        }

        // Auto-calc spacing
        if (cardSpacing <= 0 && currentCards.Count > 1)
        {
            RectTransform cardRect = currentCards[0].GetComponent<RectTransform>();
            float width = cardRect.rect.width;

            var layout = cardParent.GetComponent<HorizontalLayoutGroup>();
            float spacing = layout != null ? layout.spacing : 20f;

            cardSpacing = width + spacing;
        }

        HighlightCard(forceInstant: true);

        ScreenDimmerManager.Instance.AddDimSource("PlayScreen");

        OnHoverChanged?.Invoke();
    }

    public override void OnClose()
    {
        base.OnClose();


        if (backgroundDimmer != null)
            backgroundDimmer.FadeOut();

        if (descriptionBox != null)
            descriptionBox.gameObject.SetActive(false);

        // Clear cards
        foreach (Transform child in cardParent)
        {
            DOTween.Kill(child);
            Destroy(child.gameObject);
        }
        currentCards.Clear();

        ScreenDimmerManager.Instance.RemoveDimSource("PlayScreen");
    }

    // ----------------------------
    //         INPUT
    // ----------------------------
    private void Update()
    {
        if (lockInput || currentCards.Count == 0 || !isActive) return;

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            selectedIndex = (selectedIndex - 1 + currentCards.Count) % currentCards.Count;
            OnHoverChanged?.Invoke();
            HighlightCard();
            AudioSettingsManager.PlayNavigateSound();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            selectedIndex = (selectedIndex + 1) % currentCards.Count;
            OnHoverChanged?.Invoke();
            HighlightCard();
            AudioSettingsManager.PlayNavigateSound();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            if (ScoreManager.Instance.HighScore >= currentCards[selectedIndex].Card.scoreRequirement)
            {
                SelectAbility(currentCards[selectedIndex].Card);
            }
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            MenuManager.Instance.RequestMenuTransition(backState);
            AudioSettingsManager.PlayBackSound();
            OnMenuAbilitySelected?.Invoke();
        }


        // Smooth slide effect
        if (cardContainer != null)
        {
            cardContainer.anchoredPosition = Vector2.Lerp(
                cardContainer.anchoredPosition,
                targetPosition,
                Time.unscaledDeltaTime * slideSpeed
            );
        }
    }

    private IEnumerator ScrollAndSelectReturnCard()
    {
        // Find the Return card
        int returnIndex = currentCards.FindIndex(c => c.Card.abilityType == AbilityType.ReturnToMenu);
        if (returnIndex < 0)
        {
            Debug.LogWarning("⚠️ No 'Return' card found!");
            yield break;
        }

        lockInput = true;

        // --- Calculate target anchored position just like HighlightCard() does ---
        float totalWidth = (currentCards.Count - 1) * cardSpacing;
        float startOffset = (totalWidth / 2f) - (selectedIndex * cardSpacing);
        float targetOffset = (totalWidth / 2f) - (returnIndex * cardSpacing);

        Vector2 startPos = new Vector2(startOffset, 0);
        Vector2 endPos = new Vector2(targetOffset, 0);

        float duration = 0.6f;
        float elapsed = 0f;

        // --- Animate the scroll and index together ---
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            // interpolate scroll position
            targetPosition = Vector2.Lerp(startPos, endPos, t);

            // interpolate selection visually
            int interpIndex = Mathf.RoundToInt(Mathf.Lerp(selectedIndex, returnIndex, t));
            if (interpIndex != selectedIndex)
            {
                selectedIndex = interpIndex;
                HighlightCard();
            }

            yield return null;
        }

        // --- Snap to Return card at end ---
        selectedIndex = returnIndex;
        HighlightCard(forceInstant: true);

        // --- Optional: pulse the Return card to emphasize it ---
        var returnCard = currentCards[selectedIndex];
        returnCard.transform
            .DOScale(selectedScale * 1.1f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => returnCard.transform.DOScale(selectedScale, 0.25f));

        // --- Short pause before auto-selecting ---
        yield return new WaitForSeconds(0.25f);

        AudioSettingsManager.PlaySelectSound();

        // --- Select it just like Enter ---
        SelectAbility(currentCards[selectedIndex].Card);
    }



    // ----------------------------
    //     HIGHLIGHT / SELECT
    // ----------------------------
    private void HighlightCard(bool forceInstant = false)
    {
        for (int i = 0; i < currentCards.Count; i++)
        {
            bool isSelected = (i == selectedIndex);
            currentCards[i].SetHighlighted(isSelected);

            // Scale + fade tween
            float targetScale = isSelected ? selectedScale : unselectedScale;
            float targetAlpha = isSelected ? 1f : unselectedAlpha;

            if (forceInstant)
            {
                currentCards[i].transform.localScale = Vector3.one * targetScale;
                currentCards[i].SetAlpha(targetAlpha);
            }
            else
            {
                currentCards[i].transform
                    .DOScale(targetScale, 0.3f)
                    .SetEase(Ease.OutQuad);
                currentCards[i].SetAlphaSmooth(targetAlpha, 0.25f);
            }

            if (isSelected && descriptionTypewriter != null)
            {
                if (ScoreManager.Instance.HighScore >= currentCards[i].Card.scoreRequirement)
                {
                    string description = currentCards[i].Card != null ? currentCards[i].Card.description : "No card description found";
                    descriptionTypewriter.StartTyping(description);
                } 
                else
                {
                    descriptionTypewriter.StartTyping("High score required: " + currentCards[i].Card.scoreRequirement);
                }   
            }

            if(isSelected && abilityNameText != null)
            {
                abilityNameText.text = currentCards[i].Card.abilityName.ToUpper();
            }

            if(isSelected && abilityCostText != null)
            {
                abilityCostText.text = currentCards[i].Card.baseCost.ToString();
            }

            

            var sway = currentCards[i].GetComponent<CardParallaxSway>();
            if (sway != null)
                sway.SetActive(isSelected);
                
        }

        // Center selected card in view
        //float totalWidth = (currentCards.Count - 1) * cardSpacing;
        //float totalWidth = (currentCards.Count - 1) * cardSpacing;
        //cardSpacing = width + spacing;
        float totalWidth = (currentCards.Count - 1) * cardSpacing;
        float offset = (-selectedIndex * cardSpacing) + 0.5f * cardSpacing;
        targetPosition = new Vector2(offset, 0);
        Debug.DrawLine(Vector3.zero, Vector3.up * 5, Color.red);

    }

    private void SelectAbility(AbilityData card)
    {
        if (card == null)
        {
            Debug.LogError("❌ No ability card selected!");
            return;
        }

        AudioSettingsManager.PlaySelectSound();
        if (backgroundDimmer != null)
            backgroundDimmer.FadeOut();

        AbilityCardUI cardUI = currentCards[selectedIndex];
        if (cardUI == null)
        {
            Debug.LogError("❌ No card UI found for selected index: " + selectedIndex);
            return;
        }

        Debug.Log("▶ Selecting ability: " + card.abilityName);
        cardUI.PlaySelectAnimation(() => FinishSelection(card));
    }

    private void FinishSelection(AbilityData card)
    {
        if (card == null)
        {
            Debug.LogWarning("⚠️ Selected ability reference lost.");
            return;
        }

        // 🟣 Return-to-menu handling
        if (card.abilityType == AbilityType.ReturnToMenu)
        {
            MenuManager.Instance.RequestMenuTransition(returnCardState);
            return;
        }

        Debug.Log($"✅ Selected ability: {card.abilityName}");

        // Store globally
        AbilitySelection.SelectedAbility = card.abilityType;

        
        GameSceneLoader.PendingConfig = new GameSceneConfig(
            GameMode.StandardRun,
            0,
            null);

        

        startOptions.Open(GameMode.StandardRun, SceneNames.ArrowGameScene);
        lockInput = true;
    }
}
