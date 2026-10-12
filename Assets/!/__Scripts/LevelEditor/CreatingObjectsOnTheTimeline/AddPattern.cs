using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AddPattern : MonoBehaviour
{
    public static AddPattern Instance;

    [Header("UI References")]
    [SerializeField] private Button addButton;

    [Header("References")]
    [SerializeField] private LevelEditorData editorData;
    [SerializeField] private EditorPlaybackController playback;
    [SerializeField] private ArrowSpawner arrowSpawner;
    [SerializeField] private LevelTimelineUI timelineUI;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        addButton.onClick.AddListener(OnAddPattern);
    }


    private void OnAddPattern()
    {
        if(LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to add pattern");
            return;
        } 
        TimelineToolController.Instance.EnterAddPatternMode();
    }

    public void AddPatternAtTime(float time)
    {
        var evt = new PatternEventData(
            time,
            PatternType.Easy,
            4,
            6
        );

        EditorHistory.Instance.Execute(new AddEventCommand(evt));
        //editorData.events.Add(evt);
        editorData.SortEvents();
        editorData.RecalculateMaxTime();

        LevelTimelineUI.Instance.BuildTimeline();
        playback.RebuildSimulation();
        TimelineToolController.Instance.ExitTool();
    }

}
