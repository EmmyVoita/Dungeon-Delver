using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System;

public class LevelEditorSessionController : MonoBehaviour
{
    public static event Action OnLoadLevelPressed;
    const string LEVEL_ROOT = "Levels/";

    [Header("UI References")]
    public TMP_InputField levelFileInput;
    public Button loadLevelButton;




    void Start()
    {
        loadLevelButton.onClick.AddListener(HandleLoadLevelPressed);

        // ======================================================
        // 🔙 RETURNING FROM TEST SESSION
        // ======================================================
        if (TestSession.runSingleLevel)
        {
            TestSession.runSingleLevel = false;

            if (TestSession.tempLevelAsset != null)
            {
                LevelEditorData.Instance.LoadLevelFromText(TestSession.tempLevelAsset.text);
            }

            if (TestSession.originalLevelAsset != null)
            {
                LevelEditorData.Instance.currentLevelAsset =TestSession.originalLevelAsset;
                levelFileInput.text =TestSession.originalLevelAsset.name;
            }

           

            LevelTimelineUI.Instance.BuildTimeline();
            EditorPlaybackController.Instance.BuildSimulatedArrows();
            EditorPlaybackController.Instance.Stop();
            EditorPlaybackController.Instance.JumpToTime(TestSession.levelTime);
        }
    }

    // ------------------------------------------------------
    // Load level normally
    // ------------------------------------------------------
    private void HandleLoadLevelPressed()
    {
        OnLoadLevelPressed?.Invoke();

        string assetName = levelFileInput.text.Trim();
        if (string.IsNullOrEmpty(assetName))
        {
            UIToast.Error("No level name entered");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        TextAsset level = Resources.Load<TextAsset>(LEVEL_ROOT + assetName);
        if (level == null)
        {
            UIToast.Error($"Could not find TextAsset: {LEVEL_ROOT + assetName}");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        LevelEditorData.Instance.LoadLevelFromText(level.text);
        LevelEditorData.Instance.currentLevelAsset = level;

        LevelTimelineUI.Instance.BuildTimeline();
        EditorPlaybackController.Instance.BuildSimulatedArrows();
        EditorPlaybackController.Instance.Stop();
        EditorPlaybackController.Instance.JumpToTime(0);

        if(UIToast.Instance != null) UIToast.Show($"Loaded level asset: {level.name}");
    }

    // ------------------------------------------------------
    // ▶ TEST LEVEL
    // ------------------------------------------------------
    public void OnClick_TestLevel()
    {
        if (LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to test");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        TestSession.levelMusic = EditorPlaybackController.Instance.LevelEditorTestMusic;

        SetLevelAsset();

        TestSession.runSingleLevel = true;
        TestSession.returnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        TestSession.levelTime = 0;

        GameSceneLoader.PendingConfig = new GameSceneConfig(GameMode.LevelEditorTest,0,null);
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.ArrowGameScene);
    }



    public void OnClick_TestLevelFromPosition()
    {
        if (LevelEditorData.Instance.currentLevelAsset == null)
        {
            UIToast.Error("No level loaded to test");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);
            return;
        }

        TestSession.levelMusic = EditorPlaybackController.Instance.LevelEditorTestMusic;



        SetLevelAsset();
        TestSession.runSingleLevel = true;
        TestSession.returnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        TestSession.levelTime = EditorPlaybackController.Instance.CurrentTime;

   
        GameSceneLoader.PendingConfig = new GameSceneConfig(GameMode.LevelEditorTest,EditorPlaybackController.Instance.CurrentTime,null);
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.ArrowGameScene);
    }

    private void SetLevelAsset()
    {
        // Save original
        TestSession.originalLevelAsset = LevelEditorData.Instance.currentLevelAsset;

        // Create a TEMP runtime TextAsset from editor state
        string serializedLevel = LevelEditorData.Instance.SerializeToString();

        TestSession.tempLevelAsset = new TextAsset(serializedLevel);
    }
}
