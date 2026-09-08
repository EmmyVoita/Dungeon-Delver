
using System.Collections.Generic;
using System.Linq;

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
    public string causeOfDeath;
    public string levelIndex;
    public int totalCurrency;
    public int totalAbilityCharge;
    public int totalAbilityUsed;
    public int totalHit;
    public int totalSpawned;
    public int totalcrit;

    // Rerolls
    public int totalRerollsGained;
    public int totalRerollsUsed;

    // Shop Data
    public int totalShopsVisited;

    // Healing
    public int totalHealthHealed;
    


    // Derived flags
    public bool noDamageRun;

    public int CardCount => upgrades.Sum(i => i.count);

    public override string ToString()
    {
        return $"[{timestamp}] Score: {score:N0}, Ability: {abilityUsed}, Damage: {damageTaken}, NoDamage: {noDamageRun}";
    }
}