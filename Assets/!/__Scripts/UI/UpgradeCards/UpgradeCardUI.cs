using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image mainIconImage;
    [SerializeField] private RectTransform stackBadgePanel;
    [SerializeField] private Image cardDecorationImage;
    [SerializeField] private Image soldIcon;
    [SerializeField] private List<Image> subIconImages;
    [SerializeField] private TextMeshProUGUI stackText;
    

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
    [SerializeField] private string tintPropertyName = "_Tint";
    [SerializeField] private Color purchasedColor = Color.grey;



    [SerializeField] private RectTransform movementRect;
    private Vector3 originalScale;
    private Tween currentTween;
    private Tween idleWobbleTween;


    private string descriptionText;
    private bool _purchased = false;
    private Material _runtimeCardMaterial;
    private List<Material> _runtimeIconMaterials = new();
    private Material _runtimeBannerMaterial;

    public UpgradeOption Option { get; private set; }
    public RectTransform RectTransform { get; private set; }


    private void Awake()
    {
        originalScale = movementRect.localScale;

        if (backgroundImage != null && backgroundImage.material != null)
        {
            _runtimeCardMaterial = Instantiate(backgroundImage.material);
            backgroundImage.material = _runtimeCardMaterial;
        }

        stackText.text = "";
        outlineImage.DOFade(0,0f);

        RectTransform = GetComponent<RectTransform>();

        stackBadgePanel.gameObject.SetActive(false);
        soldIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
   
    }


   void OnDestroy()
    {
        idleWobbleTween?.Kill();
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
    public string GetDescription() => descriptionText != null ? descriptionText : "";

    public void Setup(UpgradeOption option)
    {
        foreach(Image iconImage in subIconImages)
        {
            if (iconImage != null)
            {
                iconImage.sprite = option.Icon;

                if(iconImage.material == null)
                    iconImage.material = option.Base.iconMaterial;
            }

            if (iconImage != null && iconImage.material != null)
            {
                Material mat = Instantiate(iconImage.material);
                _runtimeIconMaterials.Add(mat);
                iconImage.material = mat;
            }
        }

        if (mainIconImage != null && option.CenterIcon != null)
        {
            mainIconImage.sprite = option.CenterIcon;
            mainIconImage.SetNativeSize();
            mainIconImage.material = option.Base.iconMaterial;
        }

        if (mainIconImage != null && mainIconImage.material != null)
        {
            Material mat = Instantiate(mainIconImage.material);
            _runtimeIconMaterials.Add(mat);
            mainIconImage.material = mat;
        }

        if(cardDecorationImage != null && option.CardDecoration != null)
        {
            cardDecorationImage.sprite = option.CardDecoration;
            
            if(option.Base.cardDecorationMaterial != null)
            {
                Material mat = Instantiate(option.Base.cardDecorationMaterial);
                _runtimeIconMaterials.Add(mat);
                cardDecorationImage.material = mat;
            }
        }
        
        descriptionText = option.Description;

        Option = option;
    }

    public void SetStackBadge(int stackCount)
    {
        stackBadgePanel.gameObject.SetActive(stackCount > 0 ? true : false);
        stackText.text = $"x{stackCount}";
    }

    public void SetPurchased()
    {
        _purchased = true;
        soldIcon.gameObject.SetActive(true);
        /*
        _runtimeCardMaterial?.SetColor(tintPropertyName, purchasedColor);

        foreach(Material mat in _runtimeIconMaterials)
        {
            mat?.SetColor(tintPropertyName,purchasedColor);
        }
        
        _runtimeBannerMaterial?.SetColor(tintPropertyName,purchasedColor);
        */
    }


    // -------------------------
    public void SetHighlighted(bool isHighlighted, bool playShake = true, bool useOutline = false)
    {
        if (movementRect == null || !gameObject.activeInHierarchy) return;

        currentTween?.Kill();
        idleWobbleTween?.Kill();
        outlineImage?.DOKill();


        if(isHighlighted && useOutline)
        {
            outlineImage.DOFade(1,0.05f);
        }
        else
        {
            outlineImage.DOFade(0,0.05f);
        }

        if (isHighlighted)
        {
            Sequence seq = DOTween.Sequence();

            // Slight scale-up
            seq.Append(movementRect.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack));

            if(playShake)
            {
                // Custom shake pattern
                seq.Append(movementRect.DOLocalRotate(new Vector3(0, 0, -shakeAngle), shakeDuration * 0.25f).SetEase(Ease.OutQuad));
                seq.Append(movementRect.DOLocalRotate(new Vector3(0, 0, shakeReturnAngle), shakeDuration * 0.25f).SetEase(Ease.OutQuad));
                seq.Append(movementRect.DOLocalRotate(new Vector3(0, 0, -shakeReturnAngle * 0.5f), shakeDuration * 0.25f).SetEase(Ease.InOutQuad));
                seq.Append(movementRect.DOLocalRotate(Vector3.zero, shakeDuration * 0.25f).SetEase(Ease.OutBack));

                seq.AppendInterval(idleStartDelay);

                
                seq.OnComplete(() =>
                {
                    if (movementRect == null) return;

                    idleWobbleTween = DOTween.Sequence()
                        .Append(movementRect.DOScale(originalScale * (hoverScale + wobbleScaleOffset), wobbleDuration / 2).SetEase(Ease.InOutSine))
                        .Join(movementRect.DOLocalRotate(new Vector3(0, 0, wobbleRotation), wobbleDuration / 2).SetEase(Ease.InOutSine))
                        .Append(movementRect.DOScale(originalScale * (hoverScale - wobbleScaleOffset), wobbleDuration / 2).SetEase(Ease.InOutSine))
                        .Join(movementRect.DOLocalRotate(new Vector3(0, 0, -wobbleRotation), wobbleDuration / 2).SetEase(Ease.InOutSine))
                        .SetLoops(-1, LoopType.Yoyo);
                });
            }

            currentTween = seq;
        }
        else
        {
            if (movementRect == null) return;
            movementRect.DOScale(originalScale, scaleDuration).SetEase(Ease.InOutSine);
            movementRect.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutSine);
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
        idleWobbleTween?.Kill();

        Sequence seq = DOTween.Sequence();

        // Step 1: Quick pop
        seq.Append(movementRect.DOScale(originalScale * (hoverScale + 0.15f), 0.12f).SetEase(Ease.OutQuad));

        // Step 3: Settle back down
        seq.Append(movementRect.DOScale(originalScale * (hoverScale - 0.05f), 0.15f).SetEase(Ease.InOutQuad));
        seq.Join(movementRect.DOLocalRotate(new Vector3(0, 0, UnityEngine.Random.Range(-5f, 5f)), 0.2f).SetEase(Ease.OutSine));


        // Step 5: Completion + restart idle wobble
        seq.OnComplete(() =>
        {
            if (movementRect == null || this == null) return;

            Debug.Log("✅ Completed select animation sequence for card.");

            // Start idle wobble again (separate tween)
            idleWobbleTween = DOTween.Sequence()
                .Append(movementRect.DOScale(originalScale * (hoverScale + 0.02f), 0.6f).SetEase(Ease.InOutSine))
                .Join(movementRect.DOLocalRotate(new Vector3(0, 0, 3f), 0.6f).SetEase(Ease.InOutSine))
                .Append(movementRect.DOScale(originalScale * (hoverScale - 0.02f), 0.6f).SetEase(Ease.InOutSine))
                .Join(movementRect.DOLocalRotate(new Vector3(0, 0, -3f), 0.6f).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Yoyo);

            onComplete?.Invoke();
        });

        seq.Play();
        currentTween = seq;
    }
}
