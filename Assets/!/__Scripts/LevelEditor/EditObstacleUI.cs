using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EditObstacleUI : MonoBehaviour
{
    public static EditObstacleUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private Vector2 defaultWindowPosition;

    [Header("Buttons")]
    [SerializeField] private Button undoButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;

    private ArrowEventData eventData;

    // Backup
    private float undo_time;
    private string undo_type;

    public ArrowEventData CurrentEvent => eventData;
    public bool WindowActive => panel.activeSelf;

    private Vector2? lastWindowPosition = null;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Start()
    {
        typeDropdown.onValueChanged.AddListener(_ => ApplyChanges());
        undoButton.onClick.AddListener(OnUndo);
        deleteButton.onClick.AddListener(OnDelete);
        closeButton.onClick.AddListener(OnClose);
    }

    // =========================================================
    public void ShowFor(ArrowEventData evt, Vector2 screenPos)
    {
        eventData = evt;

        BackupOriginal();
        PopulateUI();

        if (WindowActive)
        {
            // window already open -> do not reposition
        }
        else if (lastWindowPosition.HasValue)
        {
            panel.SetActive(true);
            panel.GetComponent<RectTransform>().anchoredPosition = lastWindowPosition.Value;
        }
        else
        {
            panel.SetActive(true);
            PositionWindow(screenPos);
        }

        typeDropdown.RefreshShownValue();
    }

    public void Hide()
    {
        lastWindowPosition = panel.GetComponent<RectTransform>().anchoredPosition;
        panel.SetActive(false);

        if (LevelTimelineUI.Instance.editingMarker != null)
        {
            LevelTimelineUI.Instance.editingMarker.SetEditing(false);
            LevelTimelineUI.Instance.editingMarker = null;
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

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPos);

        panelRect.anchoredPosition = defaultWindowPosition;
    }

    // =========================================================
    private void BackupOriginal()
    {
        undo_time = eventData.beatTime;
        undo_type = eventData.nameOfGameObjectToSpawn; // fileName
    }

    private void PopulateUI()
    {
        typeDropdown.ClearOptions();

        foreach (var def in ArrowSpawner.Instance.ChallengesTypeDefinitions)
            typeDropdown.options.Add(new TMP_Dropdown.OptionData(def.displayName));

        // Load the correct dropdown index based on the *fileName*
        typeDropdown.value = IndexOf(eventData.nameOfGameObjectToSpawn);
        typeDropdown.RefreshShownValue();
    }

    // =========================================================
    private void ApplyChanges()
    {
        if (eventData == null) return;

        // Save *fileName* back to the event
        eventData.nameOfGameObjectToSpawn =
            ArrowSpawner.Instance.ChallengesTypeDefinitions[typeDropdown.value].fileName;

        LevelTimelineUI.Instance.MarkerController.CommitMarkerTime_NoRebuild(eventData, eventData.beatTime);
        LevelTimelineUI.Instance.MarkerController.FinalizeMarkerCommit();
            //OnMarkersTimesChanged();
        EditorPlaybackController.Instance.RebuildSimulation();
    }

    private void OnUndo()
    {
        eventData.beatTime = undo_time;
        eventData.nameOfGameObjectToSpawn = undo_type;

        PopulateUI();

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();
    }

    private void OnDelete()
    {
        LevelEditorData.Instance.events.Remove(eventData);

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.RebuildSimulation();

        Hide();
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
