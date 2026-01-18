using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class InstructionCanvas : MonoBehaviour
{
    public TextMeshProUGUI instructionText;   // Assign in prefab
    public float displayDuration = 1.5f;      // How long it stays
    public float fadeDuration = 0.4f;         // Fade-in and fade-out time
    public AnimationCurve scaleCurve;         // For pop animation

    private Action onFinished;               // Callback

    public void ShowMessage(string message, float duration, Action onDone = null)
    {
        instructionText.text = message;
        displayDuration = duration;
        onFinished = onDone;
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();

        // Start hidden
        group.alpha = 0f;
        transform.localScale = Vector3.one * 0.7f;

        // Fade-in and scale-up
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = t / fadeDuration;

            group.alpha = Mathf.Lerp(0, 1, p);
            transform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one * 1.1f, scaleCurve.Evaluate(p));

            yield return null;
        }

        transform.localScale = Vector3.one;

        // Wait full display time
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = t / fadeDuration;

            group.alpha = Mathf.Lerp(1, 0, p);
            yield return null;
        }

        onFinished?.Invoke();
        Destroy(gameObject);
    }
}
