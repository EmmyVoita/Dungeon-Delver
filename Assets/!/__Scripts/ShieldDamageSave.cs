using System;
using UnityEngine;
public class ShieldDamageSave : IDamageSave
{
    public static event Action<ShieldDamageSave> OnDamagePrevented;
    public int Priority => 100;
    public bool RemoveAtLevelEnd => true;
    
    private bool _preventComboBreak;
    private bool _notify;
    private float _duration;
    private float _startTime;
    private float _stopTimeDuration;

    public ShieldDamageSave(bool preventComboBreak, bool notify = false, float duration = -1, float stopTimeDuration = -1)
    {
        _preventComboBreak = preventComboBreak;
        _notify = notify;
        _duration = duration;
        _startTime = Time.time;
        _stopTimeDuration = stopTimeDuration;
    }

    public bool CanPreventDamage(int damage)
    {
        bool durationCheck = _duration == -1 || Time.time <= _startTime + _duration;
        return damage > 0 && durationCheck;
    }

    public bool PreventDamage(int damage)
    {
        Player.Instance.AddHitBlock(1);

        if(_notify)
            OnDamagePrevented?.Invoke(this);

        if(_stopTimeDuration != -1)
        {
            TimeScaleModifier modifier = new TimeScaleModifier("StopTime", 0.1f);
            TimeManager.Instance.AddTemporaryModifier(modifier,_stopTimeDuration);
        }
            
        
        return true;
    }

    public IDamageSave Clone()
    {
        return new ShieldDamageSave(_preventComboBreak);
    }
}