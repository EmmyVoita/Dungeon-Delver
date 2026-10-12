using System;
using UnityEngine;
public class ShieldDamageSave : IDamageSave
{
    public static event Action<ShieldDamageSave> OnDamagePrevented;
    public int Priority => 100;
    public bool RemoveAtLevelEnd => true;
    public GameObject ShieldPrefab { get; }
    
    private bool _preventComboBreak;
    private bool _notify;
    private float _duration;
    private float _startTime;
    private float _stopTimeDuration;
    private int _abilityChargeAmount;

    public ShieldDamageSave(bool preventComboBreak,
                            GameObject shieldPrefab = null,
                            bool notify = false, 
                            float duration = -1, 
                            float stopTimeDuration = -1, 
                            int abilityChargeAmount = 0)
    {
        _preventComboBreak = preventComboBreak;
        _notify = notify;
        _duration = duration;
        _startTime = Time.time;
        _stopTimeDuration = stopTimeDuration;
        _abilityChargeAmount = abilityChargeAmount;
        ShieldPrefab = shieldPrefab;
    }

    public bool HasExpired()
    {
        return _duration >= 0f && Time.time > _startTime + _duration;
    }

    public bool CanPreventDamage(int damage)
    {
        return damage > 0 && !HasExpired();
    }

    public bool PreventDamage(int damage)
    {
         Player.Instance.AbilityCharge += _abilityChargeAmount;
        Player.Instance.AddHitBlock(1);

       

        ComboManager.Instance._ignoreNextMissCount++;

        if(_notify)
            OnDamagePrevented?.Invoke(this);

        if(_stopTimeDuration != -1)
        {
            TimeScaleModifier modifier = new TimeScaleModifier("StopTime", 0.1f);
            TimeManager.Instance.AddTemporaryModifier(modifier,_stopTimeDuration);
        }
            
        
        return true;
    }

    public IRuntimeModifier Clone()
    {
        return new ShieldDamageSave(_preventComboBreak);
    }

    public void Destroy()
    {
        
    }

     public void OnActivate()
    {
        
    }
}