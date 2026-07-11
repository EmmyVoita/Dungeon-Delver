using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Linq;

public class PlayMenuNavigator : BaseMenu
{
    public static PlayMenuNavigator Instance { get; private set; }
    public static Action OnHoverChanged;
    public static Action OnMenuAbilitySelected;


    [Header("References")]
    [SerializeField] private StartOptionsNavigator startOptions;
    [SerializeField] private AbilityUnlockManager unlockManager;


    [Header("Transition Settings")]
    [SerializeField] private MenuState backState = MenuState.Main;
    [SerializeField] private MenuState returnCardState = MenuState.Main;


    [Header("Abilities")]
    [SerializeField] private AbilityDatabase database;


    [Header("UI")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private RectTransform cardContainer; 
    [SerializeField] private Transform cardParent;       
    [SerializeField] private Transform descriptionBox;
    [SerializeField] private TextTypewriter descriptionTypewriter;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityCostText;


    [Header("Carousel Settings")]
    [SerializeField] private float slideSpeed = 8f;
    [SerializeField] private float cardSpacing = 400f;
    [SerializeField] private float unselectedScale = 0.9f;
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float unselectedAlpha = 0.6f;


    [Header("Dynamic")]
    [SerializeField] private List<AbilityCardUI> _currentCards = new List<AbilityCardUI>();
    [SerializeField] private int _selectedIndex = 0;


    private Vector2 _targetPosition;

    public AbilityType ActiveHover => _currentCards[_selectedIndex].Card.abilityType;



    private void OnEnable()
    {
        CarouselArrow.OnCarouselArrowClicked += HanldeCarouselArrowClicked;
        AbilityCardMouseHandler.OnAbilityCardClicked += HandleAbilityCardClicked;
    }

    private void OnDisable()
    {
        CarouselArrow.OnCarouselArrowClicked -= HanldeCarouselArrowClicked;
        AbilityCardMouseHandler.OnAbilityCardClicked -= HandleAbilityCardClicked;
    }

    private void HandleAbilityCardClicked(AbilityData data)
    {
        SelectAbility(data);
    }

    private void HanldeCarouselArrowClicked(CarouselArrow.CarosuelDirection direction)
    {   
        if(direction == CarouselArrow.CarosuelDirection.Left)
            _selectedIndex = (_selectedIndex - 1 + _currentCards.Count) % _currentCards.Count;
        else if (direction == CarouselArrow.CarosuelDirection.Right)
            _selectedIndex = (_selectedIndex + 1) % _currentCards.Count;
    
        OnHoverChanged?.Invoke();
        HighlightCard();
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Smooth slide effect
        if (cardContainer != null)
        {
            cardContainer.anchoredPosition = Vector2.Lerp(
                cardContainer.anchoredPosition,
                _targetPosition,
                Time.unscaledDeltaTime * slideSpeed
            );
        }

        if (lockInput || _currentCards.Count == 0 || !isActive) return;

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            _selectedIndex = (_selectedIndex - 1 + _currentCards.Count) % _currentCards.Count;
            OnHoverChanged?.Invoke();
            HighlightCard();
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            _selectedIndex = (_selectedIndex + 1) % _currentCards.Count;
            OnHoverChanged?.Invoke();
            HighlightCard();
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            if (ScoreManager.Instance.HighScore >= _currentCards[_selectedIndex].Card.scoreRequirement)
            {
                SelectAbility(_currentCards[_selectedIndex].Card);
            }
            else
            {
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            }
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            MenuManager.Instance.RequestMenuTransition(backState);
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
            OnMenuAbilitySelected?.Invoke();
        }
    }


