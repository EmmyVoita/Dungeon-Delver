using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Combo Shield Upgrade")]
public class ComboShieldUpgrade : UpgradeBase
{
    [SerializeField] private int hitBlockCharges = 1;
    
    public override void Apply()
    {
        DamageSaveManager.Instance.Register(new ShieldDamageSave(true));
        ComboManager.Instance.PreventNextComboBreak();
    }
}
