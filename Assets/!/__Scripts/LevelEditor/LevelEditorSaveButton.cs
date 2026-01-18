using UnityEngine;
using UnityEngine.UI;

public class LevelEditorSaveButton : MonoBehaviour
{
    public Button saveButton;

    void Start()
    {
#if UNITY_EDITOR
        saveButton.onClick.AddListener(EditorSave);
#endif
    }

#if UNITY_EDITOR
    void EditorSave()
    {
        LevelEditorSaveUtility.SaveToTextAsset(
            LevelEditorData.Instance,
            LevelEditorData.Instance.currentLevelAsset
        );

        Debug.Log("💾 Saved edited level file.");
    }
#endif
}
