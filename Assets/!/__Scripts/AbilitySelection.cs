using UnityEngine;

public static class AbilitySelection
{
    public static AbilityType SelectedAbility = AbilityType.None;
}

public enum AbilityType
{
    None,
    ReturnToMenu,
    SlowTime,
    OrbitingShield,
    PlaceShield,
    ProjectileBurst,
    GoldenHarvest,
    RandomQuestion
}
