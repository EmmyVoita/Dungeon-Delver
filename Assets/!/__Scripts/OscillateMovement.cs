using UnityEngine;

public class OscillateMovement : MonoBehaviour
{
    public Vector3 direction = Vector3.right;
    public float distance = 1f;
    public float speed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float offset = Mathf.PingPong(timer * speed, distance);
        transform.position = startPos + direction.normalized * offset;
    }

}
