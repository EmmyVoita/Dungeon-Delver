using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "UI/Icon Data", fileName = "NewIconData")]
public class IconData : ScriptableObject
{
    public List<Sprite> frames;

    [Header("Animation")]
    public bool animated = false;
    public float frameDuration = 0.1f;
    public bool loop = true;
}