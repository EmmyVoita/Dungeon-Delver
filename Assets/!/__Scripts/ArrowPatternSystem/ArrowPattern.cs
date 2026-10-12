using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArrowPattern",menuName = "Violet Rush/Arrow Pattern")]
public class ArrowPattern : ScriptableObject
{
    [Header("Pattern")]
    [SerializeField] private PatternType patternType;
    [SerializeField] private int lengthInBeats = 8;

    [Header("Arrows")]
    [SerializeField] private List<ArrowSpawnData> arrows = new();

    public PatternType PatternType => patternType;
    public int LengthInBeats => lengthInBeats;
    public IReadOnlyList<ArrowSpawnData> Arrows => arrows;
}