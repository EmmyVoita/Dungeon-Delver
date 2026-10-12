public enum FireMode
{
    Burst,
    Continuous
}

public enum ProjectileFireMode
{
    Still,
    MoveBackForth
}

public enum FireDelayMode
{
    Immediate,
    WithSetDelay,
    Randomized
}

public enum FirePositionMode
{
    Default,
    PositionList
}

[System.Serializable]
public struct WallShooterData
{
    public FireMode fireMode;
    public ProjectileFireMode projectileFireMode;
    public FireDelayMode fireDelayMode;
    public FirePositionMode firePositionMode;
}