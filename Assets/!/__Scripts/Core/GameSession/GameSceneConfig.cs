using UnityEngine;

public class GameSceneConfig
{
    public GameMode Mode { get; }
    public float LevelEditorStartTime { get; }
    public ObstacleTypeDefinition PracticeObstacle { get; }
    public MenuState ReturnTarget { get; }

    // Default = Standard Run
    public GameSceneConfig() : this(GameMode.StandardRun) { }

    public GameSceneConfig(
        GameMode mode,
        float startTime = 0,
        ObstacleTypeDefinition practiceObstacle = null,
        MenuState returnTarget = MenuState.Main)
    {
        Mode = mode;
        LevelEditorStartTime = startTime;
        PracticeObstacle = practiceObstacle;
        ReturnTarget = returnTarget;
    }

    // Convenience helpers
    public bool IsEditorMode =>
        Mode == GameMode.LevelEditorTest;

    public bool IsPracticeMode =>
        Mode == GameMode.ObstaclePractice || Mode == GameMode.ObstaclePracticeBoss;
}