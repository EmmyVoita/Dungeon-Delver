using UnityEngine;

public abstract class UpgradeCardBase
{
    public string cardName;
    public string description;
    public Sprite icon;
    bool canStack = false;
    [HideInInspector] public bool hasBeenSelected = false;
    public abstract void ApplyUpgrade(Player player);
}
