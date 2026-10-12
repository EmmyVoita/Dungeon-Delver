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
        if (!TimelineToolController.Instance.IsAddArrowMode && 
            !TimelineToolController.Instance.IsAddChallengeMode &&
            !TimelineToolController.Instance.IsAddPatternMode &&
            !TimelineToolController.Instance.IsAssignLoopMode)
            return;

        LevelTimelineUI.Instance.UpdatePlacementFromCursor(e);
    }

    public void OnPointerDown(PointerEventData e)
    {
        switch(TimelineToolController.Instance.CurrentTool)
        {
            case TimelineTool.AddArrow:

                if (e.button == PointerEventData.InputButton.Left)
                {
                    PlayAudio();
                    LevelTimelineUI.Instance.ConfirmArrowPlacement(LevelTimelineUI.Instance.CurrentPlacementTime);
                }
                else
                    TimelineToolController.Instance.ExitTool();

                e.Use();
                break;

            case TimelineTool.AddChallenge:

                if (e.button == PointerEventData.InputButton.Left)
                {
                    PlayAudio();
                    LevelTimelineUI.Instance.ConfirmChallengePlacement(LevelTimelineUI.Instance.CurrentPlacementTime);
                }
                else
                    TimelineToolController.Instance.ExitTool();

                e.Use();
                break;

            case TimelineTool.AddPattern:
                if (e.button == PointerEventData.InputButton.Left)
                {
                    PlayAudio();
                    LevelTimelineUI.Instance.ConfirmPatternPlacement(LevelTimelineUI.Instance.CurrentPlacementTime);
                }
                else
                    TimelineToolController.Instance.ExitTool();

                e.Use();
                break;
            case TimelineTool.AssignLoop:
                if (e.button == PointerEventData.InputButton.Left)
                {
                    PlayAudio();
                    LevelTimelineUI.Instance.ConfirmLoopPlacement(LevelTimelineUI.Instance.CurrentPlacementTime);
                }
                else
                    TimelineToolController.Instance.ExitTool();

                e.Use();
                break;
            default:
            Debug.Log("TimelineInteractionHandler: Not in AddArrowMode, ignoring click");
            break;
        }
    }

    private void PlayAudio()
    {
         AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, Camera.main.transform.position);
    } 

    public void OnPointerExit(PointerEventData e)
    {
        if (TimelineToolController.Instance.IsAddArrowMode ||
            TimelineToolController.Instance.IsAddChallengeMode)
        {
            LevelTimelineUI.Instance.ShowPlacementMarker(false);
        }
    }
}
