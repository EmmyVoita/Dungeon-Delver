using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIWobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    public float amplitude = 10f;     // how far it moves (pixels)
    public float frequency = 1f;      // how fast it moves (cycles per second)
    public float phaseOffset = 0f;    // lets you offset motion between objects

    [Header("Randomization")]
    public bool randomizeOffset = true;  // automatically randomize start offset
    public float randomOffsetRange = Mathf.PI * 2f; // full sine wave range

    private RectTransform rect;
    private Vector2 startPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;

        if (randomizeOffset)
            phaseOffset = Random.Range(0f, randomOffsetRange);
    }

    void Update()
    {
        float y = Mathf.Sin((Time.time * frequency * 2f * Mathf.PI) + phaseOffset) * amplitude;
        rect.anchoredPosition = startPos + new Vector2(0, y);
    }
}
