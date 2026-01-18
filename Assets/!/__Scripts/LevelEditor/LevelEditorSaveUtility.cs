#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class LevelEditorSaveUtility
{
    /// <summary>
    /// Saves the current editor level data back into a TextAsset.
    /// </summary>
    public static void SaveToTextAsset(
        LevelEditorData editorData,
        TextAsset targetAsset)
    {
        if (editorData == null)
        {
            if(UIToast.Instance != null)  UIToast.Error("❌ EditorData is null");
            return;
        }

        if (targetAsset == null)
        {
            if(UIToast.Instance != null)  UIToast.Error("❌ Target TextAsset is null");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(targetAsset);
        if (string.IsNullOrEmpty(assetPath))
        {
            if(UIToast.Instance != null)  UIToast.Error("❌ Could not resolve asset path");
            return;
        }

        string serialized = editorData.SerializeToString();

        File.WriteAllText(assetPath, serialized);
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();

        if(UIToast.Instance != null)  UIToast.Show($"💾 Level saved to TextAsset: {assetPath}");
    }

    /// <summary>
    /// Saves the level as a new TextAsset.
    /// </summary>
    public static TextAsset SaveAsNewTextAsset(
        LevelEditorData editorData,
        string assetPath)
    {
        if (!assetPath.EndsWith(".txt"))
            assetPath += ".txt";

        string serialized = editorData.SerializeToString();
        File.WriteAllText(assetPath, serialized);

        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();

        TextAsset asset =
            AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

        if(UIToast.Instance != null)  UIToast.Show($"💾 Level saved as new TextAsset: {assetPath}");
        return asset;
    }
}
#endif
