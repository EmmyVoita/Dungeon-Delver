
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ArrowDirectionVisual
{
    public Sprite arrowSprite;
    public Vector2 direction;
}

public class Marker : MonoBehaviour
{
    public RectTransform root;
    public Image fillImage;
    public Image outlineImage;
    public Image arrowImage;

    [SerializeField] private List<ArrowDirectionVisual> data = new();

    public void EnableArrowImage(bool value)
    {
        arrowImage.enabled = value;
    }

    public void AssignSprite(Vector2 direction)
    {
        Sprite target = data.First(d => d.direction == direction).arrowSprite;
        if(arrowImage != null && target != null)
        {
            arrowImage.sprite = target;
        }
    }
}
