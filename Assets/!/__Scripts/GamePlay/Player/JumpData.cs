

using UnityEngine;

public readonly struct JumpData
{
    public Vector2 Direction { get; }
    public JumpType Type { get; }

    public bool IsExtraJump => Type == JumpType.Extra;

    public JumpData(Vector2 direction, JumpType type)
    {
        Direction = direction;
        Type = type;
    }
}