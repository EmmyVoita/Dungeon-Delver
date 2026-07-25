using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct PopupLocationData
{
    public Vector2 positionOffset;
    public float angleZ;
}

public class TextPopupObject : MonoBehaviour
{

    [Header("Text Settings")]
    [SerializeField] private string textPrefix = "$";


    [Header("Visual Settings")]
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private float floatDistance = 30f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float scaleUp = 1.2f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private Ease fadeEase = Ease.InSine;

    [Header("Timing")]
    [SerializeField] private float textPopupInterval = 0.2f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Layout")]
    [SerializeField] private float spacing = 6f;

    [Header("HDR Settings")]
    [Tooltip("Multiplier applied to gradient color for HDR bloom intensity.")]
    [Min(1f)]
    [SerializeField] private float hdrIntensity = 2f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI symbolText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform imageRect;
    [SerializeField] private RectTransform parentRect;
    

    [Header("BackgroundImage")]
    [SerializeField] private Image popupImage;

    [SerializeField] private float imageFadeInDelay = 0f;
    [SerializeField] private float imageFadeValue = 0.5f;
    [SerializeField] private float imageFadeInDuration = 0.4f;

    [SerializeField] private float imageFadeOutDelay = 0f;
    [SerializeField] private float imageFadeOutDuration = 0.5f;

    [SerializeField] private Ease imageScaleCurve = Ease.InOutSine;
    [SerializeField] private Ease imageFadeInCurve = Ease.InOutSine;
    [SerializeField] private Ease imageFadeOutCurve = Ease.InOutSine;

    [SerializeField] private float imageStartScale = 0f;
    [SerializeField] private float imageEndScale = 1.2f;
    [SerializeField] private float minStartRotation = -20f;
    [SerializeField] private float maxStartRotation = 20f;

    [Header("Positioning")]
    [SerializeField] private List<PopupLocationData> popupLocationData;

    private Sequence _sequence;
    private static int _nextOffsetIndex;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(int amount, string prefix = null, string suffix = null)
    {
        if(parentRect == null)
            return;

        if (amount == 0)
        {
            Destroy(gameObject);
            return;
        }

        if (amountText == null || symbolText == null)
        {
            Debug.LogWarning("TextPopupObject is missing text references.", this);
            Destroy(gameObject);
            return;
        }

        symbolText.text = amount > 0 ? "+" : "-";

        string amountString = $"{textPrefix}{Mathf.Abs(amount)}";

        if (!string.IsNullOrEmpty(prefix))
            amountString = prefix + amountString;

        if (!string.IsNullOrEmpty(suffix))
            amountString += suffix;

        amountText.text = amountString;

        if(popupLocationData == null || popupLocationData.Count == 0)
        {
            Debug.LogWarning("Inside TextPopupObject, popupLocationData list is either null or size of 0");
        }
        else
        {
            _nextOffsetIndex %= popupLocationData.Count;

            PopupLocationData popupData = popupLocationData[_nextOffsetIndex];//popupLocationData.GetRandom();
            
            _nextOffsetIndex = (_nextOffsetIndex + 1) % popupLocationData.Count;
           
            parentRect.anchoredPosition += popupData.positionOffset;

            parentRect.eulerAngles = new Vector3(parentRect.rotation.x,parentRect.rotation.y, popupData.angleZ); 
        }

        

        ApplyColor();
        PositionText();
        Animate();
    }


    private void ApplyColor()
    {
        float colorT = Random.Range(0.3f, 0.6f);
        Color color = colorGradient.Evaluate(colorT) * hdrIntensity;

        symbolText.color = color;
        amountText.color = color;
    }

