using UnityEngine;

public class SceneStateInitializer : MonoBehaviour
{
    [SerializeField]
    private GameState initialState;
    
    [Header("References")]
    [SerializeField] private BackgroundVisualManager visualManager;

    private void Start()
    {
        ApplyRunSettings();

        GameStateManager.Instance?.SetStateForceUpdate(initialState);

        if (RoundManager.Instance && 
            !BackgroundPreference.UseSimpleBackground)
        {
            visualManager.ChangeDefaultBackground(
                RoundManager.Instance.stages[0].backgroundData
            );
        }
    }

    private void ApplyRunSettings()
    {
        GameSceneConfig config = GameSessionBootstrap.Config;

        if (config == null || !TimeManager.Instance || !ScoreManager.Instance || !CurrencyManager.Instance)
            return;

        TimeManager.Instance.AddRunModifier(
            new TimeScaleModifier(
                "GameSpeed",
                config.GameSpeedMultiplier
            )
        );

        ScoreManager.Instance.AddScoreModifier(
            new ScoreScaleModifier(
                "GameSpeed_Score",
                config.ScoreMultiplier
            )
        );
        
        CurrencyManager.Instance.AddCurrencyModifier(
            new CurrencyScaleModifier(
                "GameSpeed_Currency",
                config.CurrencyMultiplier
            )
        );
    }
}