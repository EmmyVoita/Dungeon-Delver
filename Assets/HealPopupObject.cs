using TMPro;
using UnityEngine;
using DG.Tweening;

public class TextPopupObject : MonoBehaviour
{
    [Header("Visual Settings")]
    public Gradient colorGradient;
    public float floatDistance = 1.5f;
    public float duration = 1.2f;
    public float scaleUp = 1.2f;
    public Ease moveEase = Ease.OutQuad;
    public Ease fadeEase = Ease.InSine;

    [Header("HDR Settings")]
    [Tooltip("Multiplier applied to gradient color for HDR bloom intensity.")]
    [Min(1f)] public float hdrIntensity = 2f;  // try 2–5 for soft glow



    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(int amount, string prefix = null, string suffix = null)
    {
        // Assign text and color
        text.text = $"{prefix} +{amount} {suffix}";
        text.color = colorGradient.Evaluate(0f);

        // Reset visual state
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;

        // Pick a random color step from gradient for variety
        float colorT = Random.Range(0.3f, 0.6f);
        Color baseColor = colorGradient.Evaluate(colorT);
        text.color = baseColor * hdrIntensity; // multiply for HDR brightness

        // Motion target
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * floatDistance;

        // Animate float up, fade, and scale
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(endPos, duration).SetEase(moveEase));
        seq.Join(transform.DOScale(scaleUp, duration * 0.5f).SetEase(Ease.OutSine));
        seq.Join(canvasGroup.DOFade(0f, duration * 0.8f).SetEase(fadeEase).SetDelay(duration * 0.2f));

        // Cleanup at end
        seq.OnComplete(() => Destroy(gameObject));
    }
}
