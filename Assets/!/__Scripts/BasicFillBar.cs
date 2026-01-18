using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

public class BasicFillBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillTransform;
    [SerializeField] private Image trailFillTransform;
    [SerializeField] private Image backgroundImage;   // 🔹 New: backdrop image
    [SerializeField] private Transform childContainer;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float trailMoveDuration = 0.3f;
    [SerializeField] private float trailDelay = 0.4f;

    [Header("Timer Settings")]
    [SerializeField] private bool useSmoothFill = true;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool hideOnEnd = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Idle Float Settings")]
    [SerializeField] private bool enableIdleFloat = true;
    [SerializeField] private float floatAmplitude = 15f;
    [SerializeField] private float floatDuration = 2f;

    [Header("Finish Animation")]
    [SerializeField] private float finishScaleUp = 1.15f;
    [SerializeField] private float finishShakeAngle = 15f;
    [SerializeField] private float finishShakeSpeed = 0.15f;  // 🔹 smaller = faster shakes
    [SerializeField] private int finishShakeLoops = 2;

    [Header("Danger Flash Settings")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f); // light red
    [SerializeField] private int flashCount = 2;




    private float duration;
    private float timeRemaining;
    private bool isActive = false;
    private Action onBarComplete;
    private Coroutine timerRoutine;
    private Tween idleFloatTween;


    private Coroutine smoothFillRoutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void Show(float seconds, Action onComplete = null, Vector2? overridePosition = null, bool disableTimer = false, Color? barColor = null)
    {
        duration = seconds;
        timeRemaining = seconds;
        onBarComplete = onComplete;
        isActive = true;

        if(barColor.HasValue)
        {
            Color c = barColor.Value;
            fillTransform.color = c;
            trailFillTransform.color = c * 0.8f; // slightly darker for trail
        }

        childContainer.gameObject.SetActive(true);

        fillTransform.fillAmount = 1f;
        trailFillTransform.fillAmount = 1f;

        if (overridePosition.HasValue)
        {
            RectTransform rt = childContainer as RectTransform;
            if (rt != null)
                rt.anchoredPosition = overridePosition.Value;
            else
                childContainer.position = overridePosition.Value; // fallback for non-UI objects
        }


        FadeIn();
        StartIdleFloat();

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
        
        if (!disableTimer)
        {
            // Start timer normally
            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = StartCoroutine(TimerRoutine());
        }
    }

    private void FadeIn()
    {
        foreach (var img in new Image[] { fillTransform, trailFillTransform, backgroundImage })
        {
            if (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                img.DOFade(1f, fadeInDuration).SetEase(Ease.OutSine);
            }
        }
    }

    private void FadeOut(Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        foreach (var img in new Image[] { fillTransform, trailFillTransform, backgroundImage })
        {
            if (img != null)
                seq.Join(img.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine));
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }

    private void StartIdleFloat()
    {
        if (!enableIdleFloat || childContainer == null) return;

        idleFloatTween?.Kill();

        idleFloatTween = childContainer.DOLocalMoveY(
            childContainer.localPosition.y + floatAmplitude,
            floatDuration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopIdleFloat()
    {
        idleFloatTween?.Kill();
        idleFloatTween = null;
    }

    private IEnumerator TimerRoutine()
    {
        if (useSmoothFill)
        {
            while (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                float ratio = Mathf.Clamp01(timeRemaining / duration);

                fillTransform.fillAmount = ratio;
                trailFillTransform.fillAmount = Mathf.Lerp(trailFillTransform.fillAmount, ratio, Time.deltaTime * 4f);

                yield return null;
            }
        }
        else
        {
            while (timeRemaining > 0f)
            {
                timeRemaining -= updateInterval;
                float ratio = Mathf.Clamp01(timeRemaining / duration);

                MoveFill(ratio);

                yield return new WaitForSeconds(updateInterval);
            }
        }

        onBarComplete?.Invoke();
        Hide();
    }

    void MoveFill(float ratio)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(fillTransform.DOFillAmount(ratio, moveDuration).SetEase(Ease.InOutSine));
        seq.AppendInterval(trailDelay);
        seq.Append(trailFillTransform.DOFillAmount(ratio, trailMoveDuration).SetEase(Ease.InOutSine));
        seq.Play();
    }

    public void Hide()
    {
        isActive = false;
        StopIdleFloat();

        if (hideOnEnd)
        {
            PlayFinishAnimation(() =>
            {
                // optional final callback
                childContainer.gameObject.SetActive(false);
            });
        }
    }

    public void HideImmediate()
    {
        isActive = false;
        StopIdleFloat();
        childContainer.gameObject.SetActive(false);

        fillTransform.fillAmount = 1f;
        trailFillTransform.fillAmount = 1f;

        foreach (var img in new Image[] { fillTransform, trailFillTransform, backgroundImage })
        {
            if (img != null)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }
    }

    public void HideSmooth(float duration = 0.35f)
    {
        isActive = false;
        StopIdleFloat();

        // Kill timer if still running
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        Sequence seq = DOTween.Sequence();

        // Fade all images out smoothly
        foreach (var img in new Image[] { fillTransform, trailFillTransform, backgroundImage })
        {
            if (img != null)
                seq.Join(img.DOFade(0f, duration).SetEase(Ease.InOutSine));
        }

        // Scale down slightly for a soft disappear
        seq.Join(childContainer.DOScale(0.9f, duration));

        seq.OnComplete(() =>
        {
            childContainer.gameObject.SetActive(false);
        });
    }


    private void PlayFinishAnimation(Action onComplete = null)
    {
        StopIdleFloat();

        if (childContainer == null)
        {
            FadeOut(onComplete);
            return;
        }

        // Reset transforms
        childContainer.localRotation = Quaternion.identity;
        childContainer.localScale = Vector3.one;

        fillTransform.fillAmount = 0f;
        trailFillTransform.fillAmount = 0f;

        // Store original color
        Color originalColor = backgroundImage.color;

        Sequence seq = DOTween.Sequence();

        // ============================================
        // ⭐ PHASE 1: Flash Red + Punch Scale + Shake (Simultaneous)
        // ============================================

        seq.Append(backgroundImage.DOColor(flashColor, flashDuration))
            .Join(childContainer.DOPunchScale(
                new Vector3(0.25f, 0.25f, 0),
                flashDuration * 2f,     // longer punch to match shake
                6,
                0.7f
            ))
            .Join(childContainer.DORotate(
                new Vector3(0, 0, finishShakeAngle),
                finishShakeSpeed * 2f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(finishShakeLoops * 2, LoopType.Yoyo)
        );

        // ============================================
        // ⭐ PHASE 2: Color returns to normal
        // ============================================

        seq.Append(backgroundImage.DOColor(originalColor, flashDuration * 0.75f));

        // ============================================
        // ⭐ PHASE 3: Reset transforms
        // ============================================

        seq.AppendCallback(() =>
        {
            childContainer.localRotation = Quaternion.identity;
            childContainer.localScale = Vector3.one;
        });

        // ============================================
        // ⭐ PHASE 4: Fade out & destroy
        // ============================================

        seq.AppendCallback(() =>
        {
            FadeOut(() =>
            {
                childContainer.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        });
    }

    public void SetFillInstant(float ratio)
    {
        if (smoothFillRoutine != null)
        {
            StopCoroutine(smoothFillRoutine);
            smoothFillRoutine = null;
        }

        fillTransform.fillAmount = ratio;
        trailFillTransform.fillAmount = ratio;
    }

    public void SmoothSetFill(float targetRatio, float speed = 4f)
    {
        // Stop any existing smooth fill
        if (smoothFillRoutine != null)
        {
            StopCoroutine(smoothFillRoutine);
            smoothFillRoutine = null;
        }

        smoothFillRoutine = StartCoroutine(SmoothFillRoutine(targetRatio, speed));
    }

    private IEnumerator SmoothFillRoutine(float target, float speed)
    {
        float startFill = fillTransform.fillAmount;
        float startTrail = trailFillTransform.fillAmount;

        // Lerp based on a time factor rather than chasing the changing value
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float lerpT = Mathf.Clamp01(t);

            float newFill = Mathf.Lerp(startFill, target, lerpT);
            float newTrail = Mathf.Lerp(startTrail, target, lerpT);

            fillTransform.fillAmount = newFill;
            trailFillTransform.fillAmount = newTrail;

            yield return null;
        }

        fillTransform.fillAmount = target;
        trailFillTransform.fillAmount = target;

        smoothFillRoutine = null;
    }




}
