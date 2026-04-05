using UnityEngine;

public abstract class AbilityUpgradeBase : ScriptableObject
{
    public string upgradeName;
    public string description;
    public abstract void ApplyToAbility(AbilityBase ability);
}
