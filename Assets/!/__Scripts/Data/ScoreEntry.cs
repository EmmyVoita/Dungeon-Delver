[System.Serializable]
public struct ScoreEntry
{
    public ScoreSource source;
    public int amount;

    public ScoreEntry(ScoreSource source, int amount)
    {
        this.source = source;
        this.amount = amount;
    }
}