    public override void OnOpen()
    {
        base.OnOpen();

        //if (backgroundDimmer != null)
        //    backgroundDimmer.FadeIn();

        if (descriptionBox != null)
            descriptionBox.gameObject.SetActive(true);

        // Clear old
        foreach (Transform child in cardParent)
        {
            DOTween.Kill(child);
            Destroy(child.gameObject);
        }
        _currentCards.Clear();
        _selectedIndex = Mathf.Min(0, Mathf.Max(database.abilities.Count - 1,0));

        // Spawn all abilities
        foreach (var ability in database.abilities)
        {
            GameObject uiObj = Instantiate(cardUIPrefab, cardParent);
            AbilityCardUI ui = uiObj.GetComponent<AbilityCardUI>();

            bool unlocked =
                unlockManager.IsUnlocked(ability.abilityType);

            bool presented =
                unlockManager.IsPresented(ability.abilityType);

            AbilityCardState state;

            if (!unlocked)
            {
                state = AbilityCardState.Locked;
            }
            else if (!presented)
            {
                state = AbilityCardState.NewlyUnlocked;
            }
            else
            {
                state = AbilityCardState.Unlocked;
            }


            ui.Setup(ability, state);
            _currentCards.Add(ui);
        }

        // Auto-calc spacing
        if (cardSpacing <= 0 && _currentCards.Count > 1)
        {
            RectTransform cardRect = _currentCards[0].GetComponent<RectTransform>();
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


       // if (backgroundDimmer != null)
        //    backgroundDimmer.FadeOut();

        if (descriptionBox != null)
            descriptionBox.gameObject.SetActive(false);

        // Clear cards
        foreach (Transform child in cardParent)
        {
            DOTween.Kill(child);
            Destroy(child.gameObject);
        }
        _currentCards.Clear();

        ScreenDimmerManager.Instance.RemoveDimSource("PlayScreen");
    }


    public IEnumerator ScrollToCard(AbilityType type)
    {
        int returnIndex = _currentCards.FindIndex(c => c.Card.abilityType == type);

        // --- Calculate target anchored position just like HighlightCard() does ---
        float totalWidth = (_currentCards.Count - 1) * cardSpacing;
        float startOffset = (totalWidth / 2f) - (_selectedIndex * cardSpacing);
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
            _targetPosition = Vector2.Lerp(startPos, endPos, t);

            // interpolate selection visually
            int interpIndex = Mathf.RoundToInt(Mathf.Lerp(_selectedIndex, returnIndex, t));
            if (interpIndex != _selectedIndex)
            {
                _selectedIndex = interpIndex;
                HighlightCard();
            }

            yield return null;
        }

        _selectedIndex = returnIndex;
        HighlightCard(forceInstant: true);
    }

    
    public AbilityCardUI GetCard(AbilityType type)
    {
        return _currentCards
            .FirstOrDefault(a => a.Card.abilityType == type);
    }

    private void HighlightCard(bool forceInstant = false)
    {
        for (int i = 0; i < _currentCards.Count; i++)
        {
            bool isSelected = (i == _selectedIndex);
            _currentCards[i].SetHighlighted(isSelected);

            // Scale + fade tween
            float targetScale = isSelected ? selectedScale : unselectedScale;
            float targetAlpha = isSelected ? 1f : unselectedAlpha;

            if (forceInstant)
            {
                _currentCards[i].transform.localScale = Vector3.one * targetScale;
                _currentCards[i].SetAlpha(targetAlpha);
            }
            else
            {
                _currentCards[i].transform
                    .DOScale(targetScale, 0.3f)
                    .SetEase(Ease.OutQuad);
                _currentCards[i].SetAlphaSmooth(targetAlpha, 0.25f);
            }

            if (isSelected && descriptionTypewriter != null)
            {
                if (ScoreManager.Instance.HighScore >= _currentCards[i].Card.scoreRequirement)
                {
                    string description = _currentCards[i].Card != null ? _currentCards[i].Card.description : "No card description found";
                    descriptionTypewriter.StartTyping(description);
                } 
                else
                {
                    descriptionTypewriter.StartTyping("High score required: " + _currentCards[i].Card.scoreRequirement);
                }   
            }

            if(isSelected && abilityNameText != null)
            {
                abilityNameText.text = _currentCards[i].Card.abilityName.ToUpper();
            }

            if(isSelected && abilityCostText != null)
            {
                abilityCostText.text = _currentCards[i].Card.baseCost.ToString();
            }

            

            var sway = _currentCards[i].GetComponent<CardParallaxSway>();
            if (sway != null)
                sway.SetActive(isSelected);
                
        }

        float offset = (-_selectedIndex * cardSpacing) + 0.5f * cardSpacing;
        _targetPosition = new Vector2(offset, 0);
        Debug.DrawLine(Vector3.zero, Vector3.up * 5, Color.red);
    }

    private void SelectAbility(AbilityData card)
    {
        lockInput = true;

        if (card == null)
        {
            Debug.LogError("No ability card selected!");
            return;
        }

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        //if (backgroundDimmer != null)
        //    backgroundDimmer.FadeOut();

        AbilityCardUI cardUI = _currentCards[_selectedIndex];
        if (cardUI == null)
        {
            Debug.LogError("No card UI found for selected index: " + _selectedIndex);
            return;
        }

        Debug.Log("Selecting ability: " + card.abilityName);
        cardUI.PlaySelectAnimation(() => FinishSelection(card));
    }

    private void FinishSelection(AbilityData card)
    {
        if (card == null)
        {
            Debug.LogWarning("Selected ability reference lost.");
            return;
        }

        // 🟣 Return-to-menu handling
        if (card.abilityType == AbilityType.ReturnToMenu)
        {
            MenuManager.Instance.RequestMenuTransition(returnCardState);
            return;
        }

        Debug.Log($"Selected ability: {card.abilityName}");

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
