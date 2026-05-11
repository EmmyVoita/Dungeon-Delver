using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class LevelEditorUIController : MonoBehaviour
{
    const string LEVEL_ROOT = "Levels/";

    [Header("UI References")]
    public TMP_InputField levelFileInput;
    public Button loadLevelButton;


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

                LevelEditorData.Instance.LoadLevelFromText(
                    TestSession.tempLevelAsset.text
                );
            }

            if (TestSession.originalLevelAsset != null)
            {
                LevelEditorData.Instance.currentLevelAsset =
                    TestSession.originalLevelAsset;

                levelFileInput.text =
                    TestSession.originalLevelAsset.name;
            }

            LevelTimelineUI.Instance.BuildTimeline();
            EditorPlaybackController.Instance.BuildSimulatedArrows();
            EditorPlaybackController.Instance.Stop();
            EditorPlaybackController.Instance.JumpToTime(0);

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

        LevelEditorData.Instance.LoadLevelFromText(level.text);
        LevelEditorData.Instance.currentLevelAsset = level;

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.BuildSimulatedArrows();
        EditorPlaybackController.Instance.Stop();
        EditorPlaybackController.Instance.JumpToTime(0);

        if(UIToast.Instance != null) UIToast.Show($"✅ Loaded level asset: {level.name}");
        AudioSettingsManager.PlayGeneralButtonSound();
    }

    // ------------------------------------------------------
    // ▶ TEST LEVEL
    // ------------------------------------------------------
    public void OnClick_TestLevel()
    {
        if (LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to test");
            AudioSettingsManager.PlayNegativeUISound();
            return;
        }

        TestSession.levelMusic = EditorPlaybackController.Instance.LevelEditorTestMusic;

        // Save original
        TestSession.originalLevelAsset =
           LevelEditorData.Instance.currentLevelAsset;

        // Create a TEMP runtime TextAsset from editor state
        string serializedLevel =
            LevelEditorData.Instance.SerializeToString();

        TestSession.tempLevelAsset =
            new TextAsset(serializedLevel);

        TestSession.runSingleLevel = true;
        TestSession.returnScene =
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        GameSceneLoader.PendingConfig = new GameSceneConfig(
            GameMode.LevelEditorTest,
            0,
            null);
   

        Debug.Log("▶ Starting test session with in-memory level");

        UnityEngine.SceneManagement.SceneManager
            .LoadScene("ArrowGameScene");
    }



    public void OnClick_TestLevelFromPosition()
    {
        if (LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to test");
            AudioSettingsManager.PlayNegativeUISound();
            return;
        }

        TestSession.levelMusic = EditorPlaybackController.Instance.LevelEditorTestMusic;



        // Save original
        TestSession.originalLevelAsset =
           LevelEditorData.Instance.currentLevelAsset;

        // Create a TEMP runtime TextAsset from editor state
        string serializedLevel =
            LevelEditorData.Instance.SerializeToString();

        TestSession.tempLevelAsset =
            new TextAsset(serializedLevel);

        TestSession.runSingleLevel = true;
        TestSession.returnScene =
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

   
        GameSceneLoader.PendingConfig = new GameSceneConfig(
            GameMode.LevelEditorTest,
            EditorPlaybackController.Instance.CurrentTime,
            null);

        Debug.Log("▶ Starting test session with in-memory level");

        UnityEngine.SceneManagement.SceneManager
            .LoadScene("ArrowGameScene");
    }
}
