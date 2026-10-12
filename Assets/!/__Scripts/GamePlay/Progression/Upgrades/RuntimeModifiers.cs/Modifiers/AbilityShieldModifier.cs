using UnityEngine;

public class AbilityShieldModifier : IAbilityEffectModifier
{
    public int Priority => 10;

    private float _duration;
    private GameObject _shieldPrefab;

    public AbilityShieldModifier(float duration, GameObject shieldPrefab)
    {
        _duration = Mathf.Max(0.1f,duration);
        _shieldPrefab = shieldPrefab;
    }

    public void TriggerEffect()
    {
        DamageSaveManager.Instance.RegisterGeneral(new ShieldDamageSave(true,shieldPrefab: _shieldPrefab, duration: _duration));
    }

    public IRuntimeModifier Clone()
    {
        return new AbilityShieldModifier(_duration, _shieldPrefab);
    }

    public void Destroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}