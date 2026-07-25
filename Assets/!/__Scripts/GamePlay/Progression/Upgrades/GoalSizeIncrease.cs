using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/GoalSizeIncrease")]
public class GoalSizeIncrease: UpgradeBase, IGoalSizeModifier
{
    public float goalSizeMultiplier = 1.25f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{GOAL_SIZE_MULTIPLIER}", ( goalSizeMultiplier - 1).ToString("P0"));
    }

    public float ModifyGoalSize(float current)
        => current *  goalSizeMultiplier;
}
