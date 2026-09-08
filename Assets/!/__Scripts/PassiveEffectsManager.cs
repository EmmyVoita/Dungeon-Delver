
using UnityEngine;
using System;

public class PassiveEffectsManager : RuntimeModifierManager<IPassiveEffect>
{
    public static PassiveEffectsManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void OnGameStateChanged(
        GameState previousState,
        GameState newState)
    {
        base.OnGameStateChanged(previousState, newState);

        if(newState == GameStateManager.LevelStartState)
        {
            foreach(RuntimeModifierEntry<IPassiveEffect> passiveEffect in activeModifiers)
            {
                if(passiveEffect.Modifier.ShouldStart())
                    passiveEffect.Modifier.Start();
            }
        }
        else if(newState == GameStateManager.LevelEndState)
        {
            foreach(RuntimeModifierEntry<IPassiveEffect> passiveEffect in activeModifiers)
            {
                passiveEffect.Modifier.Stop();
            }
        }
    }

    private void Update()
    {
        foreach(RuntimeModifierEntry<IPassiveEffect> passiveEffect in activeModifiers)
        {
            passiveEffect.Modifier.Update();
        }
    }
}