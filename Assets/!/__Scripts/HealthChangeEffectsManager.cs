using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthChangeEffectsManager : RuntimeModifierManager<IHealthChangeEffect>
{
    public static HealthChangeEffectsManager Instance;

    [Header("Feedback")]
    [SerializeField] private SoundEffect procSoundEffect;


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
        Player.OnHealthChanged += HandleHealthChanged;
    }

    protected override void Unsubscribe()
    {
        Player.OnHealthChanged -= HandleHealthChanged;
    }


    private void HandleHealthChanged(int changeAmount)
    {
        if (!TriggerHealthEffects(changeAmount))
            return;

        AudioHelpers.PlaySoundEffect(
            procSoundEffect,
            Player.Instance.transform.position
        );
    }


    private bool TriggerHealthEffects(int amount)
    {
        if (activeModifiers.Count <= 0)
            return false;

        bool anyEffectTriggered = false;

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IHealthChangeEffect> effect = activeModifiers[i];

            if (effect == null)
                continue;

            if (!effect.Modifier.CanTriggerEffect(amount))
                continue;

            if (effect.Modifier.TriggerEffect(amount))
                anyEffectTriggered = true;
        }

        return anyEffectTriggered;
    }
}