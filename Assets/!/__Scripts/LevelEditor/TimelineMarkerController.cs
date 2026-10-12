using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum SelectionMode
{
    Replace,    // click
    Toggle,     // shift-click
    Add         // future: box select, ctrl-click, etc.
}

public class TimelineMarkerController : MonoBehaviour
{
    public event System.Action MarkerTimesChanged;
    public System.Action<TimelineMarker> OnMarkerSelected;
    [SerializeField] private float markerWidth = 8.0f;


    [Header("References")]
    public TimelineView timelineView;
    public Scrollbar scrubSlider;
    public RectTransform content;
    public GameObject markerPrefab;
    [SerializeField] private RectTransform markerContainer;

    [Header("ArrowStacking")]
    [SerializeField] private float arrowBaseY = 10f;
    [SerializeField] private float arrowLayerHeight = 28f;
    [SerializeField] private float arrowMarkerWidth = 24f;
    [SerializeField] private float markerSpacing = 2f;


    [Header("Settings")]
    public bool showEighthBeats = false;
    public bool showQuarterBeats = true;
    public float markerOffsetY = 75f;
    private List<TimelineMarker> markers = new();

    private HashSet<TimelineMarker> selectedMarkers = new HashSet<TimelineMarker>();
     public HashSet<TimelineMarker> SelectedMarkers => selectedMarkers;
     public IReadOnlyList<TimelineMarker> AllMarkers => markers;

    private bool viewArrowDirections = false;

