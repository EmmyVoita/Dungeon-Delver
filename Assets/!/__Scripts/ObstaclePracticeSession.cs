public enum JumpDirectionMode
{
    FourDirectional,
    EightDirectional
}

public enum GameMode
{
    StandardRun,
    ObstaclePractice,
    LevelEditorTest,
    LevelEdtiorPlayFromPosition
}

public static class GameSceneLoader
{
    public static GameSceneConfig PendingConfig;
}


public class GameSceneConfig
{
    public GameMode Mode = GameMode.StandardRun;
    public float levelEditorStartTime;
    public ObstacleDefinition PracticeObstacle;
    public JumpDirectionMode DirectionMode = JumpDirectionMode.FourDirectional;

    public bool ShouldStartRound => Mode == GameMode.StandardRun;
}
