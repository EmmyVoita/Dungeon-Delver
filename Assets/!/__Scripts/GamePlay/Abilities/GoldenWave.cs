using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CircleCollider2D))]
public class GoldenWave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float expandRadius = 4f;
    [SerializeField] private float expandDuration = 0.6f;
    [SerializeField] private float goldenScoreBoost = 1.5f;
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private SpriteRenderer waveSprite;
    [SerializeField] private LayerMask arrowLayer;

    private CircleCollider2D circleCollider;
    private float elapsed;

    public void Initalize(float absorbedCount)
    {
        Debug.Log("💛 Golden Wave Released! Absorbed Count: " + absorbedCount);
        expandRadius = Mathf.Clamp(0.75f + absorbedCount * 0.5f, 1f, 2.25f); 
    }

    void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        //circleCollider.radius = 1f; // base collider radius (acts as unit radius)
    }

    void Start()
    {
        transform.localScale = Vector3.zero;

        // Expands both sprite + collider perfectly in sync
        transform
            .DOScale(Vector3.one * expandRadius * 2f, expandDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));

        // optional safety destroy
        Destroy(gameObject, expandDuration + 0.05f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Smooth fadeout for visual
        if (waveSprite != null)
        {
            Color c = waveSprite.color;
            c.a = alphaCurve.Evaluate(elapsed / expandDuration);
            waveSprite.color = c;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only affect arrows on the correct layer
        if (((1 << other.gameObject.layer) & arrowLayer) == 0)
            return;

        ArrowBase arrow = other.GetComponent<ArrowBase>();
        if (arrow != null && !arrow.IsGolden)
        {
            arrow.SetGolden();
        }
    }
}
