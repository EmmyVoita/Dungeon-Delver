using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class EditArrowUI : LevelEventInspector
{
    public static EditArrowUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Dropdown directionDropdown;
    [SerializeField] private TMP_InputField speedField;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private Vector2 defaultWindowPosition;


    private ArrowEventData eventData;

    // Backup
    private float undo_time;
    private Vector2 undo_direction;
    private float undo_speed;
    private string undo_type;

    private bool suppressApply = false;

    public ArrowEventData CurrentEvent => eventData;
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
        directionDropdown.onValueChanged.AddListener(_ => ApplyChanges());
        speedField.onValueChanged.AddListener(_ => ApplyChanges());
        typeDropdown.onValueChanged.AddListener(_ => ApplyChanges());

        deleteButton.onClick.AddListener(OnDelete);
    }


    public void ShowFor(ArrowEventData evt)
    {
        eventData = evt;

        BackupOriginal();
        PopulateUI();

        LevelInspectorController.Instance.Show(this);
    }

    public override void Hide()
    {
        base.Hide();

        // remove edit highlight
        if (LevelTimelineUI.Instance != null)
        {
            if (LevelTimelineUI.Instance.editingMarker != null)
            {
                LevelTimelineUI.Instance.editingMarker.SetEditing(false);
                LevelTimelineUI.Instance.editingMarker = null;
            }
        }
        
        eventData = null;
    }



    private void BackupOriginal()
    {
        undo_time = eventData.beatTime;
        undo_direction = eventData.direction;
        undo_speed = eventData.speed;
        undo_type = eventData.arrowType;
    }

    private void PopulateUI()
    {
        suppressApply = true;

        speedField.SetTextWithoutNotify(eventData.speed.ToString());

        directionDropdown.SetValueWithoutNotify(DirectionToIndex(eventData.direction));
        directionDropdown.RefreshShownValue();

        // Rebuild type dropdown
        typeDropdown.ClearOptions();

        var options = new List<string>();

        foreach (var def in ArrowSpawner.Instance.ArrowTypeDefinitions)
            options.Add(def.displayName);

        typeDropdown.AddOptions(options);

        typeDropdown.SetValueWithoutNotify(
            GetTypeIndex(eventData.arrowType)
        );

        typeDropdown.RefreshShownValue();

        suppressApply = false;
    }


    // =========================================================
    private void ApplyChanges()
    {
        if (suppressApply) return;
        if (eventData == null) return;

        float oldTime = eventData.beatTime;
        Vector2 oldDirection = eventData.direction;
        float oldSpeed = eventData.speed;
        string oldType = eventData.arrowType;


        float newTime = eventData.beatTime;
        Vector2 newDirection = IndexToDirection(directionDropdown.value);

        float newSpeed = eventData.speed;

        if (float.TryParse(speedField.text, out float s))
            newSpeed = s;

        string newType = ArrowSpawner.Instance.ArrowTypeDefinitions[typeDropdown.value].displayName;


        EditArrowCommand newCommand = new EditArrowCommand(eventData,
                                                            oldTime,
                                                            oldDirection,
                                                            oldSpeed,
                                                            oldType,
                                                            newTime,
                                                            newDirection,
                                                            newSpeed,
                                                            newType);

        EditorHistory.Instance.Execute(newCommand);

        /*
        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.MarkerController.CommitMarkerTime_NoRebuild(eventData,eventData.beatTime);
        LevelTimelineUI.Instance.MarkerController.FinalizeMarkerCommit();
        */
    }


    private void OnUndo()
    {
        eventData.beatTime = undo_time;
        eventData.direction = undo_direction;
        eventData.speed = undo_speed;
        eventData.arrowType = undo_type;

        PopulateUI();

        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.BuildTimeline();

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
    int GetTypeIndex(string name)
    {
        var defs = ArrowSpawner.Instance.ArrowTypeDefinitions;
        for (int i = 0; i < defs.Count; i++)
            if (defs[i].displayName == name)
                return i;
        return 0;
    }

    int DirectionToIndex(Vector2 dir)
    {
        dir.Normalize();

        if (Vector2.Dot(dir, Vector2.up) > 0.9f) return 0;
        if (Vector2.Dot(dir, Vector2.down) > 0.9f) return 1;
        if (Vector2.Dot(dir, Vector2.left) > 0.9f) return 2;
        if (Vector2.Dot(dir, Vector2.right) > 0.9f) return 3;

        if (Vector2.Dot(dir, new Vector2(1, 1).normalized) > 0.9f) return 4;
        if (Vector2.Dot(dir, new Vector2(-1, 1).normalized) > 0.9f) return 5;
        if (Vector2.Dot(dir, new Vector2(1, -1).normalized) > 0.9f) return 6;
        if (Vector2.Dot(dir, new Vector2(-1, -1).normalized) > 0.9f) return 7;

        return 0;
    }


    Vector2 IndexToDirection(int index)
    {
        return index switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            3 => Vector2.right,
            4 => new Vector2(1,1).normalized,
            5 => new Vector2(-1,1).normalized,
            6 => new Vector2(1,-1).normalized,
            7 => new Vector2(-1,-1).normalized,
            _ => Vector2.up
        };
    }
}
