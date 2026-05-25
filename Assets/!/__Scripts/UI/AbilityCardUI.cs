using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class AbilityCardUI : MonoBehaviour
{
    [Header("UI References")]
    public AbilityUnlockVisual unlockVisual;
    public Image icon;   
    public Image background;  
    public Color lockColor = Color.grey;

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
    [SerializeField] private float scrollSpeed = 0.2f;

    private Material backgroundMatInstance;
    private Vector2 scrollOffset;
    private Vector2 scrollDirection;
    private float uniqueScrollOffset;

    private RectTransform rect;
    private Vector3 originalScale;
    private Tween currentTween;
    private Tween idleWobbleTween;

    public AbilityData Card { get; private set; }

    // -------------------------
    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;

        // Create unique material instance if applicable
        if (background != null && background.material != null)
        {
            backgroundMatInstance = Instantiate(background.material);
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

    public void Setup(AbilityData card, AbilityCardState cardState)
    {
        Card = card;

        switch(cardState)
        {
            case AbilityCardState.Locked:
                unlockVisual.ShowLocked();
                break;

            case AbilityCardState.NewlyUnlocked:
                unlockVisual.ShowLocked();
                break;

            case AbilityCardState.Unlocked:
                unlockVisual.ShowUnlocked();
                break;
        }

        // Animated
        if(card.iconData.frames.Count > 1 && card.iconData.animated)
        {
           UIImageCyclerPerFrameTime imageAnimator = icon.rectTransform.gameObject.AddComponent<UIImageCyclerPerFrameTime>();
           imageAnimator.Initalize(icon,card.iconData.frames,card.iconData.frameDuration, card.iconData.loop);
        }
        else
        {
            icon.sprite = card.iconData.frames[0];
        }

        // When we set up the card we want to check whether the ability is unlocked and whether it has been presented yet.
        // If it has not been unlocked, then we show the lock symbol. If it has been unlocked and not presented, we also
        // show the locked symbol. If it has been unlocked and presented, when we dont show it.

        /*
        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!(ScoreManager.Instance.HighScore >= card.scoreRequirement));
            icon.color = ScoreManager.Instance.HighScore >= card.scoreRequirement ? Color.white : lockColor;
        }
        */
           
        icon.rectTransform.localScale = card.iconScale * Vector3.one;


        //backgroundImage.material = card.cardMaterial;
        if(card.cardBackground != null)
            background.sprite = card.cardBackground;

        SetHighlighted(false);
    }

    public IEnumerator PlayUnlockAnimation()
    {
        yield return unlockVisual.PlayUnlockAnimation();
        unlockVisual.ShowUnlocked();
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


        // Step 3: Settle back down
        seq.Append(rect.DOScale(originalScale * (hoverScale - 0.05f), 0.15f).SetEase(Ease.InOutQuad));
        seq.Join(rect.DOLocalRotate(new Vector3(0, 0, UnityEngine.Random.Range(-5f, 5f)), 0.2f).SetEase(Ease.OutSine));



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
