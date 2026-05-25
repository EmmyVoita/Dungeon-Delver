using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Game State Database")]
public class GameStateDatabase : ScriptableObject
{
    public List<GameStateSettingData> data;
}
    