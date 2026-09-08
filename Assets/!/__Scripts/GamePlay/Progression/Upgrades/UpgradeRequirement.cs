using UnityEngine;


[System.Serializable]
public struct UpgradeRequirement
{
    public UpgradeBase upgrade;
    [Min(1)] public int minimumStacks;
}