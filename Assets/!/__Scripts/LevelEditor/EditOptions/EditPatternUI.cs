using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPatternUI : LevelEventInspector
{
    public static EditPatternUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Dropdown patternTypeDropdown;
    [SerializeField] private TMP_InputField lengthField;
    [SerializeField] private TMP_InputField speedField;

  
    private PatternEventData eventData;

    // Undo backup
    private float _undoBeatTime;
    private PatternType _undoPatternType;
    private int _undoLength;
    private float _undoSpeed;

    private bool _suppressApply;

    public PatternEventData CurrentEvent => eventData;


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

    private void Awake()
    {
        Instance = this;

        panel.SetActive(false);
    }

    private void Start()
    {
        SetupPatternTypeDropdown();

        patternTypeDropdown.onValueChanged.AddListener(_ => ApplyChanges());
        lengthField.onEndEdit.AddListener(_ => ApplyChanges());
        speedField.onEndEdit.AddListener(_ => ApplyChanges());

        //undoButton.onClick.AddListener(OnUndo);
        deleteButton.onClick.AddListener(OnDelete);
    }

    // ---------------------------------------------------------
    // Show / Hide
    // ---------------------------------------------------------

    public void ShowFor(PatternEventData evt)
    {
        eventData = evt;

        BackupOriginal();
        PopulateUI();

        LevelInspectorController.Instance.Show(this);
    }

    public override void Hide()
    {
        base.Hide();

        eventData = null;
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------

    private void SetupPatternTypeDropdown()
    {
        patternTypeDropdown.ClearOptions();

        string[] names = System.Enum.GetNames(typeof(PatternType));

        patternTypeDropdown.AddOptions(
            new System.Collections.Generic.List<string>(names)
        );
    }

    private void PopulateUI()
    {
        if (eventData == null)
            return;

        _suppressApply = true;

        patternTypeDropdown.SetValueWithoutNotify((int)eventData._patternType);
        lengthField.SetTextWithoutNotify(eventData._lengthInBeats.ToString());
        speedField.SetTextWithoutNotify(eventData._speed.ToString());

        patternTypeDropdown.RefreshShownValue();

        _suppressApply = false;
    }

    // ---------------------------------------------------------
    // Apply
    // ---------------------------------------------------------

    private void ApplyChanges()
    {
        if (_suppressApply || eventData == null)
            return;

        PatternType oldType = eventData._patternType;
        int oldLengthInBeats = eventData._lengthInBeats;
        float oldSpeed = eventData._speed;

        PatternType newType = (PatternType)patternTypeDropdown.value;
        int newLengthInBeats = eventData._lengthInBeats;
        float newSpeed = eventData._speed;

        if (int.TryParse(lengthField.text, out int length))
        {
            newLengthInBeats = Mathf.Max(1, length);
        }

        if (float.TryParse(speedField.text, out float speed))
        {
            newSpeed = Mathf.Max(0.1f, speed);
        }

        EditPatternCommand newCommand = new EditPatternCommand(eventData,
                                                            oldType,
                                                            oldLengthInBeats,
                                                            oldSpeed,
                                                            newType,
                                                            newLengthInBeats,
                                                            newSpeed);

        EditorHistory.Instance.Execute(newCommand);
        /*
        // Updates marker position/width.
        LevelTimelineUI.Instance.MarkerController.CommitMarkerTime_NoRebuild(eventData,eventData.beatTime);
        LevelTimelineUI.Instance.MarkerController.FinalizeMarkerCommit();

        EditorPlaybackController.Instance.RebuildSimulation();
        */
    }

    // ---------------------------------------------------------
    // Undo
    // ---------------------------------------------------------

    private void BackupOriginal()
    {
        _undoBeatTime = eventData.beatTime;
        _undoPatternType = eventData._patternType;
        _undoLength = eventData._lengthInBeats;
        _undoSpeed = eventData._speed;
    }

    private void OnUndo()
    {
        if (eventData == null)
            return;

        eventData.beatTime = _undoBeatTime;
        eventData._patternType = _undoPatternType;
        eventData._lengthInBeats = _undoLength;
        eventData._speed = _undoSpeed;

        PopulateUI();

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, Camera.main.transform.position);
    }

    // ---------------------------------------------------------
    // Delete
    // ---------------------------------------------------------

    private void OnDelete()
    {
        if (eventData == null)
            return;

        EditorHistory.Instance.Execute(new DeleteEventCommand(eventData));
        Hide();
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, Camera.main.transform.position);
    }
}