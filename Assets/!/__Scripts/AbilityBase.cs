using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [HideInInspector] public Player player;
    public int abilityBaseCost = 10;
    public AudioClip activateSound;


    [Header("Ability-Specific Upgrades")]
    public List<UpgradeCard> abilitySpecificUpgrades = new List<UpgradeCard>();
    protected List<AbilityUpgradeBase> activeUpgrades = new List<AbilityUpgradeBase>();

    public abstract void Activate(Quaternion rotation);

    public virtual void ApplyUpgrade(AbilityUpgradeBase upgrade)
    {
        activeUpgrades.Add(upgrade);
        upgrade.ApplyToAbility(this);
    }
}
