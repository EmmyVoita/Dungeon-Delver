using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollectEffectsManager : RuntimeModifierManager<ICoinCollectEffect>
{
    public static CoinCollectEffectsManager Instance;

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
        CurrencyManager.OnCurrencyAdded += HandleCurrencyAdded;
    }

    protected override void Unsubscribe()
    {
        CurrencyManager.OnCurrencyAdded -= HandleCurrencyAdded;
    }


    private void HandleCurrencyAdded(int amount)
    {
        if (!TriggerCurrencyEffects(amount))
            return;

        AudioHelpers.PlaySoundEffect(
            procSoundEffect,
            Player.Instance.transform.position
        );
    }

    /*
    private bool PullCurrencyEffect(int amount)
    {
        if( activeModifiers.Count <= 0)
            return false;

         activeModifiers.Sort((x,y) => y.Priority.CompareTo(x.Priority));

        for(int i = 0; i <  activeModifiers.Count; i++)
        {
            ICoinCollectEffect effect =  activeModifiers[i];

            if(!effect.CanTriggerEffect(amount))
                continue;

            
            effect.TriggerEffect(amount);
            return true;
        }

        return false;
    } 
    */

    private bool TriggerCurrencyEffects(int amount)
    {
        if (activeModifiers.Count <= 0)
            return false;

        bool anyEffectTriggered = false;

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            ICoinCollectEffect effect = activeModifiers[i];

            if (effect == null)
                continue;

            if (!effect.CanTriggerEffect(amount))
                continue;

            if (effect.TriggerEffect(amount))
                anyEffectTriggered = true;
        }

        return anyEffectTriggered;
    }
}