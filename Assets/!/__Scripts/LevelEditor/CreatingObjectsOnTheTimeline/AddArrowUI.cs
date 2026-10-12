using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Globalization;

public class AddArrowUI : MonoBehaviour
{
    public static AddArrowUI Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Dropdown directionDropdown;
    [SerializeField] private TMP_InputField speedInput;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private Button addButton;

    [Header("References")]
    [SerializeField] private LevelEditorData editorData;
    [SerializeField] private EditorPlaybackController playbackController;
    [SerializeField] private ArrowSpawner arrowSpawner;
    [SerializeField] private TMP_InputField defaultArrowSpeedInput;


    private List<ArrowTypeDefinition> typeDefs;
    private int defaultArrowSpeed = 6;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        typeDefs = arrowSpawner.ArrowTypeDefinitions;
        addButton.onClick.AddListener(OnAddArrow);
        defaultArrowSpeedInput.SetTextWithoutNotify($"{defaultArrowSpeed}");
    }


    private void OnAddArrow()
    {
        if(LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to add arrow");
            return;
        } 
        TimelineToolController.Instance.EnterAddArrowMode();
    }

    public void OnDefaultArrowSpeedChanged(string _)
    {
        string text = defaultArrowSpeedInput.text;

        if (string.IsNullOrWhiteSpace(text))
        {
            UIToast.Error("DefaultArrowSpeed input cannot be empty!");
            return;
        }

        if (!int.TryParse(text,NumberStyles.Integer,CultureInfo.InvariantCulture, out int speed))
        {
            UIToast.Error($"Invalid Default Speed input: '{text}'");
            return;
        }

        UIToast.Show($"Default arrow Speed updated => {speed}");
        defaultArrowSpeed = speed;
    }

    public void AddArrowAtTime(float time)
    {
        var evt = new ArrowEventData(
            time,
            Vector2.up,
            defaultArrowSpeed,
            "normalarrow"
        );

        EditorHistory.Instance.Execute(new AddEventCommand(evt));

        //LevelEditorData.Instance.AddEvent(evt);
        LevelTimelineUI.Instance.BuildTimeline();
        playbackController.RebuildSimulation();
        TimelineToolController.Instance.ExitTool();
    }

}
