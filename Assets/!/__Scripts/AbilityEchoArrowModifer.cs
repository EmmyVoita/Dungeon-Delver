using UnityEngine;

public class AbilityEchoArrowModifier : IArrowSpawnEffect
{
    private float _spawnChance;

    public int Priority => 0;

    public AbilityEchoArrowModifier(float spawnChance)
    {
        _spawnChance = spawnChance;
    }

    public void OnActivate()
    {
        //ArrowSpawnManager.Instance.AddSpawnEffect(this);
    }

    public void Destroy()
    {
        //ArrowSpawnManager.Instance.RemoveSpawnEffect(this);
    }

    public void OnArrowSpawned(ArrowBase arrow)
    {
        if (Random.value > _spawnChance)
            return;

        arrow.SetEmpoweredArrow();
    }

    public IRuntimeModifier Clone()
    {
        return new AbilityEchoArrowModifier(_spawnChance);
    }
}