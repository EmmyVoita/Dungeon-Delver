
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class RunRecord
{
    public int score;
    public AbilityType abilityUsed;
    public List<UpgradeRecord> upgrades; 
    public int damageTaken;
    public int highestCombo;
    public float accuracy;
    public float critAccuracy;
    public string timestamp;

    // Derived flags
    public bool noDamageRun;

    public override string ToString()
    {
        return $"[{timestamp}] Score: {score:N0}, Ability: {abilityUsed}, Damage: {damageTaken}, NoDamage: {noDamageRun}";
    }
}