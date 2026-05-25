using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Challenge Objects")]
public class ChallengeObjectDatabase : ScriptableObject
{
    public List<ObstacleTypeDefinition> obstacles;
}