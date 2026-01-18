using UnityEngine;
using System.Collections;

public class SimonLight : MonoBehaviour
{
    public SpriteRenderer lightImage;
    public Color normalColor = Color.white;
    public Color glowColor = Color.yellow;
    public Color failColor = Color.red;
    public float glowDuration = 0.4f;

    public ParticleSystem destroyEffect;
    public float destroyExpandScale = 1.3f;   // how big it gets before shrinking
    public float destroyDuration = 0.4f;      // how long effect lasts
    public AudioClip simonDestroySound;
    public AudioClip simonFadeInSound;

    private Coroutine destroyRoutine;


    [Header("Rotation Burst Settings")]
    public float rotationBurstAmount = 90f;  // 🔹 How big the spin is
    public float slowdownSharpness = 2f;     // 🔹 Higher = slows down quicker (smoother end)
    public float rotationDuration = 0.5f;      // 🔹 Total time for the rotation effec  t

    private Vector3 originalPos;

    private Coroutine rotationRoutine;  // 🔹 Tracks currently running GlowRotate

    private void Awake()
    {
        lightImage.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0); // start invisible
        originalPos = transform.localPosition;
    }

    public IEnumerator FadeIn(float duration, int spawnIndex)
    {
        float pitch = 1f + (spawnIndex * 0.1f); // Slight pitch increase per index
        AudioHelpers.PlayMyClipAtPoint(
            simonFadeInSound,
            AudioChannel.SFX,
            Camera.main.transform.position,
            pitch: pitch
        );

        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.6f;   // smaller start
        Vector3 overshootScale = Vector3.one * 1.15f; // slight bounce
        Vector3 finalScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // 🔹 Smooth alpha fade-in
            float alpha = Mathf.Lerp(0, 1, t);
            lightImage.color = new Color(normalColor.r, normalColor.g, normalColor.b, alpha);

            // 🔹 Bounce scale using SmoothStep & overshoot curve
            float scaleT = Mathf.SmoothStep(0f, 1f, t);
            // 👇 Elegant bounce without wobble
            transform.localScale = Vector3.Lerp(startScale, overshootScale, scaleT);
            
            yield return null;
        }

        // 🔹 Smooth settle back to 1
        float settleTime = 0f;
        while (settleTime < 0.15f)
        {
            settleTime += Time.deltaTime;
            float t2 = settleTime / 0.15f;
            transform.localScale = Vector3.Lerp(overshootScale, finalScale, Mathf.SmoothStep(0f, 1f, t2));
            yield return null;
        }

        lightImage.color = normalColor;
        transform.localScale = finalScale; // Ensures perfect reset
    }


    public IEnumerator Glow()
    {
        lightImage.color = glowColor;
        yield return new WaitForSeconds(glowDuration);
        lightImage.color = normalColor;
    }

    public IEnumerator GlowFail()
    {
        lightImage.color = failColor;
        //StartCoroutine(PlayFailShake());
        yield return new WaitForSeconds(glowDuration);
        lightImage.color = normalColor;
    }

    public IEnumerator PlayFailShake()
    {
        Vector3 originalPos = transform.localPosition;

        float duration = 0.3f;
        float timer = 0f;
        float shakeStrength = 0.15f;

        // 🔹 Generate ONE random direction (normalized) for the entire shake
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // 🔹 Move back & forth *along that direction* (not fully random per frame)
            float offset = Mathf.Sin(timer * 40f) * shakeStrength;
            transform.localPosition = originalPos + (Vector3)(randomDir * offset);

            yield return null;
        }

        transform.localPosition = originalPos;
    }



    // ------------------------------------------------
    // ✨ New: Glow + Shake for jump feedback
    // ------------------------------------------------

    public void PlayGlowRotate()
    {
        // 🔹 If rotation already running, stop it first
        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
            rotationRoutine = null;
        }

        // 🔹 Make sure rotation & color are reset before starting
        transform.localRotation = Quaternion.identity;
        lightImage.color = glowColor;

        // 🔹 Start new rotation sequence
        rotationRoutine = StartCoroutine(GlowRotate());
    }

    public IEnumerator GlowRotate()
    {
        lightImage.color = glowColor;

        float timer = 0f;

        float startAngle = 0f;
        float endAngle = rotationBurstAmount; // Peak rotation

        while (timer < rotationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / rotationDuration;

            // Easing to slow down smoothly without reversing
            float easedT = 1f - Mathf.Pow(1f - t, slowdownSharpness);

            float currentRot = Mathf.Lerp(startAngle, endAngle, easedT);
            transform.localRotation = Quaternion.Euler(0, 0, currentRot);

            yield return null;
        }

        // Small hold for visual feel (optional)
        yield return new WaitForSeconds(0.05f);

        // Reset softly (no visible reverse rotation)
        transform.localRotation = Quaternion.identity;
        lightImage.color = normalColor;
    }


    public void PlayDestroyEffect()
    {
        if (destroyRoutine != null)
            StopCoroutine(destroyRoutine);

        destroyRoutine = StartCoroutine(DestroyEffectRoutine());

        AudioHelpers.PlayClipWithVariation(
            simonDestroySound,
            AudioChannel.SFX,
            Camera.main.transform.position,
            basePitch: 1f,
            pitchRange: 0.2f
        );
    }

    private IEnumerator DestroyEffectRoutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 peakScale = originalScale * destroyExpandScale;

        float timer = 0f;

        // 🔹 Phase 1: Expand smoothly
        while (timer < destroyDuration * 0.4f)
        {
            timer += Time.deltaTime;
            float t = timer / (destroyDuration * 0.4f);
            transform.localScale = Vector3.Lerp(originalScale, peakScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // 🔹 Trigger particles at peak scale
        if (destroyEffect != null)
        {
            destroyEffect.transform.position = transform.position;
            destroyEffect.Play();
        }

        // 🔹 Phase 2: Shrink to 0
        timer = 0f;
        while (timer < destroyDuration * 0.6f)
        {
            timer += Time.deltaTime;
            float t = timer / (destroyDuration * 0.6f);
            transform.localScale = Vector3.Lerp(peakScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // 🔹 Fully hide & reset
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;
        lightImage.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0); // invisible
    }
}
