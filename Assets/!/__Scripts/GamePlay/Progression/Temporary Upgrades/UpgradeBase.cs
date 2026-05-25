using UnityEngine;
public abstract class UpgradeBase : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId;

    [Header("Card UI")]
    public Sprite icon;
    public Material iconMaterial;
    public string displayName;

    [TextArea]
    public string descriptionTemplate;

    public virtual void Apply() {}

    public virtual string GetDescription()
    {
        return descriptionTemplate;
    }
}
