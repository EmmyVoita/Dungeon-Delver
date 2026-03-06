using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
    using System.Globalization;
using TMPro;

public class LevelTimelineUI : MonoBehaviour
{

    public static LevelTimelineUI Instance;
    // ===========================================================
    // Timeline UI
    // ===========================================================

    [Header("Timeline View")]
    [SerializeField] private TimelineView timelineView;
    [SerializeField] private TimelineMarkerController markerController;
    public TimelineView TimelineView => timelineView;
    public TimelineMarkerController MarkerController => markerController;

    
    [Header("Timeline UI")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public TimelineGridGraphic gridGraphic;
    public bool useIndividualMarkers = false;   
    [SerializeField] private TMP_InputField bpmInput;


    [Header("Prefabs")]
    public GameObject markerPrefab;
    public GameObject tickPrefab;



    [Header("Color Settings")]
    public Color wholeBeatColor = new Color(1, 1, 1, 0.5f);
    public Color quarterBeatColor = new Color(1, 1, 1, 0.15f);

    [Header("Tick Height Settings")]
    public float wholeTickHeight = 100f;
    public float quarterTickHeight = 50f;

    public float markerOffsetY = 100f;
    public float tickOffsetY = 100f;

    [Header("Scrubber")]
    public Slider scrubSlider;

    [Header("Scroll Settings")]
    [SerializeField] private float zoomScrollSensitivity = 1f;


    [Header("Settings")]
    

    // Beat Grid Toggles
    public bool showQuarterBeats = true;
    public bool showEighthBeats = true;

    // Snap Settings
    public enum SnapMode { None, Quarter, Eighth }
    public SnapMode snapMode = SnapMode.Eighth;

    // Internals
    private bool userDraggingSlider = false;

    private List<TimelineMarker> markers = new();
    private List<RectTransform> tickMarks = new();

    private List<RectTransform> quarterTicks = new();
    private List<RectTransform> eighthTicks = new();
    private List<RectTransform> wholeTicks = new(); // always visible



    // === Multi-Select Support ===
    
    public Color selectionBrighten = new Color(1.2f, 1.2f, 1.2f, 1f); // multiplier

    public TimelineMarker editingMarker;

    private RectTransform markerContainer;

    [Header("Placement Marker")]
    [SerializeField] private RectTransform placementLine;

    public float CurrentPlacementTime { get; private set; }


    private float lastPlacementSnapTime = float.NaN;
    private float lastTickTime = float.NaN;
    public float minTickInterval = 0.1f; // seconds

    void Awake()
    {
        Instance = this;

        // Create a UI object with a RectTransform
        GameObject go = new GameObject("MARKERS", typeof(RectTransform));
        markerContainer = go.GetComponent<RectTransform>();

        // Set parent correctly within UI hierarchy
        markerContainer.SetParent(content, false);

        // Stretch to match content dimensions
        markerContainer.anchorMin = new Vector2(0, 0);
        markerContainer.anchorMax = new Vector2(0, 1);
        markerContainer.pivot = new Vector2(0, 1);
        markerContainer.anchoredPosition = Vector2.zero;
        markerContainer.sizeDelta = Vector2.zero; // will match height automatically

        placementLine.SetParent(content, false);
        placementLine.gameObject.SetActive(false);
    }

    void Start()
    {
        scrubSlider.onValueChanged.AddListener(OnSliderScrub);
    }

    void Update()
    {
        if (!userDraggingSlider)
        {
            float t = EditorPlaybackController.Instance.CurrentTime;
            scrubSlider.SetValueWithoutNotify(t);
            timelineView.SyncScrollToTime(t);
        }

        // Zoom handling
        float scroll = Input.mouseScrollDelta.y * zoomScrollSensitivity;
        if (scroll != 0)
        {
            timelineView.ApplyZoom(scroll);
            OnZoomChanged();
        }
    }

    private float SnapBeats(float rawBeats)
    {
        switch (snapMode)
        {
            case SnapMode.None:
                return rawBeats;

            case SnapMode.Quarter:
                return Mathf.Round(rawBeats);

            case SnapMode.Eighth:
                return Mathf.Round(rawBeats * 2f) / 2f;

            default:
                return rawBeats;
        }
    }

    // Building the timeline
    // ----------------------------------------------------------

    public void BuildTimeline()
    {
        // Grab the max time from level data to build the width of the timeline
        var data = LevelEditorData.Instance;
        float maxTime = data.MaxTime;

        float width = timelineView.TimelineEndTime * timelineView.PixelsPerSecond;
        content.sizeDelta = new Vector2(width, content.sizeDelta.y);

        // Only delete markers inside the MARKERS container
        foreach (Transform child in markerContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Rebuild our arrow/obstacle markers
        markerController.BuildMarkers();

        // Scrubber settings
        scrubSlider.minValue = 0;
        scrubSlider.maxValue = maxTime;
        scrubSlider.value = Mathf.Clamp(scrubSlider.value, 0, maxTime);

        // Update grid graphic
        gridGraphic.pixelsPerSecond = timelineView.PixelsPerSecond;
        gridGraphic.maxTime = maxTime;
        gridGraphic.SetVerticesDirty();

        bpmInput.SetTextWithoutNotify(
            LevelEditorData.Instance.BPM.ToString("F0")
        );
    }


    // BPM text input field handling
    // ----------------------------------------------------------

    public void OnBPMChanged(string _)
    {
        string text = bpmInput.text;

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError("BPM input cannot be empty!");
            return;
        }

        if (!float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float bpm))
        {
            Debug.LogError($"Invalid BPM input: '{text}'");
            return;
        }

        LevelEditorData.Instance.SetBPM(bpm);
        markerController.BuildMarkers();
        EditorPlaybackController.Instance.RebuildSimulation();

        // Update grid graphic
        gridGraphic.pixelsPerSecond = timelineView.PixelsPerSecond;
        gridGraphic.maxTime = timelineView.TimelineEndTime;
        gridGraphic.SetVerticesDirty();
    }

