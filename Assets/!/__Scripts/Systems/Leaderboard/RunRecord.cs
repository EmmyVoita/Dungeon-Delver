
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class RunRecord
{
    public int score;
    public string timestamp;
    public string causeOfDeath;
    public string levelIndex;
    
    // Upgrades
    public List<UpgradeRecord> upgrades; 

    // Route Records
    public List<RouteRecord> routeRecords = new();

    // Combo
    public int highestCombo;

    // Currency
    public int totalCurrency;

    // Ability
    public AbilityType abilityUsed;
    public int totalAbilityCharge;
    public int totalAbilityUsed;

    // Arrow Resolved Data
    public int totalHit;
    public int totalSpawned;
    public int totalcrit;

    // Accuracy
    public float accuracy;
    public float critAccuracy;

    // Rerolls
    public int totalRerollsGained;
    public int totalRerollsUsed;

    // Shop Data
    public int totalShopsVisited;

    // Healing / Damage
    public int totalHealthHealed;
    public int damageTaken;
    
    // Derived flags
    public bool noDamageRun;

    public int CardCount => upgrades.Sum(i => i.count);

    public override string ToString()
    {
        return $"[{timestamp}] Score: {score:N0}, Ability: {abilityUsed}, Damage: {damageTaken}, NoDamage: {noDamageRun}";
    }
}