using UnityEngine;

public class EditChallengeCommand : IEditorCommand
{
    private readonly ChallengeEventData data;


    private readonly string oldType;
    private readonly string newType;

    public EditChallengeCommand(ChallengeEventData data, string oldType,string newType)
    {
        this.data = data;
        this.oldType = oldType;
        this.newType = newType;
    }

    public void Execute()
    {
        Apply(newType);

        LevelEditorData.Instance.SortEvents();
        LevelEditorData.Instance.RecalculateMaxTime();

        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.BuildTimeline();
    }

    public void Undo()
    {
        Apply(oldType);
    }

    private void Apply(string type)
    {
        data.obstacleType = type;
    }
}