using UnityEngine;

public class RoundStatsTracker : MonoBehaviour
{
    public int Spawned { get; private set; }
    public int Hit { get; private set; }
    public int Missed { get; private set; }
    public int Crit { get; private set; }

    public float RoundAccuracy => Spawned == 0 ? 0f : (float)Hit / Spawned;
    public bool PerfectRound => Spawned > 0 && Hit == Spawned;

    public float LevelProgress
    {
        get
        {
            float totalArrows = ArrowSpawner.Instance?.TotalArrowsThisRound ?? 0f;

            return totalArrows == 0f ? 0f : (float)(Hit + Missed) / totalArrows;
        }
    }

    public float CurrentLevelAccuracy
    {
        get
        {
            int attempts = Hit + Missed;

            if (attempts == 0)
                return 1f; // 100% before any arrows

            return (float)Hit / attempts;
        }
    }


    public void RegisterArrow(ArrowResolvedData data)
    {
        switch (data.goalType)
        {
            case Goal.GoalType.Miss: Missed++; break;
            case Goal.GoalType.Normal: Hit++; break;
            case Goal.GoalType.Critical:
                Hit++;
                Crit++;
                break;
        }
    }

    public void Reset()
    {
        Spawned = 0;
        Hit = 0;
        Crit = 0;
        Missed = 0;
    }

    public void AddSpawned(int count = 1)
    {
        Spawned += count;
    }
}