using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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


    private List<ArrowTypeDefinition> typeDefs;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        typeDefs = arrowSpawner.ArrowTypeDefinitions;

        PopulateDirectionDropdown();
        PopulateTypeDropdown();

        addButton.onClick.AddListener(OnAddArrow);
    }

    // ------------------------------------------------------------
    // Populate Dropdowns
    // ------------------------------------------------------------

    private void PopulateDirectionDropdown()
    {
        directionDropdown.ClearOptions();
        directionDropdown.AddOptions(new List<string>
        {
            "Up", "Down", "Left", "Right",
            "Up-Right", "Up-Left", "Down-Right", "Down-Left"
        });
    }

    private void PopulateTypeDropdown()
    {
        typeDropdown.ClearOptions();

        List<string> names = new List<string>();
        foreach (var def in typeDefs)
            names.Add(def.displayName);

        typeDropdown.AddOptions(names);
    }

    // ------------------------------------------------------------
    // Convert dropdown → direction
    // ------------------------------------------------------------

    private Vector2 DirectionFromDropdown(int index)
    {
        return index switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            3 => Vector2.right,
            4 => new Vector2(1, 1).normalized,
            5 => new Vector2(-1, 1).normalized,
            6 => new Vector2(1, -1).normalized,
            7 => new Vector2(-1, -1).normalized,
            _ => Vector2.up
        };
    }

    // ------------------------------------------------------------
    // Add Arrow Button Logic (UPDATED)
    // ------------------------------------------------------------
    private void OnAddArrow()
    {
        //AudioSettingsManager.PlayGeneralButtonSound();
        if(LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to add arrow");
            return;
        } 
        TimelineToolController.Instance.EnterAddArrowMode();
    }

    public void AddArrowAtTime(float time)
    {
        if (!float.TryParse(speedInput.text, out float speed))
        {
            UIToast.Error("❌ Invalid speed value");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        Vector2 dir = DirectionFromDropdown(directionDropdown.value);
        string typeName = typeDefs[typeDropdown.value].displayName;

        var evt = new ArrowEventData(
            time,
            "arrow",
            dir,
            speed,
            typeName
        );

        LevelEditorData.Instance.AddEvent(evt);

        LevelTimelineUI.Instance.BuildTimeline();
        playbackController.RebuildSimulation();
        TimelineToolController.Instance.ExitTool();

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
    }

}
