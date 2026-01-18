using UnityEngine;
using UnityEngine.EventSystems;

public class TimelineScrollRect : UnityEngine.UI.ScrollRect
{
    public override void OnDrag(PointerEventData eventData)
    {
        // If dragging a marker, DO NOT scroll
        if (eventData.pointerDrag != null &&
            eventData.pointerDrag.GetComponent<TimelineMarker>() != null)
        {
            return;
        }

        base.OnDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null &&
            eventData.pointerDrag.GetComponent<TimelineMarker>() != null)
        {
            return;
        }

        base.OnBeginDrag(eventData);
    }
}
