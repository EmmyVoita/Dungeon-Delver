using System;
using System.Collections;


public class TimeScaleModifier
{
    public string Id;
    public float Value = 1f;
    public bool IsActive = true;

    public Action OnChanged;

    public TimeScaleModifier(string id, float targetValue)
    {
        Id = id;
        Value = targetValue;
    }

    public void SetValue(float v)
    {
        Value = v;
        OnChanged?.Invoke(); // 🔥 notify
    }
}