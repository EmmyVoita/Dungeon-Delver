using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardTitleTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform tooltipRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float showDelay = 0.45f;
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private float fadeOutTime = 0.08f;

    [Header("Motion")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 24f);
    [SerializeField] private float popScale = 1.12f;

    [Header("Audio")]
    [SerializeField] private SoundEffect appearSound;


    [Header("TextPanelSettings")]
    [SerializeField] private float maxWidth = 220f;
    [SerializeField] private float paddingX = 24f;
    [SerializeField] private float paddingY = 16f;

    private Sequence _sequence;

    private void Awake()
    {
        HideInstant();
    }

    public void ShowForCard(RectTransform cardRect, string title)
    {
        _sequence?.Kill();

        titleText.text = title;

        tooltipRoot.position = cardRect.position + (Vector3)offset;
        tooltipRoot.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        _sequence = DOTween.Sequence();
        _sequence.AppendInterval(showDelay);
        _sequence.AppendCallback(() =>
        {
            AudioHelpers.PlaySoundEffect(appearSound, Camera.main.transform.position);
        });
        _sequence.Append(canvasGroup.DOFade(1f, fadeInTime));
        _sequence.Join(tooltipRoot.DOScale(popScale, fadeInTime).SetEase(Ease.OutBack));
        _sequence.Append(tooltipRoot.DOScale(1f, 0.08f));
        

        

    
        SetTitle(title);
    }

    public void SetTitle(string title)
    {
        titleText.text = title;
        //titleText.enableWordWrapping = true;

        Vector2 preferred = titleText.GetPreferredValues(title, maxWidth, 0f);

        float finalTextWidth = Mathf.Min(preferred.x, maxWidth);
        float finalTextHeight = preferred.y;

        titleText.rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            finalTextWidth
        );

        titleText.rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            finalTextHeight
        );

        tooltipRoot.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            finalTextWidth + paddingX
        );

        tooltipRoot.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            finalTextHeight + paddingY
        );

        titleText.ForceMeshUpdate();
    }

    public void Hide()
    {
        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(canvasGroup.DOFade(0f, fadeOutTime));
        _sequence.Join(tooltipRoot.DOScale(0.85f, fadeOutTime));
    }

    public void HideInstant()
    {
        _sequence?.Kill();

        canvasGroup.alpha = 0f;
        tooltipRoot.localScale = Vector3.zero;
    }
}