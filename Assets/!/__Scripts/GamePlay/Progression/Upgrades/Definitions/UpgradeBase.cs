using UnityEngine;
public abstract class UpgradeBase : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private int _cost;
    [SerializeField] private int _maxStacks;


    [Header("Card Decoration")]
    public Sprite cardDecoration;
    public Material cardDecorationMaterial;


    [Header("Card UI")]
    public Sprite centerIcon;
    public Sprite icon;
    public Material iconMaterial;
    

    [Header("Display UI")]
    public string displayName;
    [TextArea] public string descriptionTemplate;
    [TextArea] public string detailsTemplate;


    public int MaxStacks => _maxStacks;
    public int Cost => _cost;

    public virtual void Apply() {}

    public virtual string GetDescription()
    {
        return descriptionTemplate;
    }

    public virtual string GetDetails()
    {
        return descriptionTemplate;
    }
}
