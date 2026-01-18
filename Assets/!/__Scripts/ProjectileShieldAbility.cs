using UnityEngine;

public class ProjectileShieldAbility : AbilityBase
{
    [Header("Ability Settings")]
    public ProjectileShieldShot projectilePrefab;
    public Transform firePoint;

    // Runtime flags modified by upgrades
    private bool freezeEnabled;
    private bool critEnabled;
    private bool empowerEnabled;
    private float freezeDuration = 2f;
    private float empowerRadius = 3f;

    public override void Activate(Quaternion rotation)
    {
        if (projectilePrefab == null)
            return;

        var proj = Instantiate(projectilePrefab, Player.Instance.transform.position, rotation);

        // Apply the currently active upgrade flags
        proj.useFreezeEffect = freezeEnabled;
        proj.freezeDuration = freezeDuration;
        proj.projectileCrits = critEnabled;
        proj.enableEmpowerEffect = empowerEnabled;
        proj.empowerRadius = empowerRadius;
    }

    // Called by upgrades
    public void EnableFreeze(float duration)
    {
        freezeEnabled = true;
        freezeDuration = duration;
    }

    public void EnableCrit()
    {
        critEnabled = true;
    }

    public void EnableEmpower(float radius)
    {
        empowerEnabled = true;
        empowerRadius = radius;
    }
}
