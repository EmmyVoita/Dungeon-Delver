using System;
using UnityEngine;

[Serializable]
public struct MainBackgroundData
{
    public Texture2D mainBackground;
    public Texture2D secondaryBackground;

    public float mainSpeed;
    public float secondarySpeed;

    public Vector2 mainTiling;
    public Vector2 secondaryTiling;

    public Vector2 mainDirection;
    public Vector2 secondaryDirection;
}