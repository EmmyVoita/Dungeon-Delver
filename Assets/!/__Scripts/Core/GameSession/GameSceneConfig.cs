using UnityEngine;

public class GameSceneConfig
{
    public GameMode Mode { get; }
    public float LevelEditorStartTime { get; }
    public ObstacleTypeDefinition PracticeObstacle { get; }
    public MenuState ReturnTarget { get; }

    public float GameSpeedMultiplier { set; get; }
    public float ScoreMultiplier { set; get; }
    public float CurrencyMultiplier { set; get; }
    public int RerollsToGrant { set; get; }
    public int CardsToGrant { set; get; }
    public int AdditionalDamageModifier { set; get; }
    public int AdditionalHealingModifier {set; get;}
    public int EndOfRoundHealModifier {set; get;}
    public int AdditionalRewardRoutes {set; get;}
    public int AdditionalAbilityDamage {set; get;}
    public int StartingCurrency {set; get;}
    public float NormalArrowAbilityChargeRoll {set; get;}
    public float StartingAbilityCharge {set; get;}


    // Default = Standard Run
    public GameSceneConfig() : this(GameMode.StandardRun) { }

    public GameSceneConfig(
        GameMode mode,
        float startTime = 0,
        ObstacleTypeDefinition practiceObstacle = null,
        MenuState returnTarget = MenuState.Main,
        float gameSpeedMultiplier = 1.0f,
        float scoreMultiplier = 1.0f,
        float currencyMultiplier = 1.0f)
    {
        Mode = mode;
        LevelEditorStartTime = startTime;
        PracticeObstacle = practiceObstacle;
        ReturnTarget = returnTarget;
        GameSpeedMultiplier = gameSpeedMultiplier;
        ScoreMultiplier = scoreMultiplier;
        CurrencyMultiplier = currencyMultiplier;
    }

    // Convenience helpers
    public bool IsEditorMode =>Mode == GameMode.LevelEditorTest;
    public bool IsPracticeMode => Mode == GameMode.ObstaclePractice || Mode == GameMode.ObstaclePracticeBoss;
}