using System;
using UnityEngine;

[Serializable]
public class WorldMapRouteOption
{
    public WorldMapRouteType type;

    public Sprite icon;

    public string displayName;

    [TextArea]
    public string description;


}