using System;

public static class PatternTypeExtensions
{
    public static bool TryParsePatternType(this string value,out PatternType patternType)
    {
        return Enum.TryParse(value, true, out patternType);
    }
}