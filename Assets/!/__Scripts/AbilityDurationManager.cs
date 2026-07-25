using UnityEngine;

public class AbilityDurationManager
    : RuntimeModifierManager<IAbilityDurationModifier>
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

        foreach (IAbilityDurationModifier modifier in activeModifiers)
        {
            currentDuration = modifier.ModifyDuration(currentDuration);
        }

        return Mathf.Max(0f, currentDuration);
    }
}