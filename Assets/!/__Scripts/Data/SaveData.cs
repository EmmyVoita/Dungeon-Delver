using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int highScore;
    public int totalScore;
    public int timesPlayed;

    public List<RunRecord> leaderboard = new();
}
