using UnityEngine;

public static class UIColors
{
    // Gameplay
    public static readonly Color Green     = new Color32(110, 230, 140, 255); // #6EE68C
    public static readonly Color Yellow    = new Color32(255, 214, 74, 255);  // #FFD64A
    public static readonly Color Orange    = new Color32(255, 170, 82, 255);  // #FFAA52
    public static readonly Color Red       = new Color32(255, 110, 125, 255); // #FF6E7D
    public static readonly Color Lavender  = new Color32(206, 170, 255, 255); // #CEAAFF
    public static readonly Color Purple    = new Color32(184, 94, 255, 255);  // #B85EFF

    public static string ToHex(Color c)
    {
        return ColorUtility.ToHtmlStringRGB(c);
    }
}