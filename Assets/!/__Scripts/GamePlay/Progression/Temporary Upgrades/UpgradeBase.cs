using UnityEngine;
public abstract class UpgradeBase : ScriptableObject
{
    [Header("Cost")]
    [SerializeField] private int _cost;
    [SerializeField] private int _maxStacks;

    [Header("Identity")]
    public string upgradeId;


    [Header("Card Decoration")]
    public Sprite cardDecoration;
    public Material cardDecorationMaterial;

    [Header("Card UI")]
    public Sprite centerIcon;
    public Sprite icon;
    public Material iconMaterial;
    
    public string displayName;

    [Header("Footer Icon UI")]
    public bool displayInFooter;

    [TextArea]
    public string descriptionTemplate;

    public int MaxStacks => _maxStacks;
    public int Cost => _cost;

    public virtual void Apply() {}

    public virtual string GetDescription()
    {
        return descriptionTemplate;
    }
}
