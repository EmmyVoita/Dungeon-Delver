using UnityEngine;

public class RunStatsTracker : MonoBehaviour
{
    public int TotalSpawned { get; private set; }
    public int TotalHit { get; private set; }
    public int TotalMissed { get; private set; }
    public int TotalCrit { get; private set; }
    public int HighestCombo { get; private set; }

    public int RoundsPlayed { get; private set; }

    public float RunAccuracy => TotalSpawned == 0 ? 0f : (float)TotalHit / TotalSpawned;

    public void AddRound(RoundStatsTracker round)
    {
        TotalSpawned += round.Spawned;
        TotalHit += round.Hit;
        TotalMissed += round.Missed;
        TotalCrit += round.Crit;

        RoundsPlayed++;
    }

    public void ResetRun()
    {
        TotalSpawned = 0;
        TotalHit = 0;
        TotalMissed = 0;
        TotalCrit = 0;
        RoundsPlayed = 0;
    }
}