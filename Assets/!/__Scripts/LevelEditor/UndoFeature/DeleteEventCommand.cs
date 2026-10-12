using UnityEngine;

public class DeleteEventCommand : IEditorCommand
{
    private LevelEventData data;

    public DeleteEventCommand(LevelEventData data)
    {
        this.data = data;
    }
    public void Execute()
    {
        LevelEditorData.Instance.events.Remove(data);
        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();
    }

    public void Undo()
    {
        LevelEditorData.Instance.AddEvent(data);
        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();
    }
}