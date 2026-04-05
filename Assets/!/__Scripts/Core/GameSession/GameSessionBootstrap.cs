using UnityEngine;

public class GameSessionBootstrap : MonoBehaviour
{
    public static GameSceneConfig Config { get; private set; }

    private void Awake()
    {
        Config = GameSceneLoader.PendingConfig ?? new GameSceneConfig();

        GameSceneLoader.PendingConfig = null; // prevent leakage

    }

    private void Start()
    {
        ApplySessionConfig();
    }

    private void ApplySessionConfig()
    {
        switch (Config.Mode)
        {
            case GameMode.StandardRun:
                GameStateManager.Instance.SetState(GameState.RunIntro);
                break;

            case GameMode.ObstaclePractice:
                GameStateManager.Instance.SetState(GameState.Practice);
                break;

            case GameMode.LevelEditorTest:
                GameStateManager.Instance.SetState(GameState.Editor);
                break;

            case GameMode.LevelEdtiorPlayFromPosition:
                GameStateManager.Instance.SetState(GameState.Editor);
                break;

            case GameMode.Tutorial:
                GameStateManager.Instance.SetState(GameState.Tutorial);
                break;
        }
    }
}