using UnityEngine;

[System.Serializable]
public class ArrowEvent
{
    public float time;          // beat time
    public Vector2 direction;
    public float speed;
    public string nameOfGameObjectToSpawn;       // ⭐ string id for scriptable object
    public float arrivalTime;   // in seconds
    public float spawnTime;     // in seconds
}
