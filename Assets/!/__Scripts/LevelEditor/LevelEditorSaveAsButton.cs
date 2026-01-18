using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorSaveAsButton : MonoBehaviour
{
    [SerializeField] private Button saveAsButton;

    void Start()
    {
#if UNITY_EDITOR
        saveAsButton.onClick.AddListener(OnSaveAsClicked);
#else
        saveAsButton.gameObject.SetActive(false); // hide in builds
#endif
    }

#if UNITY_EDITOR
    private void OnSaveAsClicked()
    {
        string defaultName = "NewLevel.txt";

        string path = UnityEditor.EditorUtility.SaveFilePanel(
            "Save Level As",
            Application.streamingAssetsPath,
            defaultName,
            "txt"
        );

        if (string.IsNullOrEmpty(path))
            return;

        Debug.Log("Saving level as: " + Path.GetFileName(path));

        LevelEditorSaveUtility.SaveAsNewTextAsset(
            LevelEditorData.Instance,
            path
        );
    }
#endif
}
