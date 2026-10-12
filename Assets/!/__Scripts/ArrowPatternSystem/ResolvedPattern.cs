public class ResolvedPattern
{
    public float StartBeat { get; }
    public ArrowPattern Pattern { get; }

    public ResolvedPattern(float startBeat,ArrowPattern pattern)
    {
        StartBeat = startBeat;
        Pattern = pattern;
    }
}