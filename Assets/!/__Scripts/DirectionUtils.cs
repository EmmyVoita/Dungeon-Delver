using UnityEngine;

public static class DirectionUtils
{
    public static Vector2 FromString(string direction)
    {
        return direction.Trim().ToLower() switch
        {
            "up" => Vector2.up,
            "down" => Vector2.down,
            "left" => Vector2.left,
            "right" => Vector2.right,
            "up-right" => new Vector2(1, 1).normalized,
            "down-right" => new Vector2(1, -1).normalized,
            "down-left" => new Vector2(-1, -1).normalized,
            "up-left" => new Vector2(-1, 1).normalized,
            _ => Vector2.zero
        };
    }

    public static string FromDirection(Vector2 direction)
    {
        if (direction == Vector2.up) return "up";
        if (direction == Vector2.down) return "down";
        if (direction == Vector2.left) return "left";
        if (direction == Vector2.right) return "right";

        if (direction == new Vector2(1, 1).normalized) return "up-right";
        if (direction == new Vector2(1, -1).normalized) return "down-right";
        if (direction == new Vector2(-1, -1).normalized) return "down-left";
        if (direction == new Vector2(-1, 1).normalized) return "up-left";

        return "unknown";
    }
    
}