    private void PositionText()
    {
        symbolText.ForceMeshUpdate();
        amountText.ForceMeshUpdate();

        RectTransform symbolRect = symbolText.rectTransform;
        RectTransform amountRect = amountText.rectTransform;

        float symbolWidth = symbolText.preferredWidth;
        float amountWidth = amountText.preferredWidth;
        float totalWidth = symbolWidth + spacing + amountWidth;

        // The final combined popup is centered around X = 0.
        float leftEdge = -totalWidth * 0.5f;

        float symbolRightEdge = leftEdge + symbolWidth;
        float amountLeftEdge = symbolRightEdge + spacing;

        symbolRect.anchorMin = new Vector2(0.5f, 0.5f);
        symbolRect.anchorMax = new Vector2(0.5f, 0.5f);

        amountRect.anchorMin = new Vector2(0.5f, 0.5f);
        amountRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Symbol grows leftward while keeping its right edge fixed.
        symbolRect.pivot = new Vector2(1f, 0.5f);

        // Amount grows rightward while keeping its left edge fixed.
        amountRect.pivot = new Vector2(0f, 0.5f);

        symbolRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            symbolWidth
        );

        amountRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            amountWidth
        );

        symbolRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            symbolText.preferredHeight
        );

        amountRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            amountText.preferredHeight
        );

        // Because of their pivots, these positions represent the adjacent edges.
        symbolRect.anchoredPosition = new Vector2(symbolRightEdge, 0f);
        amountRect.anchoredPosition = new Vector2(amountLeftEdge, 0f);
    }

    private void Animate()
    {
        _sequence?.Kill();

        RectTransform symbolRect = symbolText.rectTransform;
        RectTransform amountRect = amountText.rectTransform;

        Vector2 symbolStart = symbolRect.anchoredPosition;
        Vector2 amountStart = amountRect.anchoredPosition;

        symbolRect.localScale = Vector3.zero;
        amountRect.localScale = Vector3.zero;

        canvasGroup.alpha = 1f;

        _sequence = DOTween.Sequence();

        if (popupImage != null)
        {
            float randomRotation = Random.Range(
                minStartRotation,
                maxStartRotation
            );

            imageRect.localScale = Vector3.one * imageStartScale;
            imageRect.localRotation = Quaternion.Euler(
                0f,
                0f,
                randomRotation
            );

            Color imageColor = popupImage.color;
            imageColor.a = 0f;
            popupImage.color = imageColor;
        }

        _sequence.Insert(
            0f,
            symbolRect.DOAnchorPosY(
                    symbolStart.y + floatDistance,
                    duration
                )
                .SetEase(moveEase)
        );

        _sequence.Insert(
            0f,
            CreatePopTween(symbolRect)
        );

        _sequence.Insert(
            textPopupInterval,
            amountRect.DOAnchorPosY(
                    amountStart.y + floatDistance,
                    duration
                )
                .SetEase(moveEase)
        );

        _sequence.Insert(
            textPopupInterval,
            CreatePopTween(amountRect)
        );

        float fadeStart =
            textPopupInterval +
            duration +
            holdDuration;

        _sequence.Insert(
            fadeStart,
            canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(fadeEase)
        );

        if (popupImage != null)
        {
            // Fade in
            _sequence.Insert(
                imageFadeInDelay,
                popupImage.DOFade(imageFadeValue, imageFadeInDuration)
                    .SetEase(imageFadeInCurve)
            );

            _sequence.Insert(
                imageFadeInDelay,
                imageRect.DOScale(imageEndScale, imageFadeInDuration)
                    .SetEase(imageScaleCurve)
            );

            // Fade out
            float imageFadeStart =
                imageFadeInDelay +
                imageFadeInDuration +
                holdDuration +
                imageFadeOutDelay;

            _sequence.Insert(
                imageFadeStart,
                popupImage.DOFade(0f, imageFadeOutDuration)
                    .SetEase(imageFadeOutCurve)
            );
        }

        _sequence.OnComplete(() => Destroy(gameObject));
    }

    private Tween CreatePopTween(RectTransform target)
    {
        Sequence pop = DOTween.Sequence();

        pop.Append(
            target.DOScale(scaleUp, duration * 0.2f)
                .SetEase(Ease.OutBack)
        );  

        /*
        pop.Append(
            target.DOScale(Vector3.one, duration * 0.12f)
                .SetEase(Ease.OutSine)
        );
        */

        return pop;
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
}