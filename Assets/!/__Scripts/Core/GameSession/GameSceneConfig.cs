using UnityEngine;

public class GameSceneConfig
{
    public GameMode Mode { get; }
    public float LevelEditorStartTime { get; }
    public ObstacleTypeDefinition PracticeObstacle { get; }
    public MenuState ReturnTarget { get; }

    // Run Modifiers
    public float GameSpeedMultiplier { set; get; } = 1;
    public float ScoreMultiplier { set; get; } = 1;
    public float CurrencyMultiplier { set; get; } = 1;
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
    public float GoldenArrowChance {set; get;}
    public int AbilityChargeOnCurrency {set; get;}
    public int CritsAbilityChargeMod {set; get;}
    public int CritsLoseCurrencyAmount {set; get;}
    public bool RotateBoard {set; get;}
    public float AdditionalGeneralShopWeight {set; get;} = 0;


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
    public bool IsEditorMode =>Mode == GameMode.LevelEditorTest;
    public bool IsPracticeMode => Mode == GameMode.ObstaclePractice || Mode == GameMode.ObstaclePracticeBoss;
    public float LevelStartTime => IsEditorMode ? LevelEditorStartTime : 0;
}