using System.Collections.Generic;
using UnityEngine;

public class AbilityIconDatabase : MonoBehaviour
{
    public static AbilityIconDatabase Instance;

    [SerializeField] private List<AbilityIconEntry> entries;

    private Dictionary<AbilityType, Sprite> lookup;

    private void Awake()
    {
        Instance = this;

        lookup = new Dictionary<AbilityType, Sprite>();

        foreach (var entry in entries)
        {
            lookup[entry.type] = entry.icon;
        }
    }

    public Sprite GetIcon(AbilityType type)
    {
        return lookup.TryGetValue(type, out var icon) ? icon : null;
    }
}