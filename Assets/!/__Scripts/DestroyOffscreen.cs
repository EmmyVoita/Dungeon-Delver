using UnityEngine;

public class DestroyOffscreen : MonoBehaviour
{
    public float bounds = 15f;

    void Update()
    {
        if (Mathf.Abs(transform.position.x) > bounds ||
            Mathf.Abs(transform.position.y) > bounds)
        {
            Destroy(gameObject);
        }
    }
}