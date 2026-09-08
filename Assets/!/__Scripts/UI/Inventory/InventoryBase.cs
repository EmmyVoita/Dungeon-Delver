using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public abstract class InventoryBase : MonoBehaviour
{
    public static event Action<bool> OnInventoryStateChanged;

    [Header("Core")]

    [Header("Input")]
    [SerializeField] private InputActionType openKey = InputActionType.Reroll;

    [Header("Default Open Settings")]
    [SerializeField] private InventoryOpenSettings openSettings;


    [Header("References")]
    [SerializeField] protected CardTitleTooltip titleTooltip;
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private TextMeshProUGUI inventoryTitleText;

    [Header("-Cards")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] protected RectTransform content;
    [SerializeField] protected GameObject cardPrefab;

    [Header("-Database")]
    [SerializeField] protected UpgradeDatabase upgradeDatabase;
    [SerializeField] protected UpgradeCategoryDatabase categoryDatabase;


    [Header("-Description + Details")]
    //[SerializeField] private DescriptionPanelController descriptionPanel;
    [SerializeField] private RectTransform descriptionDetailsParent;
    [SerializeField] private TextTypewriter descriptionTypewriter;
    [SerializeField] private TextMeshProUGUI detailsText;


    [Header("-Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;


    [Header("Card Grid Settings")]
    [SerializeField] private int columns = 4;


    [Header("Scrolling")]
    [SerializeField] private float bottomSafePadding = 180f;


    protected List<UpgradeCardUI> _cards = new();
    protected Tween _scrollTween;
    protected InventoryOpenSettings _currentSettings;
    protected int _selectedIndex;
    protected bool _isOpen = false;
    private KeyPromptController _keyPromptController;
    protected Dictionary<UpgradeBase, UpgradeCardUI> _upgradeCards = new();
    protected readonly Dictionary<KeyPromptType, GameObject> keyPromptLookup = new();
    private RectTransform _toolTipTarget;


    protected virtual void Awake()
    {
        _selectedIndex = 0;
        Clear();
        Close();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        //_keyPromptController.PopPrompts();
        UnSubscribe();
    }

    protected virtual void Subscribe()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        UpgradeCardUI.OnCardClicked += HandleCardClicked;
    }

    protected virtual void UnSubscribe()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        UpgradeCardUI.OnCardClicked -= HandleCardClicked;
    }

    private void HandleCardClicked(UpgradeCardUI cardUI)
    {
        if (!_isOpen)
            return;

        int index = _cards.IndexOf(cardUI);

        if (index < 0)
            return;

        _selectedIndex = index;

        RefreshSelection();
    }


    protected virtual void FillInventory() {}

    protected virtual void HandleStateChanged(GameState previousState, GameState newState) {}


    protected virtual void AddCard(UpgradeBase upgradeBase, int count = 1)
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

        _cards.Add(newCardUI);
        _upgradeCards.Add(upgradeBase, newCardUI);
    }

    protected virtual void Update()
    {
        if(!InputFocusManager.HasFocus(this) || !_isOpen)
                return;

        
        foreach(InputActionType inputAction in _currentSettings.closeKeys)
        {
            if(InputBindingManager.Instance.GetKeyDown(inputAction))
            {
                InputBindingManager.Instance.BlockUntilRelease(inputAction);
                Close();
                return;
            }
        }
       
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            if (Move(-1) && panelNudge)
                panelNudge.NudgeLeft();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            if (Move(1) && panelNudge)
                panelNudge.NudgeRight();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            Move(-columns);
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            Move(columns);
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            OpenCardDetails(true);
        }
    }

    public virtual bool TryOpen(KeyPromptController keyPromptController, InventoryOpenSettings settings = null)
    {
        if (_isOpen)
            return false;

        if(settings == null)
            settings = openSettings;

        _currentSettings = settings;
        _isOpen = true;
        _selectedIndex = 0;
        _keyPromptController = keyPromptController;

        InputFocusManager.Claim(this);

        inventoryTitleText.text = _currentSettings.Title;

        keyPromptController.PushPrompts(keyPrompts);

        Clear();
        FillInventory();

        OnInventoryStateChanged?.Invoke(true);

        StartCoroutine(InitializeOpenLayout());
        return true;
    }

    public virtual void Close()
    {
        if (!_isOpen)
            return;

        _isOpen = false;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
           
        // Hide the card title popup
        titleTooltip.Hide();
       
        // Cleanup bottom keyprompts
        _keyPromptController.PopPrompts();

        // Cleanup typewriters
        descriptionTypewriter.StopTyping(true);

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

        LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionDetailsParent);

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
        _cards.Clear();
    }


    private bool Move(int amount)
    {
        int newIndex = Mathf.Clamp(
            _selectedIndex + amount,
            0,
            _cards.Count - 1
        );

        Debug.Log($"Move called. Amount => {amount}. New Index => {newIndex}. Selected Index => {_selectedIndex}. CardCount => {_cards.Count}");

        if (newIndex == _selectedIndex)
            return false;


        _selectedIndex = newIndex;
        RefreshSelection();
        return true;
    }

    private void RefreshSelection()
    {
        if(_cards.Count == 0)
            return;
            
        OpenCardDetails(false);
         
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);

        UpgradeCardUI selectedCardUI = null;

        for (int i = 0; i < _cards.Count; i++)
        {
            UpgradeCardUI cardUI = _cards[i].GetComponent<UpgradeCardUI>();
            cardUI.SetHighlighted(i == _selectedIndex, false,true, false);

            if(i == _selectedIndex)
            {
                selectedCardUI = cardUI;

                descriptionTypewriter.StartTyping(GetDescription(cardUI));
                RebuildLayout();
            }
        }

        titleTooltip.Hide();
        
        ScrollTo(selectedCardUI.RectTransform,
        () =>
        {
            //Debug.Log($"Trying to scroll to selectedCardUI => {selectedCardUI}. The display name is => {selectedCardUI.Base.displayName}");
            if(selectedCardUI)
                titleTooltip.ShowForCard(selectedCardUI.RectTransform, selectedCardUI.Base.displayName);
        });
    }

    protected virtual string GetDescription(UpgradeCardUI cardUI)
    {
        string prefix = $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${cardUI.Base.Cost.ToString("N0")}</color>] ";

        if(cardUI.Base.CanAppear())
            return prefix + cardUI.GetDescription();
        else
            return prefix + cardUI.Base.GetUnavailableDescription();
    }

    private void RebuildLayout()
    {
        descriptionTypewriter.textComponent.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            descriptionTypewriter.textComponent.rectTransform
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionDetailsParent);
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
        if(_cards.Count == 0)
            return; 
            
        UpgradeCardUI cardUI = _cards[_selectedIndex].GetComponent<UpgradeCardUI>();

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
            descriptionDetailsParent
        );
    }
}