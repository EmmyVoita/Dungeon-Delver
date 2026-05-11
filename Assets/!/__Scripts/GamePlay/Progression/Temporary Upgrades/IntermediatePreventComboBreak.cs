using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Prevent Next Combo Break")]
public class IntermediatePreventComboBreak : UpgradeBase
{
    public override void Apply()
    {
        ComboManager.Instance.PreventNextComboBreak();
    }
}
