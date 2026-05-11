using TMPro;
using UnityEngine;
using DG.Tweening;


public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float baseFontSize = 36f; // set in inspector

    

    public void Initialize(int amount, Vector3 targetPosition, ScorePopupStyle style, float runtimeScale = 1f)
    {
        text.text = $"+{amount}";
        text.color = style.color;

        if (style.font != null) text.font = style.font;
        if (style.fontMaterial != null) text.fontMaterial = style.fontMaterial;

        canvasGroup.alpha = 1f;

        float finalScale = style.scale * runtimeScale;

        // Deterministic sizing (no *=)
        text.fontSize = baseFontSize * finalScale;

        transform.localScale = Vector3.one * finalScale;

        if (style.punchScale)
        {
            transform.DOPunchScale(Vector3.one * style.punchStrength, 0.2f, 6, 0.6f);
        }

        canvasGroup.DOFade(0f, style.flyTime)
            .SetEase(style.fadeEase)
            .SetLink(gameObject);

        transform.DOMove(targetPosition, style.flyTime)
            .SetEase(style.moveEase)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

}
