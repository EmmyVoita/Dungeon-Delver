using System.Collections.Generic;
using UnityEngine;

public class MenuBlackHole : MonoBehaviour
{
    public static List<MenuBlackHole> All = new List<MenuBlackHole>();

    public float gravityStrength = 10f;
    public float influenceRadius = 4f;
    public float consumeRadius = 0.6f;
    public Vector2 velocity;

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    public Vector2 Position => transform.position;
}