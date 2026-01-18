using UnityEngine;

public static class TestSession
{
    public static bool runSingleLevel = false;

    // The ORIGINAL asset the editor was editing
    public static TextAsset originalLevelAsset;

    // Runtime-generated temp asset used for testing
    public static TextAsset tempLevelAsset;

    // Scene to return to after testing
    public static string returnScene = "LevelEditorScene";
}
