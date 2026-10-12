using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Stages Database")]
public class StageDatabase : ScriptableObject
{
    [SerializeField] private List<StageDataObject> stages;
    public List<StageDataObject> Stages => stages;
}