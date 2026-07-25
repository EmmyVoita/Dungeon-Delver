[System.Flags]
public enum ArrowStatus
{
    None    = 0,
    Golden  = 1 << 0,
    Frozen  = 1 << 1,
    Recovery = 1 << 2,
    TimeSlow = 1 << 3
}
