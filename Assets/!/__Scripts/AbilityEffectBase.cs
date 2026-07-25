using System;
using UnityEngine;

public abstract class AbilityEffectBase : MonoBehaviour
{
    public event Action<AbilityEffectBase> OnEffectEnded;
    
    protected AbilityEffectContext Context { get; private set; }

    private bool _hasEnded;

    public virtual void Activate(AbilityEffectContext context)
    {
        Context = context;
        _hasEnded = false;
    }

    protected void EndEffect()
    {
        if(_hasEnded)
            return;


        _hasEnded = true;
        OnEffectEnded?.Invoke(this);
    }

    protected virtual void OnDestory()
    {
        if(!_hasEnded)
        {
            _hasEnded = true;
            OnEffectEnded?.Invoke(this);
        }
    }
}