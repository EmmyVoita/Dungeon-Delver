using UnityEngine;

public class RelicRegenerationEffect : ITimedEffect
{
    public int Priority => 0;

    private readonly float _regenerationInterval;
    private float _elapsedTime;
    private int _currencyAmount;

    public RelicRegenerationEffect(int currencyAmount, float regenerationInterval)
    {
        _regenerationInterval = Mathf.Max(0.1f, regenerationInterval);
        _currencyAmount = currencyAmount;
    }

    public void Tick(float deltaTime)
    {
        if (OrbitingTreasureController.Instance == null)
            return;

        // Don't build timer progress while already full.
        if (OrbitingTreasureController.Instance.IsAtMaxRelics)
        {
            _elapsedTime = 0f;
            return;
        }

        _elapsedTime += deltaTime;

        if (_elapsedTime < _regenerationInterval)
            return;

        _elapsedTime -= _regenerationInterval;

        OrbitingTreasureController.Instance.AddRelics(1, _currencyAmount);
    }

    public IRuntimeModifier Clone()
    {
        return new RelicRegenerationEffect(
            _currencyAmount,
            _regenerationInterval
        );
    }

    public void OnActivate()
    {
        _elapsedTime = 0f;
    }

    public void Destroy()
    {
    }
}