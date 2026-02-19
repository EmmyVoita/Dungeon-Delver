using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class GentlePulseUI : MonoBehaviour
{
    [Header("Scale Pulse")]
    [Tooltip("Max scale at the peak of the pulse (e.g. 1.03)")]
    public float maxScale = 1.03f;

    [Tooltip("How long one full pulse cycle takes (seconds)")]
    public float pulseDuration = 3.0f;

    [Header("Alpha Modulation")]
    [Tooltip("Base alpha when calm")]
    public float baseAlpha = 1.0f;

    [Tooltip("How much alpha oscillates (e.g. 0.05)")]
    public float alphaWobbleAmount = 0.04f;

    [Tooltip("How fast alpha oscillates (relative to pulse)")]
    public float alphaWobbleSpeed = 1.0f;

    private RectTransform rectTransform;
    private Image image;

    private Vector3 startScale;
    private Color startColor;

    private float phaseOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        startScale = rectTransform.localScale;
        startColor = image.color;

        // Desync multiple hearts so they don't breathe together
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Normalized time 0 → 1 → 0
        float t = (Time.time + phaseOffset) / pulseDuration;
        float pulse = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f;

        // ❤️ Scale pulse (1.00 → maxScale → 1.00)
        float scale = Mathf.Lerp(1f, maxScale, pulse);
        rectTransform.localScale = startScale * scale;

        // 🫧 Subtle alpha modulation
        float alphaPulse = Mathf.Sin((Time.time + phaseOffset) * alphaWobbleSpeed) * 0.5f + 0.5f;
        float alpha = baseAlpha + alphaPulse * alphaWobbleAmount;

        Color c = startColor;
        c.a = Mathf.Clamp01(alpha);
        image.color = c;
    }
}
