using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability Database")]
public class AbilityDatabase : ScriptableObject
{
    public List<AbilityData> abilities;
}