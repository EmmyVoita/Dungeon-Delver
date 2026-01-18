using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Extend Golden On Crit")]
public class ExtendGoldenOnCritUpgrade : UpgradeBase, IActivatableUpgrade
{
    public int maxStack = 3;
    public int arrowsPerCrit = 1;
    private int stacksLeft = 0;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{MAX_STACK}", maxStack.ToString())
            .Replace("{ARROWS_PER_CRIT}", arrowsPerCrit.ToString());
    }

    public void Activate()
    {
        ArrowBase.OnArrowResolved += HandleArrowResolved;
        BuffHelpers.OnGoldenArrowSessionStarted += AddArrows;
        stacksLeft = 0;
    }

    public void Deactivate()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
        BuffHelpers.OnGoldenArrowSessionStarted -= AddArrows;
        stacksLeft = 0;
    }

    public void AddArrows()
    {
        stacksLeft = maxStack;
    }

    private void HandleArrowResolved(ArrowResolvedData data)
    {
        if (data.goalType != Goal.GoalType.Critical)
            return;

        if (!data.status.HasFlag(ArrowStatus.Golden))
            return;
       
        if(stacksLeft > 0)
        {
            BuffHelpers.GetOrCreateGoldenEffect(
            arrowsPerCrit
            );

            UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);

            stacksLeft--;
        }
    }

}
