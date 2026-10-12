using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UpgradeCardUI : MonoBehaviour, IPointerClickHandler
{
    public static event Action<UpgradeCardUI> OnCardClicked;

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image mainIconImage;
    [SerializeField] private RectTransform stackBadgePanel;
    [SerializeField] private Image cardDecorationImage;
    [SerializeField] private Image soldIcon;
    [SerializeField] private CardTitleTooltip soldTitle;
    [SerializeField] private List<Image> subIconImages;
    [SerializeField] private TextMeshProUGUI stackText;
    [SerializeField] private RectTransform coverRect;
    

    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.2f;

    [Header("Main Shake Settings")]
    [SerializeField] private float shakeAngle = 8f;
    [SerializeField] private float shakeReturnAngle = 4f;
    [SerializeField] private float shakeDuration = 0.45f;

    [Header("Idle Wobble Settings")]
    [SerializeField] private float wobbleRotation = 3f;
    [SerializeField] private float wobbleDuration = 1.2f;
    [SerializeField] private float wobbleScaleOffset = 0.02f;
    [SerializeField] private float idleStartDelay = 0.25f;


    [Header("Purchased")]
    [SerializeField] private string soldStatement = "Sold!";
    [SerializeField] private string tintPropertyName = "_Tint";
    [SerializeField] private Color purchasedColor = Color.grey;
    [SerializeField] private bool hideSoldPopupOnUnfocus = true;
    [SerializeField] private float cleanUpDelay = 0.1f;



    [SerializeField] private RectTransform movementRect;
    private Vector3 originalScale;
    private Tween currentTween;
    private Sequence idleSequence;


    private string descriptionText;
    private bool _purchased = false;
    private Material _runtimeCardMaterial;
    private List<Material> _runtimeIconMaterials = new();
    private Material _runtimeBannerMaterial;

    public UpgradeBase Base { get; private set; }
    public RectTransform RectTransform { get; private set; }

    private UpgradeCategoryDatabase _categoryDatabase;
    private int _stack;

    private void OnEnable()
    {
        ShopManager.OnCardPurchased += HandleCardPurchased;
        RefreshUI();
    }

    private void OnDisable()
    {
        ShopManager.OnCardPurchased -= HandleCardPurchased;
    }

    private void HandleCardPurchased(UpgradeBase upgradeBase)
    {
        RefreshUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnCardClicked?.Invoke(this);
    }

    private void Awake()
    {
        originalScale = movementRect.localScale;

        if (backgroundImage != null && backgroundImage.material != null)
        {
            _runtimeCardMaterial = Instantiate(backgroundImage.material);
            backgroundImage.material = _runtimeCardMaterial;
        }

        stackText.text = "";
        outlineImage.DOFade(0,0f).SetUpdate(true);

        RectTransform = GetComponent<RectTransform>();

        stackBadgePanel.gameObject.SetActive(false);
        soldTitle.Hide();
    }

    private void Update()
    {
        
    }


   void OnDestroy()
    {
        idleSequence?.Kill();
        currentTween?.Kill();

        DOTween.Kill(movementRect);
        DOTween.Kill(this);

        if(_runtimeCardMaterial != null)
            Destroy(_runtimeCardMaterial);
        
        foreach(Material mat in _runtimeIconMaterials)
        {
            if(mat != null)
                Destroy(mat);
        }
        

        if(_runtimeBannerMaterial != null)
            Destroy(_runtimeBannerMaterial);
    }

  
    // -------------------------
    public string GetDescription(bool isInventory = false) => Base != null ? Base.GetDescription(isInventory) : "";

    public void Setup(UpgradeBase option, UpgradeCategoryDatabase database, bool useCanAppear = false)
    {
        _categoryDatabase = database;
        _stack = 1;

        Base = option;

        RefreshUI();

        

        if(!Base.CanAppear() && useCanAppear)
            coverRect.gameObject.SetActive(true);
        else
            coverRect.gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        if(Base == null)
            return;

        foreach(Image iconImage in subIconImages)
        {
            if (iconImage != null)
            {
                iconImage.sprite = Base.GetCategoryIcon(_categoryDatabase);

                if(iconImage.material == null)
                    iconImage.material = Base.iconMaterial;
            }

            if (iconImage != null && iconImage.material != null)
            {
                Material mat = Instantiate(iconImage.material);
                _runtimeIconMaterials.Add(mat);
                iconImage.material = mat;
            }
        }

        if (mainIconImage != null && Base.CenterIcon != null)
        {
            mainIconImage.sprite = Base.CenterIcon;
            mainIconImage.SetNativeSize();
            mainIconImage.material = Base.iconMaterial;
        }

        if (mainIconImage != null && mainIconImage.material != null)
        {
            Material mat = Instantiate(mainIconImage.material);
            _runtimeIconMaterials.Add(mat);
            mainIconImage.material = mat;
        }

        if(cardDecorationImage != null && Base.cardDecoration != null)
        {
            cardDecorationImage.sprite = Base.cardDecoration;
            
            if(Base.cardDecorationMaterial != null)
            {
                Material mat = Instantiate(Base.cardDecorationMaterial);
                _runtimeIconMaterials.Add(mat);
                cardDecorationImage.material = mat;
            }
        }
        
        descriptionText = Base.GetDescription();
    }

    public void ShowDrawPercentage(UpgradeCategory upgradeCategory)
    {
        stackBadgePanel.gameObject.SetActive(true);

        float totalWeight = ShopManager.Instance.GetTotalDrawPoolWeight(upgradeCategory);

        if (totalWeight <= 0f)
        {
            stackText.text = "0.0%";
            return;
        }

        float drawChance = Base.GetOfferWeight() / totalWeight;

        stackText.text = $"{drawChance * 100f:F1}%";
    }

    public void AddStack(int amount)
    {
        _stack += amount;

        if(_stack > 1)
        {
            stackBadgePanel.gameObject.SetActive(true);
            stackText.text = $"x{_stack}";
        }
        else
        {
            stackBadgePanel.gameObject.SetActive(false);
        }
    }

    public void SetStackAmount(int amount)
    {
        _stack = amount;

        if(_stack > 1)
        {
            stackBadgePanel.gameObject.SetActive(true);
            stackText.text = $"x{_stack}";
        }
        else
        {
            stackBadgePanel.gameObject.SetActive(false);
        }
    }


    public void SetPurchased()
    {
        _purchased = true;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);
        seq.OnComplete(() =>
        {
            soldTitle.ShowForCard(title: soldStatement);
        });
    }


    // -------------------------
    public void SetHighlighted(bool isHighlighted, bool playShake = true, bool useOutline = false, bool playIdle = true)
    {
        if (movementRect == null || !gameObject.activeInHierarchy) return;

        currentTween?.Kill();
        idleSequence?.Kill();
        outlineImage?.DOKill();


        if(isHighlighted && useOutline)
        {
            outlineImage.DOFade(1,0.05f).SetUpdate(true);
        }
        else
        {
            outlineImage.DOFade(0,0.05f).SetUpdate(true);
        }

        if (isHighlighted)
        {
            if(hideSoldPopupOnUnfocus)
            {
                if (_purchased)
                    soldTitle.ShowForCard(title: soldStatement);
                else
                    soldTitle.Hide();
            }
            
            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);

            seq.Append(movementRect.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack));

            if (playShake)
            {
                seq.Append(movementRect.DOLocalRotate(new Vector3(0, 0, -shakeAngle), shakeDuration * 0.25f).SetEase(Ease.OutQuad));
                seq.Append(movementRect.DOLocalRotate(new Vector3(0, 0, shakeReturnAngle), shakeDuration * 0.25f).SetEase(Ease.OutQuad));
                seq.Append(movementRect.DOLocalRotate(new Vector3(0, 0, -shakeReturnAngle * 0.5f), shakeDuration * 0.25f).SetEase(Ease.InOutQuad));
                seq.Append(movementRect.DOLocalRotate(Vector3.zero, shakeDuration * 0.25f).SetEase(Ease.OutBack));

                seq.AppendInterval(idleStartDelay);
            }

            seq.OnComplete(() =>
            {
                if(!playIdle)
                    return;

                if (movementRect == null)
                    return;

                idleSequence = DOTween.Sequence();

                idleSequence.Append(movementRect.DOScale(originalScale * (hoverScale + wobbleScaleOffset), wobbleDuration / 2f).SetEase(Ease.InOutSine).SetUpdate(true));
                idleSequence.Join(movementRect.DOLocalRotate(new Vector3(0, 0, wobbleRotation), wobbleDuration / 2f).SetEase(Ease.InOutSine).SetUpdate(true));
                idleSequence.Append(movementRect.DOScale(originalScale * (hoverScale - wobbleScaleOffset), wobbleDuration / 2f).SetEase(Ease.InOutSine).SetUpdate(true));
                idleSequence.Join(movementRect.DOLocalRotate(new Vector3(0, 0, -wobbleRotation), wobbleDuration / 2f).SetEase(Ease.InOutSine).SetUpdate(true));
                idleSequence.SetLoops(-1, LoopType.Yoyo);
            });

            currentTween = seq;
        }
        else
        {
            if(hideSoldPopupOnUnfocus)
                soldTitle.Hide();

            movementRect.DOScale(originalScale, scaleDuration).SetEase(Ease.InOutSine).SetUpdate(true);
            movementRect.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutSine).SetUpdate(true);
        }
    }

    // -------------------------
    public void PlaySelectAnimation(Action onComplete = null)
    {
        if (movementRect == null || !gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            return;
        }
            

        currentTween?.Kill();
        idleSequence?.Kill();

        Sequence seq = DOTween.Sequence();

        // Step 1: Quick pop
        seq.Append(movementRect.DOScale(originalScale * (hoverScale + 0.15f), 0.12f).SetEase(Ease.OutQuad).SetUpdate(true));

        // Step 3: Settle back down
        seq.Append(movementRect.DOScale(originalScale * (hoverScale - 0.05f), 0.15f).SetEase(Ease.InOutQuad).SetUpdate(true));
        seq.Join(movementRect.DOLocalRotate(new Vector3(0, 0, UnityEngine.Random.Range(-5f, 5f)), 0.2f).SetEase(Ease.OutSine).SetUpdate(true));


        // Step 5: Completion + restart idle wobble
        seq.OnComplete(() =>
        {
            if (movementRect == null || this == null) return;

            Debug.Log("✅ Completed select animation sequence for card.");

            // Start idle wobble again (separate tween)
            idleSequence = DOTween.Sequence()
                .Append(movementRect.DOScale(originalScale * (hoverScale + 0.02f), 0.6f).SetEase(Ease.InOutSine).SetUpdate(true))
                .Join(movementRect.DOLocalRotate(new Vector3(0, 0, 3f), 0.6f).SetEase(Ease.InOutSine).SetUpdate(true))
                .Append(movementRect.DOScale(originalScale * (hoverScale - 0.02f), 0.6f).SetEase(Ease.InOutSine).SetUpdate(true))
                .Join(movementRect.DOLocalRotate(new Vector3(0, 0, -3f), 0.6f).SetEase(Ease.InOutSine).SetUpdate(true))
                .SetLoops(-1, LoopType.Yoyo);

            onComplete?.Invoke();
        });

        seq.Play();
        currentTween = seq;
    }

    public IEnumerator CleanUp()
    {
        if(_purchased)
        {
            soldTitle.Hide();
            yield return new WaitForSeconds(cleanUpDelay);
        }
    }
}