    // Right click show context menu handing
    // ----------------------------------------------------------

    public void ShowContextMenu(TimelineMarker marker, Vector2 screenPos)
    {
        // Turn off previous editing highlight
        if (editingMarker != null)
            editingMarker.SetEditing(false);

        // Mark this one as currently being edited
        editingMarker = marker;
        marker.SetEditing(true);

        if (marker.Event.objectType == "obstacle")
        {
            // Show the editor window
            EditObstacleUI.Instance.ShowFor(marker.Event, screenPos);
        }
        else if (marker.Event.objectType == "arrow")
        {
            // Show the editor window
            EditArrowUI.Instance.ShowFor(marker.Event, screenPos);
        }
    }

    // Moving the slider moves the timeline
    // --------------------------------------------------------------

    private void OnSliderScrub(float value)
    {
        userDraggingSlider = true;
        EditorPlaybackController.Instance.JumpToTime(value);
        timelineView.SyncScrollToTime(value);
        userDraggingSlider = false;
    }

    // Zoom changed – update grid graphic
    // --------------------------------------------------------------

     public void OnZoomChanged()
    {
        // Update marker positions
        markerController.UpdateMarkerPositions();

        // Update grid graphic
        gridGraphic.pixelsPerSecond = timelineView.PixelsPerSecond;
        gridGraphic.maxTime = timelineView.TimelineEndTime;
        gridGraphic.SetVerticesDirty();
    }

    // Placement marker during add arrow mode
    // --------------------------------------------------------------

    public void UpdatePlacementFromCursor(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            e.position,
            e.pressEventCamera,
            out Vector2 local
        );

        /*
        float rawTime = local.x / timelineView.PixelsPerSecond;
        float snapped = MarkerController.GetNearestSnapTime(rawTime);
        snapped = Mathf.Max(0f, snapped);
        */

        float rawSeconds = local.x / timelineView.PixelsPerSecond;

        // seconds → beats
        float rawBeats = rawSeconds / (60f / LevelEditorData.Instance.BPM);

        // snap in BEATS
        float snappedBeats = MarkerController.GetNearestSnapTime(rawBeats);

        snappedBeats = Mathf.Max(0f, snappedBeats);



        

        // 🔑 Only react if snap position changed
        if (!Mathf.Approximately(snappedBeats, lastPlacementSnapTime))
        {
            lastPlacementSnapTime = snappedBeats;

            CurrentPlacementTime = snappedBeats;
            UpdatePlacementMarker(snappedBeats);

            // ⏱️ MIN DELAY CHECK (AFTER snap change)
            if (float.IsNaN(lastTickTime) || Time.time - lastTickTime >= minTickInterval)
            {
                lastTickTime = Time.time;
                AudioSettingsManager.PlayGeneralButtonSound();
            }
        }
    }


    public void UpdatePlacementMarker(float snappedTime)
    {
        float seconds = snappedTime * (60f / LevelEditorData.Instance.BPM);
        float x = timelineView.TimeToPixels(seconds);

        placementLine.anchoredPosition = new Vector2(x, 0f);
        placementLine.gameObject.SetActive(true);
    }


    public void ShowPlacementMarker(bool show)
    {
        placementLine.gameObject.SetActive(show);

        if (show)
        {
            lastPlacementSnapTime = float.NaN;
            lastTickTime = float.NaN;
        }   
    }


    public void ConfirmArrowPlacement(float time)
    {
        AddArrowUI.Instance.AddArrowAtTime(time);
    }


    // ===========================================================
    // Playhead Position
    // ===========================================================
    
    
}
