using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Projectile/Freeze")]
public class ProjectileShieldUpgrade_Freeze : AbilityUpgradeBase
{
    public float duration = 2f;

    public override void ApplyToAbility(AbilityBase ability)
    {
        if (ability is ProjectileShieldAbility shield)
        {
            shield.EnableFreeze(duration);
        }
    }
}
