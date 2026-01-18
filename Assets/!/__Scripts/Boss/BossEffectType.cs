using System;

[Flags]
public enum BossEffectType
{
    None                = 0,
    ModifyArrows        = 1 << 0,
    ModifyObstacles     = 1 << 1,
    SignatureMechanic   = 1 << 2,
    VisualPressure      = 1 << 3,
}
