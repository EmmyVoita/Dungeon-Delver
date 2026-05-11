using UnityEngine;

[CreateAssetMenu(menuName = "Type Definitions/Obstacle Type Definition")]
public class ObstacleTypeDefinition : ScriptableObject
{
    public string displayName;
    public Color textColor = Color.white;
    public string fileName;
    public GameObject prefab;
    public Sprite icon;        // optional for UI
    [TextArea] public string description;
}
