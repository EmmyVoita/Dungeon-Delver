using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

[RequireComponent(typeof(RectTransform))]
public class AbilityCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;   
     public TextMeshProUGUI nameText;
    public Image lockImage;
    public string descriptionText;
    public Image highlightFrame;

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

    [Header("Background Scroll Settings")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float scrollSpeed = 0.2f;

    private Material backgroundMatInstance;
    private Vector2 scrollOffset;
    private Vector2 scrollDirection;
    private float uniqueScrollOffset;

    private RectTransform rect;
    private Vector3 originalScale;
    private Tween currentTween;
    private Tween idleWobbleTween;

    public AbilityCard Card { get; private set; }

    // -------------------------
    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;

        // Create unique material instance if applicable
        if (backgroundImage != null && backgroundImage.material != null)
        {
            backgroundMatInstance = Instantiate(backgroundImage.material);
            //backgroundImage.material = backgroundMatInstance;

            scrollDirection = UnityEngine.Random.insideUnitCircle.normalized * 20f;
            backgroundMatInstance.SetVector("_MainScrollDirection", scrollDirection);
        }
    }

    public void SetAlpha(float a)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
        group.alpha = a;
    }

    public void SetAlphaSmooth(float a, float duration)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
        group.DOFade(a, duration);
    }

    // -------------------------
    void OnDestroy()
    {
        // Ensure DOTween doesn't keep references to destroyed objects
        DOTween.Kill(rect);
        DOTween.Kill(this);
        if (backgroundMatInstance != null)
            Destroy(backgroundMatInstance);
    }

    // -------------------------
    void Update()
    {
        if (backgroundMatInstance != null)
        {
            scrollOffset.x = (Time.time * scrollSpeed + uniqueScrollOffset) % 1f;
            backgroundMatInstance.SetTextureOffset("_MainTex", scrollOffset);
        }
    }

    // -------------------------
    public string GetDescription() => descriptionText != null ? descriptionText : "No Description";

    public void Setup(AbilityCard card)
    {
        Card = card;


        if (lockImage != null) 
            lockImage.gameObject.SetActive(!(ScoreManager.Instance.HighScore >= card.scoreRequirement));

        if (iconImage != null)
            iconImage.sprite = card.icon;

        if (nameText != null)
            nameText.text = card.abilityName;

        if (descriptionText != null)
            descriptionText = card.description;

        //backgroundImage.material = card.cardMaterial;
        if(card.mainImage != null)
        backgroundImage.sprite = card.mainImage;

        SetHighlighted(false);
    }

    // -------------------------
    public void SetHighlighted(bool isHighlighted)
    {
        if (rect == null || !gameObject.activeInHierarchy) return;

        currentTween?.Kill();
        idleWobbleTween?.Kill();

        if (isHighlighted)
        {
            Sequence seq = DOTween.Sequence();

            // Slight scale-up
            seq.Append(rect.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack));

            // Custom shake pattern
            seq.Append(rect.DOLocalRotate(new Vector3(0, 0, -shakeAngle), shakeDuration * 0.25f).SetEase(Ease.OutQuad));
            seq.Append(rect.DOLocalRotate(new Vector3(0, 0, shakeReturnAngle), shakeDuration * 0.25f).SetEase(Ease.OutQuad));
            seq.Append(rect.DOLocalRotate(new Vector3(0, 0, -shakeReturnAngle * 0.5f), shakeDuration * 0.25f).SetEase(Ease.InOutQuad));
            seq.Append(rect.DOLocalRotate(Vector3.zero, shakeDuration * 0.25f).SetEase(Ease.OutBack));

            seq.AppendInterval(idleStartDelay);

            seq.OnComplete(() =>
            {
                if (rect == null) return;

                idleWobbleTween = DOTween.Sequence()
                    .Append(rect.DOScale(originalScale * (hoverScale + wobbleScaleOffset), wobbleDuration / 2).SetEase(Ease.InOutSine))
                    .Join(rect.DOLocalRotate(new Vector3(0, 0, wobbleRotation), wobbleDuration / 2).SetEase(Ease.InOutSine))
                    .Append(rect.DOScale(originalScale * (hoverScale - wobbleScaleOffset), wobbleDuration / 2).SetEase(Ease.InOutSine))
                    .Join(rect.DOLocalRotate(new Vector3(0, 0, -wobbleRotation), wobbleDuration / 2).SetEase(Ease.InOutSine))
                    .SetLoops(-1, LoopType.Yoyo);
            });

            currentTween = seq;
        }
        else
        {
            if (rect == null) return;
            rect.DOScale(originalScale, scaleDuration).SetEase(Ease.InOutSine);
            rect.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutSine);
        }
    }

    // -------------------------
    public void PlaySelectAnimation(Action onComplete = null)
    {
        if (rect == null || !gameObject.activeInHierarchy)
            return;

        currentTween?.Kill();
        idleWobbleTween?.Kill();

        Sequence seq = DOTween.Sequence();

        Debug.Log($"Playing select animation for card: {Card?.abilityName ?? "Unknown"}");

        // Step 1: Quick pop
        seq.Append(rect.DOScale(originalScale * (hoverScale + 0.15f), 0.12f).SetEase(Ease.OutQuad));

        // Step 2: Flash highlight
        if (highlightFrame != null)
        {
            highlightFrame.DOFade(1f, 0.1f).From(0f);
            seq.Join(highlightFrame.rectTransform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad));
        }

        // Step 3: Settle back down
        seq.Append(rect.DOScale(originalScale * (hoverScale - 0.05f), 0.15f).SetEase(Ease.InOutQuad));
        seq.Join(rect.DOLocalRotate(new Vector3(0, 0, UnityEngine.Random.Range(-5f, 5f)), 0.2f).SetEase(Ease.OutSine));

        // Step 4: Fade out highlight
        if (highlightFrame != null)
        {
            seq.Append(highlightFrame.DOFade(0f, 0.25f).SetEase(Ease.OutSine));
            seq.Join(highlightFrame.rectTransform.DOScale(1f, 0.25f));
        }

        // Step 5: Completion + restart idle wobble
        seq.OnComplete(() =>
        {
            if (rect == null || this == null) return;

            Debug.Log("✅ Completed select animation sequence for card.");

            // Start idle wobble again (separate tween)
            idleWobbleTween = DOTween.Sequence()
                .Append(rect.DOScale(originalScale * (hoverScale + 0.02f), 0.6f).SetEase(Ease.InOutSine))
                .Join(rect.DOLocalRotate(new Vector3(0, 0, 3f), 0.6f).SetEase(Ease.InOutSine))
                .Append(rect.DOScale(originalScale * (hoverScale - 0.02f), 0.6f).SetEase(Ease.InOutSine))
                .Join(rect.DOLocalRotate(new Vector3(0, 0, -3f), 0.6f).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Yoyo);

            onComplete?.Invoke();
        });

        seq.Play();
        currentTween = seq;
    }
}
