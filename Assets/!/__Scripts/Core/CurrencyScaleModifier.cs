using System;

public class CurrencyScaleModifier
{
    public float Value { get; private set; }
    public bool IsActive { get; private set; }

    public CurrencyScaleModifier(float value)
    {
        Value = value;
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }
}