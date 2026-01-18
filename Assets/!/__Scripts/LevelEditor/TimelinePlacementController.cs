using UnityEngine;
using UnityEngine.EventSystems;

public class TimelinePlacementController : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    public void OnPointerMove(PointerEventData e)
    {
        if (!TimelineToolController.Instance.IsAddArrowMode)
            return;

        LevelTimelineUI.Instance.UpdatePlacementFromCursor(e);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (TimelineToolController.Instance.IsAddArrowMode)
            LevelTimelineUI.Instance.ShowPlacementMarker(false);
    }
}
