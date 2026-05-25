using UnityEngine;

public static class UIColors
{
    // Gameplay
    public static readonly Color Green = new Color32(152, 255, 152, 255);
    public static readonly Color Yellow = new Color32(255, 215, 0, 255);
    public static readonly Color Red = new Color32(255, 122, 122, 255);

    public static string ToHex(Color c)
    {
        return ColorUtility.ToHtmlStringRGB(c);
    }
}