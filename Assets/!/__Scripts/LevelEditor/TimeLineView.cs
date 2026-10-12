
using UnityEngine;
using UnityEngine.UI;

public class TimelineView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;


    [Header("Scale (Zoom)")]
    [SerializeField] private float pixelsPerSecond = 50f;
    [SerializeField] private float minPixelsPerSecond = 20f;
    [SerializeField] private float maxPixelsPerSecond = 400f;
    [SerializeField] private float zoomSpeed = 1.1f;

    [Header("Timeline Length")]
    [SerializeField] private float timelineEndTime = 30f;

    public float PixelsPerSecond => pixelsPerSecond;
    public float MinPixelsPerSecond => minPixelsPerSecond;
    public float MaxPixelsPerSecond => maxPixelsPerSecond;
    public float TimelineEndTime => timelineEndTime;

    private void Awake()
    {
        AssignZoom(0.5f);
    }

    public float TimeToPixels(float seconds)
    {
        return seconds * pixelsPerSecond;
    }

    public float PixelsToTime(float pixels)
    {
        return pixels / pixelsPerSecond;
    }

    public void SetTimelineEndTime(float seconds)
    {
        timelineEndTime = Mathf.Max(1f, seconds);
        ResizeContent();
    }

    public void EnsureCoversTime(float time, float padding = 2f)
    {
        float desired = time + padding;

        if (desired > timelineEndTime)
        {
            timelineEndTime = desired;
            ResizeContent();
        }
    }

    private void ResizeContent()
    {
        content.sizeDelta = new Vector2(
            timelineEndTime * pixelsPerSecond,
            content.sizeDelta.y
        );
    }

    public void SyncScrollToTime(float time)
    {
        float x = TimeToPixels(time);
        float width = Mathf.Max(1f, content.sizeDelta.x);

        scrollRect.horizontalNormalizedPosition =
            Mathf.Clamp01(x / width);
    }

    public float GetVisibleTime()
    {
        return PixelsToTime(
            scrollRect.horizontalNormalizedPosition * content.sizeDelta.x
        );
    }

    public void ApplyZoom(float scrollDelta)
    {
        if (scrollDelta == 0f)
            return;

        float visibleTime = GetVisibleTime();

        if (scrollDelta > 0)
            pixelsPerSecond *= zoomSpeed;
        else
            pixelsPerSecond /= zoomSpeed;

        pixelsPerSecond = Mathf.Clamp(
            pixelsPerSecond,
            minPixelsPerSecond,
            maxPixelsPerSecond
        );

        ResizeContent();
        SyncScrollToTime(visibleTime);
    }



    public void AssignZoom(float sliderValue)
    {
        float visibleTime = GetVisibleTime();

        pixelsPerSecond = Mathf.Lerp(
            minPixelsPerSecond,
            maxPixelsPerSecond,
            sliderValue
        );

        ResizeContent();
        SyncScrollToTime(visibleTime);
    }
}