using UnityEngine;

public class OnArrowSpawnedRMM : RuntimeModifierManager<IArrowSpawnEffect>
{
    public static OnArrowSpawnedRMM Instance { get; private set; }

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
        ArrowSpawner.OnArrowSpawned += HandleTriggeringEffects;
    }

    protected override void Unsubscribe()
    {
        ArrowSpawner.OnArrowSpawned -= HandleTriggeringEffects;
        base.Unsubscribe();
    }


    public void HandleTriggeringEffects(ArrowBase arrowBase)
    {
        SortActiveModifiers();

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IArrowSpawnEffect> entry = activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            entry.Modifier.OnArrowSpawned(arrowBase);
        }
    }
}