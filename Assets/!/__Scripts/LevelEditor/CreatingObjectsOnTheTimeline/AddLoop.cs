using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AddLoop : MonoBehaviour
{
    public static AddLoop Instance;

    [Header("UI References")]
    [SerializeField] private Button addButton;
    [SerializeField] private TimelineView timelineView;
    [SerializeField] private GameObject loopMarkerPrefab;
    [SerializeField] private GameObject loopMarkerContainer;
    [SerializeField] private float markerYOffset;


    [Header("References")]
    [SerializeField] private LevelEditorData editorData;
    [SerializeField] private EditorPlaybackController playback;
    [SerializeField] private ArrowSpawner arrowSpawner;
    [SerializeField] private LevelTimelineUI timelineUI;

    private int index = 0;
    private float startTime;
    private float endTime;
    private bool loopActive;


    private RectTransform loopMarkerContainerRect;

    private void Awake()
    {
        Instance = this;

        loopMarkerContainerRect = loopMarkerContainer.GetComponent<RectTransform>();

        loopMarkerContainerRect.SetParent(timelineUI.content,false);

        loopMarkerContainerRect.anchorMin = new Vector2(0f, 0f);
        loopMarkerContainerRect.anchorMax = new Vector2(0f, 1f);

        loopMarkerContainerRect.pivot = new Vector2(0f, 1f);

        loopMarkerContainerRect.anchoredPosition = Vector2.zero;
        loopMarkerContainerRect.sizeDelta = Vector2.zero;

        loopActive = false;
    }

    void Start()
    {
        addButton.onClick.AddListener(OnAddLoop);
    }

    private void OnEnable()
    {
        LevelTimelineUI.OnRebuildTimeline += HandleRebuildTimeline;
    }   

    private void OnDisable()
    {
        LevelTimelineUI.OnRebuildTimeline -= HandleRebuildTimeline;
    } 


    private void OnAddLoop()
    {
        loopActive = !loopActive;

        if(LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to add pattern");
            return;
        } 

        foreach (Transform child in loopMarkerContainer.transform)
        {
            Destroy(child.gameObject);
        }

        index = 0;
        startTime = 0;
        endTime = 0;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

        if(loopActive)
            TimelineToolController.Instance.EnterAssignLoopMode();
        else
        {
            EditorPlaybackController.Instance.ClearLoopTool();
            TimelineToolController.Instance.ExitTool();
        }
    }

    public void AddLoopMark(float time)
    {
        if (index == 0)
        {
            // First click
            startTime = time * (60f / LevelEditorData.Instance.BPM);
            index = 1;

            Debug.Log($"Loop start: {startTime}");

            CreateLoopMarker(startTime);

            return;
        }

        if (index == 1)
        {
            // Second click
            endTime = time * (60f / LevelEditorData.Instance.BPM);

            if(endTime < startTime)
            {
                float t = startTime;
                startTime = endTime;
                endTime = t;
            }

            Debug.Log($"Loop end: {endTime}");

            EditorPlaybackController.Instance.AssignLoop(
                startTime,
                endTime
            );

            CreateLoopMarker(endTime);

            index = 0;

            TimelineToolController.Instance.ExitTool();

            LevelTimelineUI.Instance.BuildTimeline();
            playback.RebuildSimulation();
        }
    }


    public void HandleRebuildTimeline(float pps, float max)
    {
        UpdateLoopMarkers();
    }

    public void UpdateLoopMarkers()
    {
        if (loopMarkerPrefab == null || loopMarkerContainer == null)
            return;

        // Clear existing numbers
        foreach (Transform child in loopMarkerContainer.transform)
        {
            Destroy(child.gameObject);
        }

        CreateLoopMarker(startTime);
        CreateLoopMarker(endTime);
    }

    private void CreateLoopMarker(float time)
    {
        GameObject loopMarker = Instantiate(loopMarkerPrefab, loopMarkerContainer.transform);
        RectTransform rect = loopMarker.transform as RectTransform;

        float x = timelineView.TimeToPixels(time);
        rect.anchoredPosition = new Vector2(x, markerYOffset);
    }

}
