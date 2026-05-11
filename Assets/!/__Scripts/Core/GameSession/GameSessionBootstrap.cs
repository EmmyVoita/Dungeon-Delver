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
                GameStateManager.Instance.SetStateForceUpdate(GameState.RunIntro);
                break;

            case GameMode.ObstaclePractice:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Practice);
                break;

            case GameMode.ObstaclePracticeBoss:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Practice);
                break;

            case GameMode.LevelEditorTest:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Editor);
                break;

            case GameMode.Tutorial:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Tutorial);
                break;
        }
    }
}