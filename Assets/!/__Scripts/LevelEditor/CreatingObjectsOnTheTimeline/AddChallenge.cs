using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AddChallengeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown challengeTypeDropdown;
    [SerializeField] private Button addButton;

    [Header("Refs")]
    [SerializeField] private LevelEditorData editorData;
    [SerializeField] private LevelTimelineUI timelineUI;
    [SerializeField] private EditorPlaybackController playback;
    [SerializeField] private ArrowSpawner spawner;

    private List<ObstacleTypeDefinition> defs;

    void Start()
    {
        defs = spawner.ChallengesTypeDefinitions;
        PopulateTypeDropdown();
        addButton.onClick.AddListener(OnAddChallenge);
    }

    void PopulateTypeDropdown()
    {
        challengeTypeDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var d in defs) names.Add(d.displayName);

        challengeTypeDropdown.AddOptions(names);
    }

    private void OnAddChallenge()
    {
        if(LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to add challenge");
            return;
        } 
        TimelineToolController.Instance.EnterAddChallengeMode();
    }

    public void AddChallengeAtTime(float time)
    {
        if(LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to add challenge");
            return;
        } 
  
        var def = defs[challengeTypeDropdown.value];

        var evt = new ChallengeEventData(time, def.fileName);

        EditorHistory.Instance.Execute(new AddEventCommand(evt));
        //editorData.events.Add(evt);
        editorData.SortEvents();
        editorData.RecalculateMaxTime();

        LevelTimelineUI.Instance.BuildTimeline();
        playback.RebuildSimulation();
        TimelineToolController.Instance.ExitTool();

        Debug.Log($"Added challenge '{def.displayName}' @ t={time:F2}");
    }
}
