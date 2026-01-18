using UnityEngine;
using UnityEngine.EventSystems;

public class TimelineInteractionHandler :
    MonoBehaviour,
    IPointerMoveHandler,
    IPointerDownHandler,
    IPointerExitHandler
{
    public void OnPointerMove(PointerEventData e)
    {
        if (!TimelineToolController.Instance.IsAddArrowMode)
            return;

        LevelTimelineUI.Instance.UpdatePlacementFromCursor(e);
    }

    public void OnPointerDown(PointerEventData e)
    {
        Debug.Log("TimelineInteractionHandler: OnPointerDown");
        if (!TimelineToolController.Instance.IsAddArrowMode)
        {
            Debug.Log("TimelineInteractionHandler: Not in AddArrowMode, ignoring click");
            return;
        }
            

        if (e.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("TimelineInteractionHandler: Confirming arrow placement");
            LevelTimelineUI.Instance.ConfirmArrowPlacement(
                LevelTimelineUI.Instance.CurrentPlacementTime
            );
            e.Use();
        }
        else
        {
            // Right-click / middle-click cancels
            TimelineToolController.Instance.ExitTool();
            e.Use();
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (TimelineToolController.Instance.IsAddArrowMode)
        {
            LevelTimelineUI.Instance.ShowPlacementMarker(false);
        }
    }
}
