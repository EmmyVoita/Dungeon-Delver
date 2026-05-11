using UnityEngine;

[CreateAssetMenu(menuName = "Obstacle/ObstacleDefinition")]
public class ObstacleDefinition : ScriptableObject
{
    public string displayName;
    public GameObject obstaclePrefab;
    public Sprite previewImage;
    public string description;
}
