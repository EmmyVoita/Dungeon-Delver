using UnityEngine;

public class GameSceneConfig
{
    public GameMode Mode { get; }
    public float LevelEditorStartTime { get; }
    public ObstacleTypeDefinition PracticeObstacle { get; }
    public JumpDirectionMode DirectionMode { get; }

    // Default = Standard Run
    public GameSceneConfig() : this(GameMode.StandardRun) { }

    public GameSceneConfig(
        GameMode mode,
        float startTime = 0,
        ObstacleTypeDefinition practiceObstacle = null,
        JumpDirectionMode directionMode = JumpDirectionMode.FourDirectional)
    {
        Mode = mode;
        LevelEditorStartTime = startTime;
        PracticeObstacle = practiceObstacle;
        DirectionMode = directionMode;
    }

    // Convenience helpers
    public bool IsEditorMode =>
        Mode == GameMode.LevelEditorTest ||
        Mode == GameMode.LevelEdtiorPlayFromPosition;

    public bool IsPracticeMode =>
        Mode == GameMode.ObstaclePractice;
}