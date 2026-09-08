using System;

public class CurrencyScaleModifier
{
    public string Id;
    public float Value = 1f;
    public bool IsActive = true;

    public CurrencyScaleModifier(string id, float targetValue)
    {
        Id = id;
        Value = targetValue;
    }

    public void SetValue(float v)
    {
        Value = v;
    }
}