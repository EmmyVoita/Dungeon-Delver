using UnityEngine;
public abstract class IntermediateEffectSO : ScriptableObject
{
    [Header("Card UI")]
    public Sprite icon;
    public string displayName;

    [TextArea]
    public string descriptionTemplate;

    public abstract void Apply();

    public virtual string GetDescription()
    {
        return descriptionTemplate;
    }
}
