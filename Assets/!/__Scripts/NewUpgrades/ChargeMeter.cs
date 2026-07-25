using System;

public class ChargeMeter
{
    public int CurrentCharge { get; private set; }
    public int MaxCharge { get; }

    public float NormalizedCharge =>
        MaxCharge <= 0 ? 0f : (float)CurrentCharge / MaxCharge;

    public event Action<int, int> OnChargeChanged;
    public event Action OnMeterFilled;

    public ChargeMeter(int maxCharge)
    {
        MaxCharge = Math.Max(1, maxCharge);
    }

    public bool AddCharge(int amount = 1)
    {
        if (amount <= 0)
            return false;

        CurrentCharge += amount;

        bool filled = CurrentCharge >= MaxCharge;

        if (filled)
        {
            CurrentCharge -= MaxCharge;
            OnMeterFilled?.Invoke();
        }

        OnChargeChanged?.Invoke(CurrentCharge, MaxCharge);

        return filled;
    }

    public void Reset()
    {
        CurrentCharge = 0;
        OnChargeChanged?.Invoke(CurrentCharge, MaxCharge);
    }
}