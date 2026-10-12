using System.Collections.Generic;
using UnityEngine;

public class AbilityIconDatabase : MonoBehaviour
{
    public static AbilityIconDatabase Instance;

    [SerializeField] private List<AbilityIconEntry> entries;

    private Dictionary<AbilityType, AbilityIconEntry> lookup;

    private void Awake()
    {
        Instance = this;

        lookup = new Dictionary<AbilityType, AbilityIconEntry>();

        foreach (var entry in entries)
        {
            lookup[entry.type] = entry;
        }
    }

    public Sprite GetIcon(AbilityType type)
    {
        return lookup.TryGetValue(type, out var entry) ? entry.icon : null;
    }

    public Sprite GetSmallIcon(AbilityType type)
    {
        return lookup.TryGetValue(type, out var entry) ? entry.smallIcon : null;
    }
}