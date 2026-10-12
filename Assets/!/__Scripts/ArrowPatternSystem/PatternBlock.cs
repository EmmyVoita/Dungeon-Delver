using System;
using UnityEngine;

[Serializable]
public class PatternBlock
{
    [SerializeField] private float startBeat;
    [SerializeField] private int lengthInBeats = 8;
    [SerializeField] private PatternType patternType;

    public float StartBeat => startBeat;
    public int LengthInBeats => lengthInBeats;
    public PatternType PatternType => patternType;
}