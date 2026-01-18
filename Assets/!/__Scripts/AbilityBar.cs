using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class AbilityBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillBarImage;
    [SerializeField] private Image starImage;
    [SerializeField] private ParticleSystem fillEffectParticleSystem;

    [Header("Star Flash Settings")]
    [SerializeField] private Sprite normalStarSprite;
    [SerializeField] private Sprite flashStarSprite;
    [SerializeField] private float flashDuration = 0.15f;

    [Header("Fill Bar Settings")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float trailDelay = 0.1f;

    [Header("Fill Bar Color Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.6f, 1f, 1f);
    [SerializeField] private Color fullColor = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float colorChangeDuration = 0.3f;
    [SerializeField] private Ease colorChangeEase = Ease.OutSine;
    [SerializeField] private float rainbowSpeed = 0.4f;
    [SerializeField] private float rainbowFadeInDuration = 1.5f;

    private float currentFill = 0f;
    private Coroutine flashRoutine;
    private Coroutine rainbowRoutine;
    private Coroutine queueRoutine;
    private bool isFull = false;

    // 🧾 Queue for pending bar changes
    private readonly Queue<float> pendingFills = new Queue<float>();

    void OnEnable() => Player.OnAbilityChargeChanged += EnqueueUpdate;
    void OnDisable() => Player.OnAbilityChargeChanged -= EnqueueUpdate;

    void Start()
    {
        SetFillInstant(0f);

        if (starImage != null && normalStarSprite != null)
            starImage.sprite = normalStarSprite;

        if (fillBarImage != null)
        {
            fillBarImage.color = normalColor;
            fillBarImage.type = Image.Type.Filled;
            fillBarImage.fillMethod = Image.FillMethod.Vertical;
            fillBarImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
    }

    // Instead of immediately updating, enqueue the target fill
    void EnqueueUpdate(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        Debug.Log($"AbilityCharge {Player.Instance.AbilityCharge} / {Player.Instance.MaxAbilityCharge}");
        float targetFill = Player.Instance.AbilityCharge / (float)Player.Instance.MaxAbilityCharge;
        pendingFills.Enqueue(targetFill);

        if (queueRoutine == null)
            queueRoutine = StartCoroutine(ProcessQueue());
    }

    IEnumerator ProcessQueue()
    {
        while (pendingFills.Count > 0)
        {
            float nextFill = pendingFills.Dequeue();
            yield return MoveFill(nextFill);
            CheckFullState(nextFill);
        }

        queueRoutine = null;
    }

    IEnumerator MoveFill(float targetFill)
    {
        currentFill = targetFill;

        if (fillEffectParticleSystem != null)
            fillEffectParticleSystem.Play();

        FlashStar();

        float start = fillBarImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            fillBarImage.fillAmount = Mathf.Lerp(start, targetFill, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        fillBarImage.fillAmount = targetFill;
        yield return new WaitForSeconds(trailDelay);
    }

    void SetFillInstant(float fill)
    {
        currentFill = fill;
        if (fillBarImage != null)
            fillBarImage.fillAmount = fill;
    }

    void FlashStar()
    {
        if (starImage == null || flashStarSprite == null || normalStarSprite == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashStarCoroutine());
    }

    IEnumerator FlashStarCoroutine()
    {
        starImage.sprite = flashStarSprite;
        yield return new WaitForSeconds(flashDuration);
        starImage.sprite = normalStarSprite;
    }

    void CheckFullState(float targetFill)
    {
        bool nowFull = targetFill >= 1f - 0.001f;

        if (nowFull && !isFull)
        {
            isFull = true;
            fillBarImage.DOColor(fullColor, colorChangeDuration).SetEase(colorChangeEase);

            if (rainbowRoutine != null)
                StopCoroutine(rainbowRoutine);
            rainbowRoutine = StartCoroutine(RainbowColorLoop(true));
        }
        else if (!nowFull && isFull)
        {
            isFull = false;

            if (rainbowRoutine != null)
                StopCoroutine(rainbowRoutine);
            fillBarImage.DOColor(normalColor, colorChangeDuration).SetEase(colorChangeEase);
        }
    }

    IEnumerator RainbowColorLoop(bool fadeIn)
    {
        float hue = 0f;
        float blend = 0f;

        if (fadeIn)
        {
            float elapsed = 0f;
            while (elapsed < rainbowFadeInDuration)
            {
                elapsed += Time.deltaTime;
                blend = Mathf.Clamp01(elapsed / rainbowFadeInDuration);

                hue += Time.deltaTime * rainbowSpeed;
                if (hue > 1f) hue -= 1f;

                Color rainbow = Color.HSVToRGB(hue, 0.45f, 1.05f);
                rainbow = Color.Lerp(rainbow, Color.white, 0.15f);
                fillBarImage.color = Color.Lerp(fullColor, rainbow, blend);

                yield return null;
            }
        }

        while (true)
        {
            hue += Time.deltaTime * rainbowSpeed;
            if (hue > 1f) hue -= 1f;

            Color rainbow = Color.HSVToRGB(hue, 0.45f, 1.05f);
            rainbow = Color.Lerp(rainbow, Color.white, 0.15f);
            fillBarImage.color = rainbow;
            yield return null;
        }
    }
}
