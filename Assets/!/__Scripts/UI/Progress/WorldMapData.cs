using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/World Map Data")]
public class WorldMapData : ScriptableObject
{
    public List<WorldNodeData> nodes = new();
}