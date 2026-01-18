using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/SlowTime/Wave")]
public class TimeSlowUpgrade_Wave : AbilityUpgradeBase
{

    public override void ApplyToAbility(AbilityBase ability)
    {
        if (ability is SlowTimeAbility _ability)
        {
            _ability.EnableExpandingWave();
        }
    }


}
