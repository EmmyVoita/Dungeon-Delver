using UnityEngine;
using UnityEngine.SceneManagement;

public class RunStatsTracker : MonoBehaviour
{
    public int TotalScore { get; private set; }
    public int TotalSpawned { get; private set; }
    public int TotalHit { get; private set; }
    public int TotalMissed { get; private set; }
    public int TotalCrit { get; private set; }
    public int HighestCombo { get; private set; }
    public int TotalDamageTaken { get; private set; }
    public int RoundsPlayed { get; private set; }
    public int TotalAbilityUsed { get; private set; }
    public int TotalAbilityCharge { get; private set; }
    public int RerollsUsed { get; private set; }
    public int RerollsGained { get; private set; }
    public int TotalHealthHealed { get; private set; }
    public int TotalShopsVisited { get; private set; }


    public float RunAccuracy => TotalSpawned == 0 ? 0f : (float)TotalHit / TotalSpawned;
    public float RunCritRate => TotalHit == 0 ? 0f : (float)TotalCrit / TotalHit;

    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RunStateManager.OnRerollGained += AddRerollGained;
        RunStateManager.OnUseReroll += AddRerollUsed;
        Player.OnHeal += AddHeal;
        ShopManager.OnShopOpened += AddShopOpened;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RunStateManager.OnRerollGained -= AddRerollGained;
        RunStateManager.OnUseReroll -= AddRerollUsed;
        Player.OnHeal -= AddHeal;
        ShopManager.OnShopOpened -= AddShopOpened;
    }

    private void AddRerollGained(int amount)
    {
        RerollsGained += amount;
    }

    private void AddRerollUsed()
    {
        RerollsGained++;
    }

    private void AddHeal(int amount, bool wasFullHealth)
    {
        if(wasFullHealth)
            return;

        if(GameStateManager.Instance.CurrentState == GameState.RunLoad || GameStateManager.Instance.CurrentState == GameState.RunIntro)
            return;

        TotalHealthHealed += amount;
    }

    private void AddShopOpened(ShopType shopType)
    {
        if(shopType != ShopType.Normal)
            return;

        TotalShopsVisited++;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetRun();
    }

    public void AddRound(RoundStatsTracker round)
    {
        TotalScore += round.Score;
        TotalSpawned += round.Spawned;
        TotalHit += round.Hit;
        TotalMissed += round.Missed;
        TotalCrit += round.Crit;
        TotalDamageTaken += round.DamageTaken;
        HighestCombo = Mathf.Max(HighestCombo, round.HighestCombo);
        TotalAbilityUsed += round.AbilityUsed;
        TotalAbilityCharge += round.AbilityCharge;


        RoundsPlayed++;
    }

    public void ResetRun()
    {
        TotalScore = 0;
        TotalSpawned = 0;
        TotalHit = 0;
        TotalMissed = 0;
        TotalCrit = 0;
        RoundsPlayed = 0;
        TotalDamageTaken = 0;
        HighestCombo = 0;
        TotalAbilityUsed = 0;
        TotalAbilityCharge = 0;
        RerollsUsed = 0;
        RerollsGained = 0;
    }

    public void PrintStats()
    {
        Debug.Log(
            $"Run Stats:\n" +
            $"Total Spawned: {TotalSpawned}\n" +
            $"Total Hit: {TotalHit}\n" +
            $"Total Missed: {TotalMissed}\n" +
            $"Total Crit: {TotalCrit}\n" +
            $"Accuracy: {(RunAccuracy * 100f):F1}%\n" +
            $"Highest Combo: {HighestCombo}\n" +
            $"Total Damage Taken: {TotalDamageTaken}\n" +
            $"Rounds Played: {RoundsPlayed}"
        );
    }
}