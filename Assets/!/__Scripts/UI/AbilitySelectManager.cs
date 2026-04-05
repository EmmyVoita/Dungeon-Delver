using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class AbilitySelectManager : BaseMenu
{
    public static AbilitySelectManager Instance { get; private set; }
    public static Action<SelectOption> OnAbilitySelected;
    public static Action OnMenuAbilitySelected;

    [Header("UI")]
    public GameObject titleUI;
    public GameObject highscoreUI;
    public GameObject cardUIPrefab;
    public RectTransform cardContainer; // 🌀 Scrollable parent
    public Transform cardParent;        // Optional fallback (usually same as cardContainer)
    public Transform descriptionBox;
    public BackgroundDimmerController backgroundDimmer;
    public TextTypewriter descriptionTypewriter;

    [Header("Carousel Settings")]
    public float slideSpeed = 8f;
    public float cardSpacing = 400f; // Auto-calculated if left at 0
    public float unselectedScale = 0.9f;
    public float selectedScale = 1.15f;
    public float unselectedAlpha = 0.6f;

    [Header("Available Abilities")]
    public List<AbilityCard> allAbilities;
    private List<AbilityCardUI> currentCards = new List<AbilityCardUI>();
    private int selectedIndex = 0;

    // For smooth sliding
    private Vector2 targetPosition;

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

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }


    // ----------------------------
    //      SPAWN ABILITY CARDS
    // ----------------------------
    public override void OnOpen()
    {
        base.OnOpen();

        titleUI.SetActive(true);
        highscoreUI.SetActive(true);

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
        selectedIndex = Mathf.Min(1, allAbilities.Count - 1);

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
    }

    public override void OnClose()
    {
        base.OnClose();

        titleUI.SetActive(false);
        highscoreUI.SetActive(false);

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
    }

    // ----------------------------
    //         INPUT
    // ----------------------------
    private void Update()
    {
        if (lockInput || currentCards.Count == 0) return;

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            selectedIndex = (selectedIndex - 1 + currentCards.Count) % currentCards.Count;
            HighlightCard();
            AudioSettingsManager.PlayNavigateSound();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            selectedIndex = (selectedIndex + 1) % currentCards.Count;
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
            MenuManager.Instance.TransitionToMenu(StartMenuWindows.MainMenu, 0.2f);
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
                    descriptionTypewriter.StartTyping(currentCards[i].GetDescription());
                else
                    descriptionTypewriter.StartTyping("High score required: " + currentCards[i].Card.scoreRequirement);
            }

            var sway = currentCards[i].GetComponent<CardParallaxSway>();
            if (sway != null)
                sway.SetActive(isSelected);
                
        }

        // Center selected card in view
        float totalWidth = (currentCards.Count - 1) * cardSpacing;
        float offset = (totalWidth / 2f) - (selectedIndex * cardSpacing);
        targetPosition = new Vector2(offset, 0);

    }

    private void SelectAbility(AbilityCard card)
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

    private void FinishSelection(AbilityCard card)
    {
        if (card == null)
        {
            Debug.LogWarning("⚠️ Selected ability reference lost.");
            return;
        }

        // 🟣 Return-to-menu handling
        if (card.abilityType == AbilityType.ReturnToMenu)
        {
            MenuManager.Instance.TransitionToMenu(StartMenuWindows.MainMenu, 0.2f);
            return;
        }

        Debug.Log($"✅ Selected ability: {card.abilityName}");

        // Store globally
        AbilitySelection.SelectedAbility = card.abilityType;

        
        GameSceneLoader.PendingConfig = new GameSceneConfig(
            GameMode.StandardRun,
            0,
            null,
            JumpDirectionMode.FourDirectional);

        

        OnAbilitySelected?.Invoke(SelectOption.MainGame);
        lockInput = true;
    }
}
