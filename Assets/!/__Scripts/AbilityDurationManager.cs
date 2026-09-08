using UnityEngine;

public class AbilityDurationManager : RuntimeModifierManager<IAbilityDurationModifier>
{
    public static AbilityDurationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public float GetModifiedDuration(float baseDuration)
    {
        float currentDuration = baseDuration;

        SortActiveModifiers();

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IAbilityDurationModifier> entry =
                activeModifiers[i];

            if (entry?.Modifier == null)
                continue;

            currentDuration =
                entry.Modifier.ModifyDuration(currentDuration);
        }

        return Mathf.Max(0f, currentDuration);
    }
}