using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlayerRingTimer : MonoBehaviour
{
    [Header("References")]
    public Image ringImage;

    [Header("Timer Settings")]
    public float fadeOutDuration = 0.3f;
    public bool disableOnEnd = true;

    [Header("Tick Settings")]
    public AudioClip tickSound;
    public AudioClip endSound;
    public float tickInterval = 1f;            // time between ticks
    public float minPitch = 0.8f;             // start pitch
    public float maxPitch = 1.4f;             // pitch when time is almost out

    private float duration;
    private float timeLeft;
    private float nextTickTime;

    private bool active = false;
    private Color baseColor;

    private Action onTimerEnd;

    void Awake()
    {
        if (ringImage != null)
            baseColor = ringImage.color;

        ringImage.enabled = false;
    }

    void Update()
    {
        if (!active) return;

        timeLeft -= Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(timeLeft / duration);
        ringImage.fillAmount = t;

        // 🔔 Handle ticking
        HandleTicking(t);

        if (timeLeft <= 0f)
        {
            onTimerEnd?.Invoke();
            Hide();

            onTimerEnd = null;  
        }
    }

    private void HandleTicking(float normalizedTimeRemaining)
    {
        // time to tick
        if (Time.unscaledTime >= nextTickTime)
        {
            nextTickTime += tickInterval;

            if (tickSound != null)
            {
                float pitch = Mathf.Lerp(minPitch, maxPitch, 1f - normalizedTimeRemaining);
                AudioHelpers.PlayClipWithVariation(
                    tickSound,
                    AudioChannel.UI,
                    Camera.main.transform.position,
                    pitch,
                    0f,
                    1f
                );
            }
        }
    }

    public void Show(float seconds, Action onTimerEnd = null, Vector3? scale = null)
    {
        ringImage.enabled = true;
        duration = seconds;
        timeLeft = seconds;
        active = true;

        ringImage.fillAmount = 1f;
        ringImage.color = baseColor;
        ringImage.enabled = true;

        nextTickTime = Time.unscaledTime + tickInterval; // reset tick timer

        this.onTimerEnd = onTimerEnd;

        // 🔹 Apply scale if provided
        if (scale.HasValue)
            ringImage.transform.localScale = scale.Value;
        else
            ringImage.transform.localScale = Vector3.one; // default scale
    }


    public void Hide()
    {
        active = false;

        if (disableOnEnd)
            StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = ringImage.color;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float fade = 1f - (elapsed / fadeOutDuration);
            ringImage.color = new Color(startColor.r, startColor.g, startColor.b, baseColor.a * fade);
            yield return null;
        }

        ringImage.enabled = false;
        ringImage.color = baseColor; // reset
    }
}
