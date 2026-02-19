using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelEditorUIController : MonoBehaviour
{
    const string LEVEL_ROOT = "Levels/";

    [Header("UI References")]
    public TMP_InputField levelFileInput;
    public Button loadLevelButton;

    [Header("Editor Components")]
    public LevelEditorData editorData;
    public EditorPlaybackController playbackController;

    void Start()
    {
        loadLevelButton.onClick.AddListener(OnLoadLevelPressed);

        // ======================================================
        // 🔙 RETURNING FROM TEST SESSION
        // ======================================================
        if (TestSession.runSingleLevel)
        {
            Debug.Log("🔄 Returning from test session");

            TestSession.runSingleLevel = false;

            if (TestSession.tempLevelAsset != null)
            {
                Debug.Log("🔄 Loading temp test level from memory");

                editorData.LoadLevelFromText(
                    TestSession.tempLevelAsset.text
                );
            }

            if (TestSession.originalLevelAsset != null)
            {
                editorData.currentLevelAsset =
                    TestSession.originalLevelAsset;

                levelFileInput.text =
                    TestSession.originalLevelAsset.name;
            }

            LevelTimelineUI.Instance.BuildTimeline();
            playbackController.BuildSimulatedArrows();
            playbackController.Stop();
            playbackController.JumpToTime(0);

            Debug.Log("✅ Editor restored from test session");
        }
    }

    // ------------------------------------------------------
    // Load level normally
    // ------------------------------------------------------
    private void OnLoadLevelPressed()
    {
        string assetName = levelFileInput.text.Trim();
        if (string.IsNullOrEmpty(assetName))
        {
            UIToast.Error("❌ No level name entered");
            AudioSettingsManager.PlayNegativeUISound();
            return;
        }

        TextAsset level = Resources.Load<TextAsset>(LEVEL_ROOT + assetName);
        if (level == null)
        {
            UIToast.Error($"❌ Could not find TextAsset: {LEVEL_ROOT + assetName}");
            AudioSettingsManager.PlayNegativeUISound();
            return;
        }

        editorData.LoadLevelFromText(level.text);
        editorData.currentLevelAsset = level;

        LevelTimelineUI.Instance.BuildTimeline();
        playbackController.BuildSimulatedArrows();
        playbackController.Stop();
        playbackController.JumpToTime(0);

        if(UIToast.Instance != null) UIToast.Show($"✅ Loaded level asset: {level.name}");
        AudioSettingsManager.PlayGeneralButtonSound();
    }

    // ------------------------------------------------------
    // ▶ TEST LEVEL
    // ------------------------------------------------------
    public void OnClick_TestLevel()
    {
        if (editorData.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to test");
            AudioSettingsManager.PlayNegativeUISound();
            return;
        }

        TestSession.levelMusic = EditorPlaybackController.Instance.LevelEditorTestMusic;

        // Save original
        TestSession.originalLevelAsset =
            editorData.currentLevelAsset;

        // Create a TEMP runtime TextAsset from editor state
        string serializedLevel =
            editorData.SerializeToString();

        TestSession.tempLevelAsset =
            new TextAsset(serializedLevel);

        TestSession.runSingleLevel = true;
        TestSession.returnScene =
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        GameSceneLoader.PendingConfig = new GameSceneConfig
        {
            Mode = GameMode.LevelEditorTest,
            PracticeObstacle = null,
            DirectionMode = JumpDirectionMode.FourDirectional
        };

        Debug.Log("▶ Starting test session with in-memory level");

        UnityEngine.SceneManagement.SceneManager
            .LoadScene("ArrowGameScene");
    }
}
