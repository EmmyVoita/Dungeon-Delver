using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Arrow Objects")]
public class ArrowObjectDatabase : ScriptableObject
{
    public List<ArrowTypeDefinition> arrows;
}