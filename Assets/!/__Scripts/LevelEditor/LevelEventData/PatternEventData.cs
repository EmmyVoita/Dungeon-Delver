using UnityEngine;
using System;

public class PatternEventData : LevelEventData
{
    public PatternType _patternType;
    public int _lengthInBeats;
    public float _speed;

    public PatternEventData(float beatTime, PatternType type, int lengthInBeats, float arrowSpeed) : base(beatTime)
    {
        _patternType = type;
        _lengthInBeats = lengthInBeats;
        _speed = arrowSpeed;
    }

    public override string Serialize()
    {
        return
            $"{beatTime},pattern," +
            $"{_patternType.ToString().ToLowerInvariant()}," +
            $"{_lengthInBeats}," +
            $"{_speed}";
    }

    public override string Describe()
    {
        return
            $"Pattern @ {beatTime:0.##} beats" +
            $"\nType: {_patternType}" +
            $"\nLength: {_lengthInBeats} beats" +
            $"\nSpeed: {_speed}";
    }
}