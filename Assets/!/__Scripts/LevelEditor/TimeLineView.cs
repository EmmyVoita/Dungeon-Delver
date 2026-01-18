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
    [SerializeField] private float timelineEndTime = 30f; // seconds

    // ─────────────────────────────────────────────
    // PROPERTIES
    // ─────────────────────────────────────────────

    public float PixelsPerSecond => pixelsPerSecond;
    public float TimelineEndTime => timelineEndTime;

    // ─────────────────────────────────────────────
    // TIME ⇄ PIXEL CONVERSION
    // ─────────────────────────────────────────────

    public float TimeToPixels(float time)
    {
        return time * pixelsPerSecond;
    }

    public float PixelsToTime(float pixels)
    {
        return pixels / pixelsPerSecond;
    }

    // ─────────────────────────────────────────────
    // TIMELINE SIZE
    // ─────────────────────────────────────────────

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

        //Debug.Log($"🕒 Timeline resized: {content.sizeDelta.x} pixels for {timelineEndTime} seconds.")  ;
    }

    // ─────────────────────────────────────────────
    // SCROLLING (TIME-BASED)
    // ─────────────────────────────────────────────

    public void SyncScrollToTime(float time)
    {
        float x = TimeToPixels(time);
        float width = Mathf.Max(1f, content.sizeDelta.x);
        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(x / width);
    }

    public float GetVisibleTime()
    {
        return PixelsToTime(
            scrollRect.horizontalNormalizedPosition * content.sizeDelta.x
        );
    }

    // ─────────────────────────────────────────────
    // ZOOM (CORRECT IMPLEMENTATION)
    // ─────────────────────────────────────────────

    public void ApplyZoom(float scrollDelta)
    {
        if (scrollDelta == 0f)
            return;

        // Preserve the time currently visible
        float visibleTime = GetVisibleTime();

        // Adjust zoom directly in PPS space
        if (scrollDelta > 0)
            pixelsPerSecond *= zoomSpeed;
        else
            pixelsPerSecond /= zoomSpeed;

        pixelsPerSecond = Mathf.Clamp(
            pixelsPerSecond,
            minPixelsPerSecond,
            maxPixelsPerSecond
        );

        //Debug.Log($"🕒 Zoom adjusted: {pixelsPerSecond} PPS. ScrollDelta {scrollDelta}");

        ResizeContent();

        // Restore view so time alignment stays correct
        SyncScrollToTime(visibleTime);
    }

    
}
