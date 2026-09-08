using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Database")]
public class UpgradeDatabase : ScriptableObject
{
    [SerializeField] private List<UpgradeBase> upgrades = new();

    private Dictionary<string, UpgradeBase> _upgradeLookup;

    public List<UpgradeBase> Upgrades => upgrades;

    private void BuildLookup()
    {
        _upgradeLookup = new Dictionary<string, UpgradeBase>();

        foreach (UpgradeBase upgrade in upgrades)
        {
            if (upgrade == null)
                continue;

            _upgradeLookup[upgrade.UpgradeID] = upgrade;
        }
    }

    public UpgradeBase GetUpgrade(string id)
    {
        if (_upgradeLookup == null)
            BuildLookup();

        if (_upgradeLookup.TryGetValue(id, out UpgradeBase upgrade))
            return upgrade;

        Debug.LogWarning($"Could not find upgrade with ID: {id}");
        return null;
    }
}