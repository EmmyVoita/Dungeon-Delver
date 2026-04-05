using UnityEngine;
using DG.Tweening;

public enum BalloonColor
{
    Blue,
    Yellow,
    Green,
    Purple
}

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class BalloonObject : MonoBehaviour
{
    [Header("Balloon Color")]
    public BalloonColor balloonColor;
    public Sprite emptySprite; // for debugging
    public Sprite blueSprite;
    public Sprite yellowSprite;
    public Sprite greenSprite;
    public Sprite purpleSprite;

    [Header("Rotation Settings")]
    public float rotationScaler = 5f;     // tilt based on speed
    public float wobbleAmount = 5f;       // wobble in degrees
    public float wobbleSpeed = 2f;        // time for wobble

    [Header("Visual Effects")]
    public ParticleSystem hitParticles;
    public float fadeDuration = 0.2f;

    [Header("Audio")]
    public AudioClip hitSound;

    [Header("Glow / Outline Settings")]
    public Color outlineColor = Color.white;
    public float outlineScale = 1.12f;
    public float glowPulseStrength = 0.2f;
    public float glowPulseSpeed = 1.2f;


    private SpriteRenderer sRend;
    private Collider2D col;
    private bool hasBeenHit = false;

    private BalloonGalleryManager manager;

    private float baseTiltRotation;       // store directional tilt!
    private Tween wobbleTween;

    // Outline renderer
    private SpriteRenderer outlineRenderer;
    private Tween glowTween;

    public Tween moveTween; // assigned by manager

    private float currentMoveSpeed;
    private Vector2 moveDir;


    public void SlowToStop(float slowDuration = 0.5f)
    {
        if (moveTween != null && moveTween.IsActive())
        {
            // Slow movement tween
            DOTween.To(() => moveTween.timeScale, 
                    x => moveTween.timeScale = x, 
                    0f, 
                    slowDuration)
                .SetEase(Ease.OutCubic);

            // Also reduce speed value we use to tilt
            DOTween.To(() => currentMoveSpeed,
                    x => { 
                        currentMoveSpeed = x; 
                        ApplyTilt();     // update tilt every frame
                    },
                    0f,
                    slowDuration)
                .SetEase(Ease.OutCubic);
        }
    }



    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        CreateOutlineRenderer();

    }

    void Start()
    {
        ApplyColorSprite();
    }

    public void Init(BalloonGalleryManager parent, BalloonColor color, float moveSpeed, Vector2 direction)
    {
        manager = parent;
        balloonColor = color;

        currentMoveSpeed = moveSpeed;
        moveDir = direction.normalized;

        if (manager == null || manager.IsEnding)
        {
            ForceKill();
            return;
        }

        ApplyColorSprite();
        ApplyTilt();    // set initial rotation
        StartWobble();
    }

    private void ApplyTilt()
    {
        float signed = Mathf.Sign(moveDir.x); // tilt left or right depending on direction
        float angle = signed * currentMoveSpeed * rotationScaler;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }




    void ApplyColorSprite()
    {
        Sprite chosen = null;

        switch (balloonColor)
        {
            case BalloonColor.Blue:   chosen = blueSprite; break;
            case BalloonColor.Yellow: chosen = yellowSprite; break;
            case BalloonColor.Green:  chosen = greenSprite; break;
            case BalloonColor.Purple: chosen = purpleSprite; break;
        }

        sRend.sprite = chosen;
        
        if (outlineRenderer != null)
            outlineRenderer.sprite = emptySprite;
    }

      // ---------------------------------------------------------
    // OUTLINE ENABLE / DISABLE
    // ---------------------------------------------------------
    public void EnableGlow()
    {
        if (outlineRenderer == null) return;

        // base color visible
        outlineRenderer.color = outlineColor;

        // pulsing alpha
        glowTween?.Kill();
        glowTween = outlineRenderer
            .DOFade(1f, glowPulseSpeed)
            .From(1f - glowPulseStrength)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void DisableGlow()
    {
        glowTween?.Kill();
        glowTween = null;

        if (outlineRenderer != null)
            outlineRenderer.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, 0);
    }

    void StartWobble()
    {
        // Kill old tween if reused
        wobbleTween?.Kill();

        wobbleTween = DOTween.To(
            () => 0f,
            v =>
            {
                float wobble = Mathf.Sin(v) * wobbleAmount;
                transform.rotation = Quaternion.Euler(0, 0, baseTiltRotation + wobble);
            },
            Mathf.PI * 2f,
            wobbleSpeed
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }


    private void CreateOutlineRenderer()
    {
        GameObject outlineObj = new GameObject("OutlineRenderer");
        outlineObj.transform.SetParent(transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localScale = Vector3.one * outlineScale;

        outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = null; // assigned after ApplyColorSprite()
        outlineRenderer.sortingLayerID =  sRend.sortingLayerID;
        outlineRenderer.sortingOrder = sRend.sortingOrder - 1;
        outlineRenderer.color = new Color(1, 1, 1, 0); // start invisible
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenHit) return;
        //if (!other.CompareTag("Player")) return;

        hasBeenHit = true;
        col.enabled = false;

        manager.OnBalloonHit(this);

        // fade
        sRend.DOFade(0f, fadeDuration);

        // particles
        if (hitParticles != null)
            Instantiate(hitParticles, transform.position, Quaternion.identity).Play();

        // audio
        if (hitSound != null)
            AudioHelpers.PlayClipWithVariation(hitSound, AudioChannel.SFX, Camera.main.transform.position);

        Destroy(this.transform.parent.gameObject, fadeDuration + 0.05f);
    }

     // ---------------------------------------------------------
    // Cleanup on obstacle end
    // ---------------------------------------------------------
    public void ForceKill()
    {

        glowTween?.Kill();

        sRend.DOFade(0f, fadeDuration);

        // particles
        if (hitParticles != null)
            Instantiate(hitParticles, transform.position, Quaternion.identity).Play();

        // audio
        if (hitSound != null)
            AudioHelpers.PlayClipWithVariation(hitSound, AudioChannel.SFX, Camera.main.transform.position);

        if (outlineRenderer != null)
            outlineRenderer.DOFade(0f, fadeDuration);

        Destroy(this.transform.parent.gameObject, fadeDuration + 0.05f);
    }
}
