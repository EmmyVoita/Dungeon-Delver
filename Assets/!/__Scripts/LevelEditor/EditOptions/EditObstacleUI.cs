using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EditObstacleUI : LevelEventInspector
{
    public static EditObstacleUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private Vector2 defaultWindowPosition;


    private ChallengeEventData eventData;

    // Backup
    private float undo_time;
    private string undo_type;

    public ChallengeEventData CurrentEvent => eventData;
    public bool WindowActive => panel.activeSelf;

     private void OnEnable()
    {
        EditorHistory.OnUndo += HandleUndo;
    }

    private void OnDisable()
    {
        EditorHistory.OnUndo -= HandleUndo;
    }


    private void HandleUndo()
    {
        LevelEditorData.Instance.SortEvents();
        LevelEditorData.Instance.RecalculateMaxTime();

        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.BuildTimeline();

        if (eventData != null)
            PopulateUI();
    }


    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Start()
    {
        typeDropdown.onValueChanged.AddListener(_ => ApplyChanges());
        //undoButton.onClick.AddListener(OnUndo);
        deleteButton.onClick.AddListener(OnDelete);
    }

    public void ShowFor(ChallengeEventData evt)
    {
        eventData = evt;

        BackupOriginal();
        PopulateUI();

        LevelInspectorController.Instance.Show(this);
    }

    public override void Hide()
    {
        base.Hide();

        if (LevelTimelineUI.Instance.editingMarker != null)
        {
            LevelTimelineUI.Instance.editingMarker.SetEditing(false);
            LevelTimelineUI.Instance.editingMarker = null;
        }

        eventData = null;
    }


    // =========================================================
    private void BackupOriginal()
    {
        undo_time = eventData.beatTime;
        undo_type = eventData.obstacleType; // fileName
    }

    private void PopulateUI()
    {
        typeDropdown.ClearOptions();

        foreach (var def in ArrowSpawner.Instance.ChallengesTypeDefinitions)
            typeDropdown.options.Add(new TMP_Dropdown.OptionData(def.displayName));

        // Load the correct dropdown index based on the *fileName*
        typeDropdown.value = IndexOf(eventData.obstacleType);
        typeDropdown.RefreshShownValue();
    }

    // =========================================================
    private void ApplyChanges()
    {
        if (eventData == null) return;

        string oldType = eventData.obstacleType;
        string newType = ArrowSpawner.Instance.ChallengesTypeDefinitions[typeDropdown.value].fileName;

        EditChallengeCommand newCommand = new EditChallengeCommand(eventData,oldType,newType);
        EditorHistory.Instance.Execute(newCommand);

        //LevelTimelineUI.Instance.MarkerController.CommitMarkerTime_NoRebuild(eventData, eventData.beatTime);
        //LevelTimelineUI.Instance.MarkerController.FinalizeMarkerCommit();
            //OnMarkersTimesChanged();
        //EditorPlaybackController.Instance.RebuildSimulation();
    }

    private void OnUndo()
    {
        eventData.beatTime = undo_time;
        eventData.obstacleType = undo_type;

        PopulateUI();

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, Camera.main.transform.position);
    }

    private void OnDelete()
    {
        if (eventData == null)
            return;
            
        EditorHistory.Instance.Execute(new DeleteEventCommand(eventData));
        Hide();
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, Camera.main.transform.position);
    }

    // =========================================================
    int IndexOf(string fileName)
    {
        var defs = ArrowSpawner.Instance.ChallengesTypeDefinitions;

        for (int i = 0; i < defs.Count; i++)
            if (defs[i].fileName == fileName)   // <-- CORRECT FIELD
                return i;

        return 0;
    }
}
