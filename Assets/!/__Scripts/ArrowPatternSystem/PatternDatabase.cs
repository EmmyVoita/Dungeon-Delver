using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternDatabase",menuName = "Violet Rush/Pattern Database")]
public class PatternDatabase : ScriptableObject
{
    [SerializeField]
    private List<ArrowPattern> patterns = new();

    public List<ArrowPattern> GetPatterns(PatternType type,int lengthInBeats)
    {
        return patterns.Where(pattern =>pattern.PatternType == type && pattern.LengthInBeats == lengthInBeats).ToList();
    }

    public ArrowPattern GetRandomPattern(PatternType type,int lengthInBeats)
    {
        List<ArrowPattern> validPatterns = GetPatterns(type, lengthInBeats);

        if (validPatterns.Count == 0)
        {
            Debug.LogWarning($"No pattern found for {type} / {lengthInBeats} beats.");
            return null;
        }

        return validPatterns[Random.Range(0, validPatterns.Count)];
    }
}