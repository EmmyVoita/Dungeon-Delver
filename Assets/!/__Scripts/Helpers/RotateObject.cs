using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0f, 0f, 90f); // degrees per second
    public Space rotationSpace = Space.Self; // or Space.World

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
    }
}