    public void ToggleArrowDirectionView()
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, Camera.main.transform.position);

        viewArrowDirections = !viewArrowDirections;

        UIToast.Show($"Toggling arrow direction view => {(viewArrowDirections ? "On" : "Off")}");

        foreach(TimelineMarker marker in markers)
        {
            Marker markerComp = marker.GetComponent<Marker>();
            markerComp.EnableArrowImage(viewArrowDirections);
        }
    }


    void Awake()
    {
        // Create a UI object with a RectTransform
        //GameObject go = new GameObject("MARKERS", typeof(RectTransform));
        //markerContainer = go.GetComponent<RectTransform>();

        // Set parent correctly within UI hierarchy
        markerContainer.SetParent(content, false);

        // Stretch to match content dimensions
        markerContainer.anchorMin = new Vector2(0, 0);
        markerContainer.anchorMax = new Vector2(0, 1);
        markerContainer.pivot = new Vector2(0, 1);
        markerContainer.anchoredPosition = Vector2.zero;
        markerContainer.sizeDelta = Vector2.zero; // will match height automatically
    }


    // Create Markers
    // ------------------------------------------------------------------------------------

    public void BuildMarkers()
    {
        ClearSelectionInternal();
        // Destroy all existing markers from the marker container
        foreach (Transform child in markerContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Clear our existing marker list
        markers.Clear();

        // Create new markers. Switch on event type for color 
        foreach (var evt in LevelEditorData.Instance.events)
        {
            if(evt is ArrowEventData arrowEventData)
            {
                CreateMarker(arrowEventData, GetArrowColor(arrowEventData.arrowType));
            }
            else if(evt is ChallengeEventData challengeEventData)
            {
                CreateMarker(challengeEventData, Color.green);
            }
            else if(evt is PatternEventData patternEventData)
            {
                CreateMarker(patternEventData, Color.purple);
            }
        }
    }

    public Color GetArrowColor(string arrowType)
    {
        ArrowTypeDefinition definition = ArrowSpawner.Instance.ArrowTypeDefinitions.Find(e => e.displayName == arrowType);

        if(definition == null)
        {
            return Color.white;
        }

        return definition.markerColor;
    }

    private void LayoutArrowLayers(List<TimelineMarker> markers)
    {
        var arrowMarkers = markers
            .Where(m => m.Event is LevelEventData)
            .OrderBy(m => m.Event.beatTime)
            .ToList();

        float? currentBeat = null;
        int layer = 0;

        foreach (TimelineMarker marker in arrowMarkers)
        {
            // New beat -> start back at layer 0
            if (currentBeat == null || !Mathf.Approximately(marker.Event.beatTime, currentBeat.Value))
            {
                currentBeat = marker.Event.beatTime;
                layer = 0;
            }

            Vector2 position = marker.Rect.anchoredPosition;

            position.y = arrowBaseY + layer * arrowLayerHeight;

            marker.Rect.anchoredPosition = position;

            layer++;
        }
    }

    private int FindAvailableLayer(float markerLeft,List<float> layerEndXs)
    {
        for (int i = 0; i < layerEndXs.Count; i++)
        {
            if (markerLeft >= layerEndXs[i] + markerSpacing)
                return i;
        }

        return layerEndXs.Count;
    }


    private void CreateMarker(LevelEventData evt, Color? color = null)
    {
        // Instantiate marker prefab & set its position in the timeline
        GameObject obj = Instantiate(markerPrefab, markerContainer);
        var rt = obj.GetComponent<RectTransform>();

        float seconds = evt.beatTime * (60f / LevelEditorData.Instance.BPM);
        float x = timelineView.TimeToPixels(seconds);
        rt.anchoredPosition = new Vector2(x, markerOffsetY);


        // Add our time line marker script so we can handle ui events for it
        TimelineMarker marker = obj.AddComponent<TimelineMarker>();

        if(evt is PatternEventData patternEventData)
        {
            float beatsToSeconds = patternEventData._lengthInBeats * (60f / LevelEditorData.Instance.BPM);
            float width = timelineView.TimeToPixels(beatsToSeconds);

            marker.Initialize(evt, LevelTimelineUI.Instance, width);
        }
        else if(evt is ArrowEventData arrowEventData)
        {   
            Marker markerComp = marker.GetComponent<Marker>();
            markerComp.AssignSprite(arrowEventData.direction);
            markerComp.EnableArrowImage(viewArrowDirections);

            marker.Initialize(evt, LevelTimelineUI.Instance, markerWidth);
        }
        else
        {
            marker.Initialize(evt, LevelTimelineUI.Instance, markerWidth);
        }

        

        // Set the color if specified
        if (color.HasValue)
        {
            obj.GetComponent<Marker>().fillImage.color = color.Value;
            marker.originalColor = color.Value;
        }

        // Add the marker to our list
        markers.Add(marker);

        UpdateMarkerPositions();
    }

    // Marker Updating
    // ------------------------------------------------------------------------------------

    public void CommitMarkerTime_NoRebuild(LevelEventData evt, float time)
    {
        evt.beatTime = Mathf.Max(0f, time);
    }


    public void FinalizeMarkerCommit()
    {
        // Expand timeline if needed
        foreach (var evt in LevelEditorData.Instance.events)
            timelineView.EnsureCoversTime(evt.beatTime);

        LevelEditorData.Instance.RecalculateMaxTime();
        LevelEditorData.Instance.SortEvents();

        UpdateMarkerPositions();
        //scrubSlider.maxValue = timelineView.TimelineEndTime;

        MarkerTimesChanged?.Invoke();
        EditorPlaybackController.Instance.RebuildSimulation();
    }



    // Only moves markers, does not rebuild UI
    public void UpdateMarkerPositions()
    {
        foreach (var marker in markers)
        {
            float seconds = marker.Event.beatTime * (60f / LevelEditorData.Instance.BPM);
            float x = timelineView.TimeToPixels(seconds);
            marker.Rect.anchoredPosition = new Vector2(x, markerOffsetY);
            marker.UpdateTransform(timelineView);
        }

        float t = EditorPlaybackController.Instance.CurrentTime;

         // 2. Now that actual screen positions/sizes are known,
        //    resolve overlaps.
        LayoutArrowLayers(markers);
    }


    public float GetNearestSnapTime(float rawBeat)
    {
        List<float> snapTimes = new();

        // Allow snapping ahead of cursor (in BEATS)
        float maxBeat = Mathf.Max(
            LevelEditorData.Instance.MaxBeatTime,
            rawBeat + 2f
        );

        int wholeCount = Mathf.CeilToInt(maxBeat);

        // Whole beats
        for (int i = 0; i <= wholeCount; i++)
            snapTimes.Add(i);

        // Quarter beats
        if (showQuarterBeats)
        {
            for (int i = 0; i <= wholeCount; i++)
            {
                snapTimes.Add(i + 0.25f);
                snapTimes.Add(i + 0.50f);
                snapTimes.Add(i + 0.75f);
            }
        }

        if(showEighthBeats)
        {
            for (int i = 0; i <= wholeCount; i++)
            {
                snapTimes.Add(i + 0.125f);
                snapTimes.Add(i + 0.375f);
                snapTimes.Add(i + 0.625f);
                snapTimes.Add(i + 0.875f);
            }
        }

        if (snapTimes.Count == 0)
            return rawBeat;

        // Find nearest snap
        float nearest = snapTimes[0];
        float bestDist = Mathf.Abs(rawBeat - nearest);

        for (int i = 1; i < snapTimes.Count; i++)
        {
            float dist = Mathf.Abs(rawBeat - snapTimes[i]);
            if (dist < bestDist)
            {
                nearest = snapTimes[i];
                bestDist = dist;
            }
        }

        return nearest;
    }


    // Marker Select Logic
    //-----------------------------------------------------------------------
    public bool IsSelected(TimelineMarker m) => selectedMarkers.Contains(m);

    public void SelectMarker(TimelineMarker marker, SelectionMode mode)
    {

        switch (mode)
        {
            case SelectionMode.Replace:
                SelectSingleInternal(marker);
                break;

            case SelectionMode.Toggle:
                ToggleInternal(marker);
                break;

            case SelectionMode.Add:
                AddInternal(marker);
                break;
        }

        OnMarkerSelected?.Invoke(marker);
    }

    private void ClearSelectionInternal()
    {
        foreach (var m in selectedMarkers)
            m.SetSelected(false);

        selectedMarkers.Clear();
    }

    private void SelectSingleInternal(TimelineMarker marker)
    {
        ClearSelectionInternal();
        selectedMarkers.Add(marker);
        marker.SetSelected(true);
    }

    private void ToggleInternal(TimelineMarker marker)
    {
        if (selectedMarkers.Remove(marker))
            marker.SetSelected(false);
        else
        {
            selectedMarkers.Add(marker);
            marker.SetSelected(true);
        }
    }

    private void AddInternal(TimelineMarker marker)
    {
        if (selectedMarkers.Add(marker))
            marker.SetSelected(true);
    }

    public void ClearSelectionPreview()
    {
        foreach (var m in selectedMarkers)
            m.SetSelected(false);

        selectedMarkers.Clear();
    }

}