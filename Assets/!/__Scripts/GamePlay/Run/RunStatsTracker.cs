using UnityEngine;

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

    public float RunAccuracy => TotalSpawned == 0 ? 0f : (float)TotalHit / TotalSpawned;
    public float RunCritRate => TotalHit == 0 ? 0f : (float)TotalCrit / TotalHit;

    public void AddRound(RoundStatsTracker round)
    {
        TotalScore += round.Score;
        TotalSpawned += round.Spawned;
        TotalHit += round.Hit;
        TotalMissed += round.Missed;
        TotalCrit += round.Crit;
        TotalDamageTaken += round.DamageTaken;
        HighestCombo = Mathf.Max(HighestCombo, round.HighestCombo);

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