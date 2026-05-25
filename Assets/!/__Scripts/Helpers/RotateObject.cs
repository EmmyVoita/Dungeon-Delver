using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private bool randomStartingAngleZ = false;
    [SerializeField] public Vector3 rotationSpeed = new Vector3(0f, 0f, 90f); // degrees per second
    [SerializeField] public Space rotationSpace = Space.Self; // or Space.World

    void Start()
    {
        if(randomStartingAngleZ)
            transform.eulerAngles = new Vector3(0,0,Random.Range(0,360));
    }
    void Update()
    {
        
        transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
    }
}
