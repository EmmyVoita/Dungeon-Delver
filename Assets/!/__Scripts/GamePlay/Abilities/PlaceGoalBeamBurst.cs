using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PlaceGoalBeamBurst : MonoBehaviour
{
    [Header("Beam Dimensions")]
    public float upDownLength = 8f;
    public float leftRightLength = 14f; // widescreen correction
    public float beamThickness = 1f;
    public float lifetime = 0.4f;
    public float additionalOffset = 1f;

    [Header("Visuals")]
    public Color beamColor = Color.cyan;
    public AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private BoxCollider2D col;
    public SpriteRenderer sRend;
    private Vector3 startScale;
    private float timer = 0f;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        //sRend = GetComponent<SpriteRenderer>();
        sRend.color = beamColor;
        startScale = transform.localScale;
    }

    void Start()
    {
        // Determine facing direction
        Vector2 dir = transform.up;
        bool facingVertical = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);

        float targetLength = facingVertical ? upDownLength : leftRightLength;

        // Scale and offset beam so it extends outward
        sRend.transform.localScale = new Vector3(beamThickness, targetLength, 1f);

        // Offset so it extends away from the goal (not centered on it)
        Vector3 offset = transform.up * (targetLength * 0.5f + additionalOffset);
        sRend.transform.position += offset;

        // Adjust collider size to match beam
        col.size = new Vector2(beamThickness, targetLength);
        col.offset = new Vector2(0f, targetLength * 0.5f + additionalOffset);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / lifetime);

        // Optional fade out or flash intensity
        float alpha = 1f - appearCurve.Evaluate(t);
        sRend.color = new Color(beamColor.r, beamColor.g, beamColor.b, alpha);

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ArrowBase arrow = other.GetComponent<ArrowBase>();
        if (arrow != null)
        {
            arrow.OnArrowHit(1, Goal.GoalType.Normal, transform.up);
        }

        // Optional: could damage enemies here later
    }
}
