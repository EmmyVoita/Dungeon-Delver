using UnityEngine;

public interface IDamageSave : IRuntimeModifier
{
    public GameObject ShieldPrefab {get;}
    bool CanPreventDamage(int incomingDamage);
    bool PreventDamage(int incomingDamage);
    bool RemoveAtLevelEnd { get; }
    bool HasExpired();
}