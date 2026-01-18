using UnityEngine;

[CreateAssetMenu(menuName = "Type Definitions/Arrow Type Definition")]
public class ArrowTypeDefinition : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public Sprite icon;        // optional for UI

    [Header("Telegraph")]
    public bool requiresWarning;
    public float warningLeadTime = 0.6f;
}
