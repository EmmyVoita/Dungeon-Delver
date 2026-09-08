using UnityEngine;

public class JumpEffectsManager : RuntimeModifierManager<IJumpEffect>
{
    public static JumpEffectsManager Instance { get; private set; }

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
        Player.OnJumped += HandlePlayerJumped;
    }

    protected override void Unsubscribe()
    {
        Player.OnJumped -= HandlePlayerJumped;
    }

    private void HandlePlayerJumped(JumpData data)
    {
        TriggerJumpEffects(data);
    }

    private void TriggerJumpEffects(JumpData data)
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            RuntimeModifierEntry<IJumpEffect> entry = activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            if (!entry.Modifier.CanTriggerEffect(data))
                continue;

            entry.Modifier.TriggerEffect(data);
        }
    }
}