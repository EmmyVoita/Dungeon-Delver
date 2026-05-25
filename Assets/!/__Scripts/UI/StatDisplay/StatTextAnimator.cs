using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StatTextAnimator : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float countDuration = 1.5f;
    [SerializeField] private float lingerAfterCount = 1f;
    [SerializeField] private float fadeInDuration = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip animateTickSound;
    [SerializeField] private float animateTickMinInterval = 0.05f;
    [SerializeField] private float animateTickPitchStart = 1.0f;
    [SerializeField] private float animateTickPitchIncrease = 0.02f;


    public IEnumerator AnimateStatText(
        TextMeshProUGUI targetText,
        StatValue stat,
        string prefix = "",
        string suffix = "",
        Func<bool> shouldSkip = null,
        Action onComplete = null)
    {
        if (targetText == null)
        {
            Debug.LogWarning("AnimateStatText called with null TMP target!");
            yield break;
        }

        switch (stat.type)
        {
            case StatDisplayType.Int:
                yield return StartCoroutine(DisplayTextInt(targetText, stat, prefix, suffix, shouldSkip));
                break;

            case StatDisplayType.Ratio:
                yield return StartCoroutine(DisplayTextRatio(targetText, stat, prefix, suffix, shouldSkip));
                break;

            case StatDisplayType.String:
                targetText.text = BuildTextString(stat.text, prefix, suffix);
                break;

            case StatDisplayType.Percent:
                yield return StartCoroutine(DisplayTextInt(targetText, stat, prefix, "%", shouldSkip));
                break;

            default:
                targetText.text = "Unknown Stat Display Type";
                break;
        }

        targetText.gameObject.SetActive(true);

        if (shouldSkip != null && shouldSkip())
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(lingerAfterCount);

        onComplete?.Invoke();
    }


    // Helper text formatter
    string BuildTextRatio(int current, int total, string prefix, string suffix)
    {
        string currentStr = current.ToString("N0");
        string totalStr = total.ToString("N0");

        return $"{prefix}{currentStr}/{totalStr}{suffix}";
    }

    string BuildTextInt(int current, string prefix, string suffix)
    {
        string currentStr = current.ToString("N0");
        return $"{prefix}{currentStr}{suffix}";
    }

    string BuildTextString(string text, string prefix, string suffix)
    {
        return $"{prefix}{text}{suffix}";
    }

    IEnumerator DisplayTextInt(
        TextMeshProUGUI targetText,
        StatValue stat,
        string prefix,
        string suffix,
        Func<bool> shouldSkip = null)
    {
        int i = 0;

        float lastTickTime = -999f;
        float elapsedTime = 0f;
        float currentPitch = animateTickPitchStart;

        targetText.text = BuildTextInt(0, prefix, suffix);

        while (i < stat.value)
        {
            if (shouldSkip != null && shouldSkip())
                break;

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / countDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            int newValue = Mathf.FloorToInt(Mathf.Lerp(0, stat.value, smooth));

            if (newValue != i)
            {
                i = newValue;
                targetText.text = BuildTextInt(i, prefix, suffix);

                if (animateTickSound != null &&
                    Time.time - lastTickTime >= animateTickMinInterval)
                {
                    SoundEffect soundEffect = AudioLibrary.Instance.Database.tallyBase;
                    soundEffect.pitch = currentPitch;

                    AudioHelpers.PlaySoundEffect(soundEffect, transform.position);

                    currentPitch += animateTickPitchIncrease;
                    lastTickTime = Time.time;
                }
            }

            yield return null;
        }

        targetText.text = BuildTextInt(stat.value, prefix, suffix);
    }

    IEnumerator DisplayTextRatio(
        TextMeshProUGUI targetText,
        StatValue stat,
        string prefix,
        string suffix,
        Func<bool> shouldSkip = null)
    {
        int i = 0;

        float lastTickTime = -999f;
        float elapsedTime = 0f;
        float currentPitch = animateTickPitchStart;

        targetText.text = BuildTextRatio(0, stat.total, prefix, suffix);

        while (i < stat.value)
        {
            if (shouldSkip != null && shouldSkip())
                break;

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / countDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            int newValue = Mathf.FloorToInt(Mathf.Lerp(0, stat.value, smooth));

            if (newValue != i)
            {
                i = newValue;
                targetText.text = BuildTextRatio(i, stat.total, prefix, suffix);

                if (animateTickSound != null &&
                    Time.time - lastTickTime >= animateTickMinInterval)
                {
                    SoundEffect soundEffect = AudioLibrary.Instance.Database.tallyBase;
                    soundEffect.pitch = currentPitch;

                    AudioHelpers.PlaySoundEffect(soundEffect, transform.position);

                    currentPitch += animateTickPitchIncrease;
                    lastTickTime = Time.time;
                }
            }

            yield return null;
        }

        targetText.text = BuildTextRatio(stat.value, stat.total, prefix, suffix);
    }
}