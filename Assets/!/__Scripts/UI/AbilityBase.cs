using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    public event Action<AbilityBase> OnAbilityStarted;
    public event Action<AbilityBase> OnAbilityEnded;

    public AbilityData Data { get; private set; }

    public bool IsActive {get; private set;}

    protected readonly List<AbilityUpgradeBase> activeUpgrades = new();

    public virtual void Initialize(AbilityData data)
    {
        Data = data;
    }

    protected float GetModifiedDuration()
    {
        float baseDuration = Data != null ? Data.baseDuration : 0f;

        if (AbilityDurationManager.Instance == null)
            return baseDuration;

        return AbilityDurationManager.Instance.GetModifiedDuration(baseDuration);
    }

    public bool TryActive(Quaternion rotation)
    {
        if(IsActive)
            return false;

        IsActive = true;
        OnAbilityStarted?.Invoke(this);
        Activate(rotation);

        return true;
    }

    public virtual void Activate(Quaternion rotation)
    {
        AudioHelpers.PlaySoundEffect(Data.activationSound, Player.Instance.transform.position);
    }

    protected void EndAbility()
    {
        if(!IsActive)
            return;

        IsActive = false;
        OnAbilityEnded?.Invoke(this);
    }

    public virtual void ApplyUpgrade(AbilityUpgradeBase upgrade)
    {
        if (upgrade == null)
            return;

        activeUpgrades.Add(upgrade);
        upgrade.ApplyToAbility(this);
    }
}