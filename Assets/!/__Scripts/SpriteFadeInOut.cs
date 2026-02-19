using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeInOut : MonoBehaviour
{
    public enum FadeColorMode
    {
        SpriteRendererColor,
        MaterialColor
    }

    

    [Header("Fade Mode")]
    [SerializeField] private FadeColorMode fadeMode = FadeColorMode.SpriteRendererColor;
    [SerializeField] private bool playFullSequenceOnAwake = false;

    [Header("Material Settings")]
    [Tooltip("Used only when Fade Mode = MaterialColor")]
    [SerializeField] private string materialColorProperty = "_MainColor";

    [Header("Fade Timings")]
    public float fadeInTime = 0.3f;
    public float visibleTime = 2f;
    public float fadeOutTime = 0.3f;

    private SpriteRenderer sRend;
    private Material runtimeMaterial;
    private Color originalColor;

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();

        // 🔒 Ensure we don't modify shared materials
        if (fadeMode == FadeColorMode.MaterialColor)
        {
            runtimeMaterial = Instantiate(sRend.material);
            sRend.material = runtimeMaterial;

            if (!runtimeMaterial.HasProperty(materialColorProperty))
            {
                Debug.LogError(
                    $"Material does not have color property '{materialColorProperty}'",
                    this
                );
                enabled = false;
                return;
            }

            originalColor = runtimeMaterial.GetColor(materialColorProperty);
        }
        else
        {
            originalColor = sRend.color;
        }

        // Start fully transparent
        SetAlpha(0f);

        StartCoroutine(FadeIn());
    }


    private IEnumerator FadeIn()
    {
        yield return Fade(0f, originalColor.a, fadeInTime);
        if (playFullSequenceOnAwake)
        {
            yield return StartCoroutine(FadeSequence());
        }
    }


    public IEnumerator FadeSequence()
    {
        // Hold
        yield return new WaitForSeconds(visibleTime);

        // Fade Out
        yield return Fade(originalColor.a, 0f, fadeOutTime);

        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeMode == FadeColorMode.MaterialColor)
        {
            Color c = originalColor;
            c.a = alpha;
            runtimeMaterial.SetColor(materialColorProperty, c);
        }
        else
        {
            Color c = originalColor;
            c.a = alpha;
            sRend.color = c;
        }
    }
}
