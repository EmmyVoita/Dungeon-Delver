using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Prevent Next Combo Break")]
public class IntermediatePreventComboBreak : IntermediateEffectSO
{
    public override void Apply()
    {
        ComboManager.Instance.PreventNextComboBreak();
    }
}
