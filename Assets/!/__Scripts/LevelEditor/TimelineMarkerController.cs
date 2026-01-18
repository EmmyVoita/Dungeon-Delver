using System.Collections;
using System.Collections.Generic;
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


    [Header("References")]
    public TimelineView timelineView;
    public Slider scrubSlider;
    public RectTransform content;
    public GameObject markerPrefab;
    private RectTransform markerContainer;


    [Header("Settings")]
    public bool showQuarterBeats = true;
    public float markerOffsetY = 75f;
    private List<TimelineMarker> markers = new();

    private HashSet<TimelineMarker> selectedMarkers = new HashSet<TimelineMarker>();
     public HashSet<TimelineMarker> SelectedMarkers => selectedMarkers;
     public IReadOnlyList<TimelineMarker> AllMarkers => markers;




    void Awake()
    {
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
            switch (evt.objectType)
            {
                case "arrow":
                    CreateMarker(evt, Color.cyan);
                    break;

                case "obstacle":
                    CreateMarker(evt, Color.green);
                    break;
            }
        }
    }


    private void CreateMarker(ArrowEventData evt, Color? color = null)
    {
        // Instantiate marker prefab & set its position in the timeline
        GameObject obj = Instantiate(markerPrefab, markerContainer);
        var rt = obj.GetComponent<RectTransform>();

        float seconds = evt.beatTime * (60f / LevelEditorData.Instance.BPM);
        float x = timelineView.TimeToPixels(seconds);
        rt.anchoredPosition = new Vector2(x, markerOffsetY);


        // Add our time line marker script so we can handle ui events for it
        TimelineMarker marker = obj.AddComponent<TimelineMarker>();
        marker.Initialize(evt, LevelTimelineUI.Instance);

        // Set the color if specified
        if (color.HasValue)
        {
            obj.GetComponent<Marker>().fillImage.color = color.Value;
            marker.originalColor = color.Value;
        }

        // Add the marker to our list
        markers.Add(marker);
    }

    // Marker Updating
    // ------------------------------------------------------------------------------------

    public void CommitMarkerTime_NoRebuild(ArrowEventData evt, float time)
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
        scrubSlider.maxValue = timelineView.TimelineEndTime;

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
        }

        float t = EditorPlaybackController.Instance.CurrentTime;
    }


    public float GetNearestSnapTime(float rawBeat)
    {
        List<float> snapTimes = new();

        // Allow snapping ahead of cursor (in BEATS)
        float maxBeat = Mathf.Max(
            LevelEditorData.Instance.MaxTime,
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