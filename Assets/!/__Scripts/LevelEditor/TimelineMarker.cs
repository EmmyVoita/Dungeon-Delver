using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelineMarker : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Color originalColor;

    private float offsetX;
    private float lastDragSnappedTime;

    private Image fillImage;
    private Image outlineImage;
    private bool isEditing = false;


    private RectTransform rect;
    private ArrowEventData evt;
    private LevelTimelineUI timeline;
    private Canvas canvas;

    private bool isDraggingGroup = false;

    public RectTransform Rect => rect;
    public ArrowEventData Event => evt;

    private Dictionary<TimelineMarker, float> dragStartTimes;


    // --------------------------------------------------------------
    // Initialization
    // --------------------------------------------------------------
    public void Initialize(ArrowEventData evt, LevelTimelineUI timeline)
    {
        this.evt = evt;
        this.timeline = timeline;

        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();


        Marker markerComp = GetComponent<Marker>();
        if (markerComp != null)
        {
            fillImage = markerComp.fillImage;
            outlineImage = markerComp.outlineImage;

            originalColor = fillImage.color;

            var c = outlineImage.color;
            c.a = 0f;
            outlineImage.color = c;
        }
        else
        {
            Debug.LogError("TimelineMarker: No Marker component found on marker prefab!");
        }
    }


    // --------------------------------------------------------------
    // Marker Click
    // --------------------------------------------------------------
    public void OnPointerClick(PointerEventData e)
    {
        var controller = timeline.MarkerController;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (e.button == PointerEventData.InputButton.Left)
        {
            controller.SelectMarker(
                this,
                shift ? SelectionMode.Toggle : SelectionMode.Replace
            );
        }
        else if (e.button == PointerEventData.InputButton.Right)
        {
            controller.SelectMarker(this, SelectionMode.Replace);
            timeline.ShowContextMenu(this, e.position);
        }
    }




    // --------------------------------------------------------------
    // Begin Drag
    // --------------------------------------------------------------
    public void OnBeginDrag(PointerEventData e)
    {
        var controller = timeline.MarkerController;

        timeline.scrollRect.enabled = false;
        float mouseLocalX = ScreenToLocalX(e.position);

        // If this marker isn't selected, treat as single drag
        if (!timeline.MarkerController.IsSelected(this))
        {
            controller.SelectMarker(
                this,
                SelectionMode.Replace
            );
        }

        offsetX = rect.anchoredPosition.x - mouseLocalX;
        isDraggingGroup = true;

        // Capture starting times for all selected markers
        dragStartTimes = new Dictionary<TimelineMarker, float>();
        foreach (var m in timeline.MarkerController.SelectedMarkers)
        {
            dragStartTimes[m] = m.Event.beatTime;
        }
    }



    // --------------------------------------------------------------
    // Dragging
    // --------------------------------------------------------------
    public void OnDrag(PointerEventData e)
    {
        if (!isDraggingGroup)
            return;

        float mouseLocalX = ScreenToLocalX(e.position);
        float finalX = mouseLocalX + offsetX;

        // pixels → seconds → beats
        float rawSeconds = finalX / timeline.TimelineView.PixelsPerSecond;
        float rawBeats = rawSeconds / (60f / LevelEditorData.Instance.BPM);

        // snap anchor in BEATS
        float snappedAnchorBeat =
            timeline.MarkerController.GetNearestSnapTime(rawBeats);

        float anchorStartBeat = dragStartTimes[this];
        float deltaBeat = snappedAnchorBeat - anchorStartBeat;

        if (Mathf.Approximately(deltaBeat, 0f))
            return;

        // apply SAME beat delta to all selected markers
        foreach (var pair in dragStartTimes)
        {
            TimelineMarker m = pair.Key;
            float startBeat = pair.Value;

            float newBeat = Mathf.Max(0f, startBeat + deltaBeat);

            float seconds = newBeat * (60f / LevelEditorData.Instance.BPM);
            float x = timeline.TimelineView.TimeToPixels(seconds);

            m.Rect.anchoredPosition =
                new Vector2(x, m.Rect.anchoredPosition.y);
        }
    }





    // --------------------------------------------------------------
    // Drag End
    // --------------------------------------------------------------
    public void OnEndDrag(PointerEventData e)
    {
        timeline.scrollRect.enabled = true;
        if (!isDraggingGroup)
            return;

        isDraggingGroup = false;

        var controller = timeline.MarkerController;

        // 1Commit ALL times without rebuilding
        foreach (var pair in dragStartTimes)
        {
            TimelineMarker m = pair.Key;

            float committedSeconds =
                m.Rect.anchoredPosition.x / timeline.TimelineView.PixelsPerSecond;

            float committedBeats =
                committedSeconds / (60f / LevelEditorData.Instance.BPM);

            controller.CommitMarkerTime_NoRebuild(
                m.Event,
                committedBeats
            );

        }

        // Rebuild ONCE
        controller.FinalizeMarkerCommit();

        dragStartTimes.Clear();
    }



    // --------------------------------------------------------------
    // Convert screen → local X inside the timeline
    // --------------------------------------------------------------
    private float ScreenToLocalX(Vector2 screenPos)
    {
        if (timeline == null || timeline.content == null)
            return 0;

        if (canvas == null)
            return 0;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            timeline.content,
            screenPos,
            canvas.worldCamera,
            out Vector2 local);

        return local.x;
    }

    // --------------------------------------------------------------
    // Selection Visual Feedback
    // --------------------------------------------------------------
    public void SetSelected(bool selected)
    {
        if (fillImage == null) return;

        fillImage.color = selected ? Color.yellow : originalColor;
    }

    public void SetEditing(bool editing)
    {
        isEditing = editing;

        if (outlineImage != null)
        {
            var c = outlineImage.color;
            c.a = editing ? 1f : 0f;  // turn outline on/off
            outlineImage.color = c;
        }
    }

    private void HandleAutoScroll(PointerEventData e)
    {
        return;
        /*
        RectTransform viewport = timeline.scrollRect.viewport;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport, e.position, null, out Vector2 local))
            return;

        float edgeZonePixels = 24f;   // how close to the edge (px)
        float maxScrollSpeed = 0.015f;

        float halfWidth = viewport.rect.width * 0.5f;
        float rightEdgeStart = halfWidth - edgeZonePixels;
        float leftEdgeStart  = -halfWidth + edgeZonePixels;

        float scrollDelta = 0f;

        // Right edge
        if (local.x > rightEdgeStart)
        {
            float t = Mathf.InverseLerp(rightEdgeStart, halfWidth, local.x);
            scrollDelta = Mathf.Lerp(0f, maxScrollSpeed, t);
        }
        // Left edge
        else if (local.x < leftEdgeStart)
        {
            float t = Mathf.InverseLerp(leftEdgeStart, -halfWidth, local.x);
            scrollDelta = Mathf.Lerp(0f, -maxScrollSpeed, t);
        }

        if (!Mathf.Approximately(scrollDelta, 0f))
        {
            timeline.scrollRect.horizontalNormalizedPosition =
                Mathf.Clamp01(timeline.scrollRect.horizontalNormalizedPosition + scrollDelta);
        }
        */

    }


}

