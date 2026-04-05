using UnityEngine;

/// <summary>
/// Central definition of all scene names used in the project.
/// </summary>
public static class SceneNames
{
    public const string MainMenu       = "MainMenuScene";
    public const string TutorialScene  = "ArrowGameScene";
    public const string ObstaclePractice = "ObstaclePracticeScene";
    public const string ArrowGameScene = "ArrowGameScene";
    public const string AbilitySelect  = "AbilitySelect";
    public const string Credits        = "Credits";

    // Optional helper: checks existence (useful for debugging)
    public static bool Exists(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }
}
