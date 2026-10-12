using TMPro;
using UnityEngine;

public class TimeArrowReward : ICoinCollectEffect
{
    public int Priority => 0;

    //public int RequiredInstances => _requiredInstances;
    public float TimeWindow => _timeWindow;
    public bool RemoveAtLevelEnd => false;

    //private readonly int _requiredInstances;
    private readonly int _amount;
    private readonly float _timeWindow;
    private readonly int _currencyCost;
    private readonly int _chargeRequired;
    private readonly SoundEffect _chargeSound;
    private readonly SoundEffect _fullSound;
    private readonly Sprite _meterIcon;

    public ChargeMeterHandle _meter;

    public TimeArrowReward(
        int chargeRequired,
        int currencyCost,
        SoundEffect chargeSound,
        SoundEffect fullSound,
        Sprite meterIcon,
        int amount)
    {
        //_requiredInstances = requiredInstances;
        _currencyCost = currencyCost;
        _chargeRequired = chargeRequired;

        _chargeSound = chargeSound;
        _fullSound = fullSound;

        _meterIcon = meterIcon;
        _amount =  amount;
    }

    public bool CanTriggerEffect(int amount)
    {
        return true;
    }

    public bool TriggerEffect(int amount)
    {
        if (_meter == null)
            return false;

        if (_meter.IsLocked)
            return false;

        bool full = _meter.AddCharge();

        if (!full)
        {
            PlaySound(_chargeSound);
            return false;
        }

        // Stop currency gained during the animation from adding charge.
        _meter.Lock();

        // Immediately reset the logical meter.
        _meter.Consume();

        //BuffHelpers.GetOrCreateRecoveryArrow(1);
        BuffHelpers.GetOrCreateTimeSlowArrow(_amount);
        PlaySound(_fullSound);

        return true;
    }

    private void PlaySound(SoundEffect sound)
    {
        AudioHelpers.PlaySoundEffect(
            sound,
            Camera.main.transform.position
        );
    }

    public IRuntimeModifier Clone()
    {
        return new TimeArrowReward(
            //_requiredInstances,
            _chargeRequired,
            _currencyCost,
            _chargeSound,
            _fullSound,
            _meterIcon,
            _amount
        );
    }

    public void Destroy()
    {
        if(_meter != null)
            ChargeMeterManager.Instance.RemoveMeter(_meter.Id);

        _meter.View.OnFullAnimationCompleted -= HandleFullAnimationCompleted;
    }

    public void OnActivate()
    {
        _meter = ChargeMeterManager.Instance.CreateMeter(
            "Recovery Arrow",
            _meterIcon,
            _chargeRequired
        );

        _meter.View.OnFullAnimationCompleted += HandleFullAnimationCompleted;
    }

    private void HandleFullAnimationCompleted()
    {
        if (_meter == null)
            return;

        _meter.Unlock();
    }
}