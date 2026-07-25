using System;
using UnityEngine;

public sealed class ChargeMeterHandle
{
    public int Id { get; }
    public int CurrentCharge { get; private set; }
    public int MaxCharge { get; }

    public bool IsFull => CurrentCharge >= MaxCharge;

    internal ChargeMeterUI View { get; }

    private readonly Action<int> _removeCallback;
    private bool _disposed;

    public bool IsLocked { get; private set; }

    public void Lock()
    {
        IsLocked = true;
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    internal ChargeMeterHandle(
        int id,
        int maxCharge,
        ChargeMeterUI view,
        Action<int> removeCallback)
    {
        Id = id;
        MaxCharge = Mathf.Max(1, maxCharge);
        View = view;
        _removeCallback = removeCallback;
    }

    public bool AddCharge(int amount = 1)
    {

        if (_disposed || amount <= 0 || IsLocked)
            return IsFull;

        CurrentCharge = Mathf.Clamp(
            CurrentCharge + amount,
            0,
            MaxCharge
        );

        View?.SetCharge(CurrentCharge, MaxCharge);

        return IsFull;
    }

    public void SetCharge(int amount)
    {
        if (_disposed)
            return;

        CurrentCharge = Mathf.Clamp(amount, 0, MaxCharge);
        View?.SetCharge(CurrentCharge, MaxCharge);
    }

    public bool Consume()
    {
        if (_disposed || !IsFull)
            return false;

        CurrentCharge = 0;
        View?.SetCharge(CurrentCharge, MaxCharge);

        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _removeCallback?.Invoke(Id);
    }
}