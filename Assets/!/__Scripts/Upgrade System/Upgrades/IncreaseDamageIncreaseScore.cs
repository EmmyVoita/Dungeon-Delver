using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Increase Damage Increase Score")]
public class IncreaseDamageIncreaseScore : UpgradeBase, IDamageModifier, IGlobalScoreMultiplier
{
    public int additionalDamage = 1;
    public float globalValueMultiplier = 1.5f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{ADDITIONAL_DAMAGE}", additionalDamage.ToString())
            .Replace("{GLOBAL_VALUE_MULTIPLIER}", (globalValueMultiplier - 1).ToString("P0"));
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
       
    }
    public int ModifyDamageTaken(int baseDamage)
    {
        return baseDamage + additionalDamage;
    }

    public float ModifyGlobalScore(float currentMultiplier)
    {
        return currentMultiplier * globalValueMultiplier;
    }
}
