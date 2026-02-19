using UnityEngine;

public class IdleBob : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobHeight = 0.1f;   // how far up/down
    public float bobSpeed = 1.5f;    // how fast it moves

    private Vector3 startPos;
    private float phaseOffset;

    void Start()
    {
        startPos = transform.localPosition;

        // Randomize where in the sine wave this object starts
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed + phaseOffset) * bobHeight;
        transform.localPosition = startPos + new Vector3(0f, yOffset, 0f);
    }
}
