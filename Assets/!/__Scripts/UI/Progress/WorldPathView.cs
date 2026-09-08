using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldPathView : MonoBehaviour
{
    [SerializeField] private Image lineImage;

    private RectTransform _rectTransform;
    //private RectTransform _lineImageRect;
    private float _targetLength;
    private float _originalHeight;

    private void Awake()
    {
        _rectTransform = lineImage.transform as RectTransform;
        //_lineImageRect = lineImage.transform as RectTransform;

        if (lineImage == null)
            lineImage = GetComponent<Image>();

        if (_rectTransform == null)
        {
            Debug.LogError(
                $"{nameof(WorldPathView)} requires a RectTransform.",
                this
            );

            return;
        }

        if (lineImage == null)
        {
            Debug.LogError(
                $"{nameof(WorldPathView)} requires a UI Image.",
                this
            );

            return;
        }

        _originalHeight = _rectTransform.sizeDelta.y;

        // Grow from the left side.
        _rectTransform.pivot = new Vector2(0f, 0.5f);
    }



    public void Setup(Vector2 from, Vector2 to)
    {
        if (_rectTransform == null)
            return;

        Vector2 direction = to - from;

        _targetLength = direction.magnitude;

        _rectTransform.anchoredPosition = from;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        _rectTransform.localRotation =Quaternion.Euler(0f, 0f, angle);

        SetLength(0f);
    }

    public IEnumerator Build(float duration)
    {
        if (_rectTransform == null)
            yield break;

        if (duration <= 0f)
        {
            SetLength(_targetLength);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            SetLength(_targetLength * easedProgress);

            yield return null;
        }

        SetLength(_targetLength);
    }

    private void SetLength(float length)
    {
        Vector2 size = _rectTransform.sizeDelta;

        size.x = Mathf.Max(0f, length);
        size.y = _originalHeight;

        _rectTransform.sizeDelta = size;
    }
}