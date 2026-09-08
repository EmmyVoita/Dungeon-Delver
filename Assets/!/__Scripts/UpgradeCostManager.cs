using UnityEngine;

public class UpgradeCostManager : RuntimeModifierManager<IUpgradeCostModifier>
{
    public static UpgradeCostManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetModifiedCost(UpgradeBase upgrade)
    {
        if (upgrade == null)
            return 0;

        int currentCost = Mathf.Max(0, upgrade.Cost);

        SortActiveModifiers();

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IUpgradeCostModifier> entry = activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            currentCost = entry.Modifier.ModifyCost(upgrade,currentCost);

            currentCost = Mathf.Max(0, currentCost);
        }

        return currentCost;
    }
}