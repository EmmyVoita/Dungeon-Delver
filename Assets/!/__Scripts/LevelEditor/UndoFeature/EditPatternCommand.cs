using UnityEngine;

public class EditPatternCommand : IEditorCommand
{
    private readonly PatternEventData data;


    private readonly PatternType oldType;
    private readonly int oldLengthInBeats;
    private readonly float oldSpeed;

    private readonly PatternType newType;
    private readonly int newLengthInBeats;
    private readonly float newSpeed;

    public EditPatternCommand(PatternEventData data, 
                              PatternType oldType,
                              int oldLengthInBeats,
                              float oldSpeed,
                              PatternType newType,
                              int newLengthInBeats,
                              float newSpeed)
    {
        this.data = data;
        this.oldType = oldType;
        this.newType = newType;
        this.oldLengthInBeats = oldLengthInBeats;
        this.oldSpeed = oldSpeed;
        this.newLengthInBeats = newLengthInBeats;
        this.newSpeed = newSpeed;
    }

    public void Execute()
    {
        Apply(newType, newLengthInBeats, newSpeed);

        LevelEditorData.Instance.SortEvents();
        LevelEditorData.Instance.RecalculateMaxTime();

        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.BuildTimeline();
    }

    public void Undo()
    {
        Apply(oldType, oldLengthInBeats, oldSpeed);
    }

    private void Apply(PatternType type, int lengthInBeats, float speed)
    {
        data._speed = speed;
        data._lengthInBeats = lengthInBeats;
        data._patternType = type;
    }
}