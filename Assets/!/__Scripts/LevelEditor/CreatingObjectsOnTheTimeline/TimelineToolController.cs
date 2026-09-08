using UnityEngine;

public enum TimelineTool
{
    None,
    AddArrow,
    AddChallenge
}

public class TimelineToolController : MonoBehaviour
{
    public static TimelineToolController Instance { get; private set; }

    public TimelineTool CurrentTool { get; private set; } = TimelineTool.None;

    private void OnEnable()
    {
        LevelEditorSessionController.OnLoadLevelPressed += ExitTool;
    }

    private void OnDisable()
    {
        LevelEditorSessionController.OnLoadLevelPressed -= ExitTool;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (CurrentTool != TimelineTool.None && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitTool();
        }
    }

    public void SwitchTool(TimelineTool newTool)
    {
        if(UIToast.Instance != null) UIToast.Show($"Switching tool to {newTool}");
        CurrentTool = newTool;
    }

    public void EnterAddArrowMode()
    {
        SwitchTool(TimelineTool.AddArrow);
        LevelTimelineUI.Instance.ShowPlacementMarker(true);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // optional
    }

    public void EnterAddChallengeMode()
    {
        SwitchTool(TimelineTool.AddChallenge);
        LevelTimelineUI.Instance.ShowPlacementMarker(true);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // optional
    }

    public void ExitTool()
    {
        SwitchTool(TimelineTool.None);
        LevelTimelineUI.Instance.ShowPlacementMarker(false);
    }

    public bool IsAddArrowMode => CurrentTool == TimelineTool.AddArrow;
    public bool IsAddChallengeMode => CurrentTool == TimelineTool.AddChallenge;
}
