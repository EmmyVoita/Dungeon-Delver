using UnityEngine;

public class SlowTimeAbility : AbilityBase
{
    [Header("Ability Settings")]
    public SlowTimeAbilityObject slowTimeEffectPrefab;
    private bool expandingWaveEnabled;

    // Runtime flags modified by upgrades


    public override void Activate(Quaternion rotation)
    {
        if (slowTimeEffectPrefab == null)
            return;

        var effect = Instantiate(slowTimeEffectPrefab, Vector3.zero, rotation);
        effect.useExpandingWave = expandingWaveEnabled; 

        // Apply the currently active upgrade flags
        /*
        proj.useFreezeEffect = freezeEnabled;
        proj.freezeDuration = freezeDuration;
        proj.projectileCrits = critEnabled;
        proj.enableEmpowerEffect = empowerEnabled;
        proj.empowerRadius = empowerRadius;
        */
    }


    // Called by upgrades
    public void EnableExpandingWave()
    {
        expandingWaveEnabled = true;
    }

    /*
    public void EnableCrit()
    {
        critEnabled = true;
    }

    public void EnableEmpower(float radius)
    {
        empowerEnabled = true;
        empowerRadius = radius;
    }
    */
}
