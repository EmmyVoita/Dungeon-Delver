
using UnityEngine;
public class LaneMover : MonoBehaviour
{
    [SerializeField] private float speed = 3f;

    private int direction = 1;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int dir)
    {
        direction = dir;
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(speed * direction, 0);
    }
}