using System;
using UnityEngine;

[Serializable]
public class WorldMapRouteOption
{
    public WorldMapRouteType type;

    public Sprite icon;

    public string displayName;
    public float baseWeight = 1.0f;

    [TextArea]
    public string description;


}