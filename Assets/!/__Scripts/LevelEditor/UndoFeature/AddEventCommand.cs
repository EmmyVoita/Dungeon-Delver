using UnityEngine;

public class AddEventCommand : IEditorCommand
{
    private LevelEventData data;

    public AddEventCommand(LevelEventData data)
    {
        this.data = data;
    }
    public void Execute()
    {
        LevelEditorData.Instance.AddEvent(data);
    }

    public void Undo()
    {
        LevelEditorData.Instance.RemoveEvent(data);
        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();
    }
}