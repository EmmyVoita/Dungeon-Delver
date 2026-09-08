using UnityEngine;

public class OnShopRerollRMM : RuntimeModifierManager<IShopRerollEffect>
{
    public static OnShopRerollRMM Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Subscribe()
    {
        RunStateManager.OnUseReroll += HandleTriggeringEffects;
    }

    protected override void Unsubscribe()
    {
        RunStateManager.OnUseReroll -= HandleTriggeringEffects;
        base.Unsubscribe();
    }

    public void HandleTriggeringEffects()
    {
        SortActiveModifiers();

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IShopRerollEffect> entry = activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            entry.Modifier.OnShopRerolled();
        }
    }
}