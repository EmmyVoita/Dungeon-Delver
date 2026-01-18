using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EditArrowUI : MonoBehaviour
{
    public static EditArrowUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Dropdown directionDropdown;
    [SerializeField] private TMP_InputField speedField;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private Vector2 defaultWindowPosition;

    

    [Header("Buttons")]
    [SerializeField] private Button undoButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;   // <-- NEW

    private ArrowEventData eventData;

    // Backup
    private float undo_time;
    private Vector2 undo_direction;
    private float undo_speed;
    private string undo_type;

    public ArrowEventData CurrentEvent => eventData;
    public bool WindowActive => panel.activeSelf;

    private Vector2? lastWindowPosition = null;
    private bool suppressApply = false;




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
        closeButton.onClick.AddListener(OnClose);   // <-- NEW


        undoButton.onClick.AddListener(OnUndo);
        deleteButton.onClick.AddListener(OnDelete);
    }

    // =========================================================
    public void ShowFor(ArrowEventData evt, Vector2 screenPos)
    {
        eventData = evt;

        BackupOriginal();

        PopulateUI();

        if(WindowActive)
        {
            // Do nothing, already open
        }
        else if (lastWindowPosition.HasValue)
        {
            panel.SetActive(true);
            // Re-open where it was before
            panel.GetComponent<RectTransform>().anchoredPosition = lastWindowPosition.Value;
        }
        else if(!lastWindowPosition.HasValue)
        {
            panel.SetActive(true);
            // First time → place at default or cursor position
            PositionWindow(screenPos);
        }    
    }

    public void Hide()
    {
        Debug.Log("Hiding EditArrowUI");
        lastWindowPosition = panel.GetComponent<RectTransform>().anchoredPosition;
        panel.SetActive(false);

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

    private void OnClose()
    {
        Hide();
    }



    // =========================================================
    private void PositionWindow(Vector2 screenPos)
    {
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        // Get the Canvas correctly
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Convert screen → local space inside the canvas  
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPos);

        panelRect.anchoredPosition = defaultWindowPosition;
    }



    private void BackupOriginal()
    {
        undo_time = eventData.beatTime;
        undo_direction = eventData.direction;
        undo_speed = eventData.speed;
        undo_type = eventData.nameOfGameObjectToSpawn;
    }

    private void PopulateUI()
    {
        suppressApply = true;

        speedField.SetTextWithoutNotify(eventData.speed.ToString());

        directionDropdown.SetValueWithoutNotify(
            DirectionToIndex(eventData.direction)
        );

        typeDropdown.ClearOptions();
        foreach (var def in ArrowSpawner.Instance.ArrowTypeDefinitions)
            typeDropdown.options.Add(new TMP_Dropdown.OptionData(def.displayName));

        typeDropdown.SetValueWithoutNotify(
            GetTypeIndex(eventData.nameOfGameObjectToSpawn)
        );

        suppressApply = false;
    }


    // =========================================================
    private void ApplyChanges()
    {
        if (suppressApply) return;
        if (eventData == null) return;

        eventData.direction = IndexToDirection(directionDropdown.value);

        if (float.TryParse(speedField.text, out float s))
            eventData.speed = s;

        eventData.nameOfGameObjectToSpawn =
            ArrowSpawner.Instance.ArrowTypeDefinitions[typeDropdown.value].displayName;

        EditorPlaybackController.Instance.RebuildSimulation();

        LevelTimelineUI.Instance.MarkerController.CommitMarkerTime_NoRebuild(
            eventData,
            eventData.beatTime
        );
        LevelTimelineUI.Instance.MarkerController.FinalizeMarkerCommit();
    }


    private void OnUndo()
    {
        eventData.beatTime = undo_time;
        eventData.direction = undo_direction;
        eventData.speed = undo_speed;
        eventData.nameOfGameObjectToSpawn = undo_type;

        PopulateUI();

        EditorPlaybackController.Instance.RebuildSimulation();
        LevelTimelineUI.Instance.BuildTimeline();
    }

    private void OnDelete()
    {
        LevelEditorData.Instance.events.Remove(eventData);

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();

        Hide();
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
