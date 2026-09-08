using UnityEngine;

public class TimedEffectManager : RuntimeModifierManager<ITimedEffect>
{
    public static TimedEffectManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<ITimedEffect> entry = activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            entry.Modifier.Tick(deltaTime);
        }
    }
}