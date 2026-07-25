using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Extend Golden On Crit")]
public class ExtendGoldenOnCritUpgrade : UpgradeBase
{
    public int maxStack = 3;
    public int arrowsPerCrit = 1;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{MAX_STACK}", maxStack.ToString())
            .Replace("{ARROWS_PER_CRIT}", arrowsPerCrit.ToString());
    }

    public override void Apply()
    {
        GoldenArrowManager.Instance.AddGoldenExtension(this);
    }

}
