using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCategoryDatabase", menuName = "Database/Upgrade Category Database")]
public class UpgradeCategoryDatabase : ScriptableObject
{
    [Serializable]
    public class CategoryEntry
    {
        public UpgradeCategory category;
        public Sprite icon;
        public Color color = Color.white;
    }

    [SerializeField]
    private Sprite fallbackIcon;

    [SerializeField]
    private List<CategoryEntry> entries = new();

    private Dictionary<UpgradeCategory, CategoryEntry> _lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void OnValidate()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<UpgradeCategory, CategoryEntry>();

        foreach (CategoryEntry entry in entries)
        {
            if (entry == null)
                continue;

            if (_lookup.ContainsKey(entry.category))
            {
                Debug.LogWarning(
                    $"Duplicate category entry for {entry.category}.",
                    this
                );

                continue;
            }

            _lookup.Add(entry.category, entry);
        }
    }

    public Sprite GetIcon(UpgradeCategory category)
    {
        EnsureLookup();

        return _lookup.TryGetValue(category, out CategoryEntry entry)
            ? entry.icon
            : fallbackIcon;
    }

    public Color GetColor(UpgradeCategory category)
    {
        EnsureLookup();

        return _lookup.TryGetValue(category, out CategoryEntry entry)
            ? entry.color
            : Color.white;
    }

    private void EnsureLookup()
    {
        if (_lookup == null)
            BuildLookup();
    }
}