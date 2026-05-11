
public struct StatValue
{
    public StatDisplayType type;

    public int value;
    public int total;
    public string text;

    public static StatValue FromInt(int v)
        => new StatValue { type = StatDisplayType.Int, value = v };

    public static StatValue FromRatio(int v, int t)
        => new StatValue { type = StatDisplayType.Ratio, value = v, total = t };

    public static StatValue FromString(string s)
        => new StatValue { type = StatDisplayType.String, text = s };

    public static StatValue FromPercent(int v)
        => new StatValue { type = StatDisplayType.Percent, value = v };
}