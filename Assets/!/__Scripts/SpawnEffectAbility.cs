using UnityEngine;

public class SpawnEffectAbility : AbilityBase
{
    [Header("Effect")]
    [SerializeField] private AbilityEffectBase effectPrefab;
    [SerializeField] private bool spawnAtPlayer = true;

    public override void Activate(Quaternion rotation)
    {
        if (effectPrefab == null)
        {
            Debug.LogError($"{name} has no effect prefab.");
            EndAbility();
            return;
        }

        Vector3 position =
            spawnAtPlayer && Player.Instance != null
                ? Player.Instance.transform.position
                : Vector3.zero;

        AbilityEffectBase effect = Instantiate(
            effectPrefab,
            position,
            rotation
        );

        // Listen before activating in case the effect ends immediately.
        effect.OnEffectEnded += HandleEffectEnded;

        float duration = GetModifiedDuration();

        effect.Activate(new AbilityEffectContext
        {
            Duration = duration
        });
    }

    private void HandleEffectEnded(AbilityEffectBase effect)
    {
        if (effect != null)
            effect.OnEffectEnded -= HandleEffectEnded;

        EndAbility();
    }
}