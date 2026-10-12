using UnityEngine;

public class EditArrowCommand : IEditorCommand
{
    private readonly ArrowEventData data;

    private readonly float oldTime;
    private readonly Vector2 oldDirection;
    private readonly float oldSpeed;
    private readonly string oldType;

    private readonly float newTime;
    private readonly Vector2 newDirection;
    private readonly float newSpeed;
    private readonly string newType;

    public EditArrowCommand(
        ArrowEventData data,
        float oldTime,
        Vector2 oldDirection,
        float oldSpeed,
        string oldType,
        float newTime,
        Vector2 newDirection,
        float newSpeed,
        string newType)
    {
        this.data = data;

        this.oldTime = oldTime;
        this.oldDirection = oldDirection;
        this.oldSpeed = oldSpeed;
        this.oldType = oldType;

        this.newTime = newTime;
        this.newDirection = newDirection;
        this.newSpeed = newSpeed;
        this.newType = newType;
    }

    public void Execute()
    {
        Apply(newTime,newDirection,newSpeed,newType);

        LevelEditorData.Instance.SortEvents();
        LevelEditorData.Instance.RecalculateMaxTime();

        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.BuildTimeline();
    }

    public void Undo()
    {
        Apply(oldTime,oldDirection,oldSpeed,oldType);
    }

    private void Apply(float time,Vector2 direction,float speed,string type)
    {
        data.beatTime = time;
        data.direction = direction;
        data.speed = speed;
        data.arrowType = type;
    }
}