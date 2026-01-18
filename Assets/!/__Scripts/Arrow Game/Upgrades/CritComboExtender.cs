using UnityEngine;

public class CritComboExtender : UpgradeEffectBase
{
    void OnEnable()
    {
    }
    void OnDisable()
    {
    }

    public override void Apply(Player player)
    {
        CritComboUI.ExtendCritComboWindow?.Invoke(1); // Extend by 2 seconds
        Debug.Log("Invoked ExtendCritComboWindow event to extend by 2 seconds.");
    }
}
