using UnityEngine;

public class OnAbilityUsedRMM : RuntimeModifierManager<IAbilityEffectModifier>
{
    public static OnAbilityUsedRMM Instance { get; private set; }

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
        Player.OnAbilityUsed += HandleTriggeringEffects;
    }

    protected override void Unsubscribe()
    {
        Player.OnAbilityUsed -= HandleTriggeringEffects;
        base.Unsubscribe();
    }

    public void HandleTriggeringEffects()
    {
        SortActiveModifiers();

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IAbilityEffectModifier> entry = activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            entry.Modifier.TriggerEffect();
        }
    }
}