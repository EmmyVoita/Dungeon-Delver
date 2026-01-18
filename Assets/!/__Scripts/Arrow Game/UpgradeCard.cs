using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Card")]
public class UpgradeCard : ScriptableObject
{
    public UpgradeBase upgrade;
    public bool canStack;
}
