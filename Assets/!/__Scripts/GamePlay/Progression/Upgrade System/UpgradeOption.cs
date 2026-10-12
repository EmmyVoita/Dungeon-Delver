using UnityEngine;

public class UpgradeOption 
{
    private UpgradeBase effect;

    public UpgradeOption(UpgradeBase effect)
    {
        this.effect = effect;
    }

    public Sprite CardDecoration => effect.cardDecoration;
    public Sprite CenterIcon => effect.CenterIcon;
    public string DisplayName => effect.displayName;
    public string Description => effect.GetDescription();
    public UpgradeBase Base => effect;

    public void OnSelected()
    {
        Debug.Log($"Upgrade Option Affect Aplied => {DisplayName}");
        effect.Apply();
    }
}
