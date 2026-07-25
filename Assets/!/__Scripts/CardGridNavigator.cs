using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CardGridNavigator : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private InputActionType openKey = InputActionType.Interact;
    [SerializeField] private RectTransform rect;

    [Header("Slide Animation")]
    [SerializeField] private float hiddenX = -300f;
    [SerializeField] private float hiddenY = -300f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float shownX = 0f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutBack;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    


    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<GameObject> cards = new();
    [SerializeField] private DescriptionPanelController descriptionPanel;
    [SerializeField] private CardTitleTooltip titleTooltip;
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private RectTransform layoutRoot;

    [Header("Key Prompts")]
    [SerializeField] private List<PromptGameObject> promptObjects;


    [SerializeField] private int columns = 4;


    [Header("Scroll")]
    [SerializeField] private float bottomSafePadding = 180f;

    private Tween _scrollTween;

    private int _selectedIndex;
    private bool _isOpen = false;
    private Dictionary<UpgradeBase, UpgradeCardUI> _upgradeCards = new();
    private readonly Dictionary<KeyPromptType, GameObject> keyPromptLookup = new();



    private void Awake()
    {
        BuildLookups();
        Close();
    }

    private void OnEnable()
    {
        _selectedIndex = 0;
        UpgradeCardManager.OnCardPurchased += HandleCardPurchased;
    }

    private void OnDisable()
    {
        UpgradeCardManager.OnCardPurchased -= HandleCardPurchased;
    }

    private void BuildLookups()
    {
        keyPromptLookup.Clear();

        foreach (var promptObj in promptObjects)
        {
            if (promptObj.obj == null)
                continue;

            keyPromptLookup[promptObj.keyPrompt] = promptObj.obj;
        }
    }

    private void SetAllPromptsActive(bool active)
    {
        foreach (var pair in keyPromptLookup)
        {
            pair.Value.SetActive(active);
        }
    }

    private void HandleCardPurchased(UpgradeOption upgrade)
    {
        if(_upgradeCards.ContainsKey(upgrade.Base))
        {
            UpgradeCardUI cardUI;
            _upgradeCards.TryGetValue(upgrade.Base, out cardUI);

            if(cardUI != null)
            {
                int stackAmount;
                UpgradeCardManager.Instance.AllChosenCards.TryGetValue(upgrade.Base, out stackAmount);

                if(stackAmount > 1)
                    cardUI.SetStackBadge(stackAmount);
            }
        }
        else
        {
            GameObject card = Instantiate(cardPrefab, content);
            UpgradeCardUI cardUI = card.GetComponent<UpgradeCardUI>();

            if(!cardUI)
            {
                Destroy(card);
                return;
            }

            cardUI.Setup(upgrade);
            cards.Add(card);
            _upgradeCards.Add(upgrade.Base, cardUI);
        }
    }

    private void Update()
    {
        if(InputBindingManager.Instance.GetKeyDown(openKey) && !_isOpen && InputFocusManager.CurrentOwner == null)
        {
            Open();
            return;
        }

        else if(InputBindingManager.Instance.GetKeyDown(openKey) && _isOpen)
        {
            Close();
            return;
        }

        if(_isOpen == false)
            return;

        if(!InputFocusManager.HasFocus(this))
            return;

        if (cards.Count == 0)
            return;

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            if(Move(-1)) 
                panelNudge.NudgeLeft();
        }
            

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            if(Move(1))
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
        //else if(!InputBindingManager.Instance.GetKeyUp(InputActionType.Confirm))
        //{
        //    OpenCardDetails(false);
        //}
    }

    public void Open()
    {
        InputFocusManager.Claim(this);

        mainCanvasGroup.DOKill();
        mainCanvasGroup.DOFade(1,fadeInDuration);

        rect.DOKill();

        rect.anchoredPosition = new Vector2(hiddenX, hiddenY);

        rect.DOAnchorPosY(shownY, duration).SetEase(ease);
        rect.DOAnchorPosX(shownX, duration).SetEase(ease);

        _isOpen = true;

        _selectedIndex = 0;

        SetAllPromptsActive(true);

        if(cards.Count == 0) return;

        RebuildCanvas();

        ScrollTo(cards[_selectedIndex].GetComponent<RectTransform>());
        RefreshSelection();
    }

    private void RebuildCanvas()
    {
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

        Canvas.ForceUpdateCanvases();
    }


    void Close()
    {
        InputFocusManager.Release(this);

        mainCanvasGroup.DOKill();
        mainCanvasGroup.DOFade(0,fadeOutDuration);

        rect.DOKill();

        rect.DOAnchorPosY(hiddenY, duration)
            .SetEase(Ease.InBack);

        rect.DOAnchorPosX(hiddenX, duration)
            .SetEase(Ease.InBack);

        _isOpen = false;
        
        SetAllPromptsActive(false);
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
            cardUI.SetHighlighted(i == _selectedIndex, false,true);

            if(i == _selectedIndex)
            {
                selectedCardUI = cardUI;

                descriptionPanel.Show($"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${cardUI.Option.Base.Cost.ToString("N0")}</color>] " +
                                        cardUI.GetDescription());
            }
        }

        titleTooltip.Hide();

        ScrollTo(selectedCardUI.RectTransform,
        () =>
        {
            Debug.Log($"Trying to scroll to selectedCardUI => {selectedCardUI}. The display name is => {selectedCardUI.Option.DisplayName}");
            if(selectedCardUI)
                titleTooltip.ShowForCard(selectedCardUI.RectTransform, selectedCardUI.Option.DisplayName);
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
            .OnComplete(() => onComplete?.Invoke());
    }

    private void OpenCardDetails(bool state)
    {
        UpgradeCardUI cardUI = cards[_selectedIndex].GetComponent<UpgradeCardUI>();

        switch(state)
        {
            case true:
                detailsText.text = cardUI.Option.Base.GetDetails();
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
}