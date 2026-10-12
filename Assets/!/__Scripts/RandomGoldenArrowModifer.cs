using UnityEngine;

public class RandomGoldenArrowModifier : IArrowSpawnEffect
{
    private float _spawnChance;

    public int Priority => 0;

    public RandomGoldenArrowModifier(float spawnChance)
    {
        _spawnChance = spawnChance;
    }

    public void OnActivate()
    {
    }

    public void Destroy()
    {
    }

    public void OnArrowSpawned(ArrowBase arrow)
    {
        if (Random.value > _spawnChance)
            return;

        arrow.SetGolden();
    }

    public IRuntimeModifier Clone()
    {
        return new RandomGoldenArrowModifier(_spawnChance);
    }
}