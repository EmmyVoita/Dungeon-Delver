using UnityEngine;

public abstract class UpgradeBase : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId;

    [Header("UI")]
    public string displayName;
    [TextArea] public string descriptionTemplate;
    public Sprite baseIcon;
    public Sprite activeIcon;
    public IconFeedbackStyle feedbackStyle;

    public virtual string GetDescription()
    {
        return descriptionTemplate;
    }
}
