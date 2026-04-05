using UnityEngine;
using DG.Tweening;

public class ComboStarAnimator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer mainStar;      // Central star
    public SpriteRenderer outlineStar;   // Optional glow layer
    public ParticleSystem burstParticles;

    [Header("Shader Properties")]
    public string colorScalarProperty = "_ColorScalar"; // Must match Shader Graph property
    public Color baseColor = Color.white;
    public Gradient colorGradient;
    public float baseScalar = 1f;       // Idle brightness
    public float maxScalar = 3f;        // Max brightness
    public float scalarLerpSpeed = 2f;  // How quickly it reacts to combo changes
    public float burstBoost = 1.5f;     // Extra brightness when combo increases
    private Material starMaterial;
    private float currentScalar;

    [Header("Combo Thresholds")]
    public int appearThreshold = 5;
    public int colorShiftThreshold = 10;
    public int maxCombo = 20;

    [Header("Animation Settings")]
    public float appearScale = 1.4f;
    public float pulseScaleMultiplier = 1.1f;
    public float appearTime = 0.4f;
    public float pulseSpeed = 0.8f;
    public float idleRotationSpeed = 30f;
    public float burstRotationSpeed = 400f;
    public float fadeOutDuration = 0.4f;
    public float colorShiftSpeed = 0.2f;

    private Tween rotationTween;
    private Tween pulseTween;
    private Tween colorTween;
    private bool isActive;
    private float baseScale;

    void Start()
    {
        if (mainStar != null)
            starMaterial = mainStar.material;

        HideInstant();
    }

    void Update()
    {
        int combo = 0;//ComboManager.Instance.GetComboCount();

        // Activate when reaching combo threshold
        if (!isActive && combo >= appearThreshold)
            ActivateStar();

        // Color shifting at high combos
        if (isActive && combo >= colorShiftThreshold)
            AnimateColorShift(combo);

        // Brightness + alpha intensity tied to combo progress
        if (isActive && starMaterial != null)
        {
            float t = Mathf.Clamp01((float)combo / maxCombo);

            // --- Brightness ---
            float targetScalar = Mathf.Lerp(baseScalar, maxScalar, t);
            currentScalar = Mathf.Lerp(currentScalar, targetScalar, Time.deltaTime * scalarLerpSpeed);
            starMaterial.SetFloat(colorScalarProperty, currentScalar);

            // --- Alpha control (dimmer at low combo) ---
            UpdateStarAlpha(t);
        }
    }

    void UpdateStarAlpha(float comboT)
    {
        // ComboT = 0 → barely visible, ComboT = 1 → fully bright
        float mainAlpha = Mathf.Lerp(1f, 1f, comboT);
        float outlineAlpha = Mathf.Lerp(0.3f, 1f, comboT);

        Color mainColor = mainStar.color;
        Color outlineColor = outlineStar.color;

        mainColor.a = Mathf.Lerp(mainColor.a, mainAlpha, Time.deltaTime * 4f);
        outlineColor.a = Mathf.Lerp(outlineColor.a, outlineAlpha, Time.deltaTime * 4f);

        mainStar.color = mainColor;
        outlineStar.color = outlineColor;
    }


    void ActivateStar()
    {
        isActive = true;
        baseScale = appearScale;

        // Reset initial state
        mainStar.color = baseColor;
        outlineStar.color = baseColor;
        transform.localScale = Vector3.one * 0.2f;

        // --- Appear tween ---
        transform.DOScale(appearScale, appearTime)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // --- Gentle pulsing (centered around appearScale) ---
                pulseTween?.Kill();
                pulseTween = transform
                    .DOScale(appearScale * pulseScaleMultiplier, pulseSpeed)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });

        mainStar.DOFade(1f, appearTime);
        outlineStar.DOFade(0.7f, appearTime);

        // --- Idle rotation ---
        rotationTween?.Kill();
        rotationTween = transform
            .DORotate(new Vector3(0, 0, 360f), 360f / idleRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        // --- Optional burst particles ---
        if (burstParticles) burstParticles.Play();

        // Reset brightness scalar
        if (starMaterial != null)
        {
            currentScalar = baseScalar;
            starMaterial.SetFloat(colorScalarProperty, baseScalar);
        }
    }

    public void OnComboIncreased()
    {
        if (!isActive) return;

        Debug.Log("Combo increased - playing burst animation.");

        // Flash brightness (shader + fade flash)
        if (starMaterial != null)
        {
            float boostedValue = Mathf.Min(currentScalar * burstBoost, maxScalar * 1.2f);
            starMaterial.DOFloat(boostedValue, colorScalarProperty, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        // Flash sprite alpha briefly
        mainStar.DOFade(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
        outlineStar.DOFade(1f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    public void EndCombo()
    {
        if (!isActive)
        {
            Debug.Log("Combo ended - stopping animations.");
            return;
        }

        isActive = false;

        rotationTween?.Kill();
        pulseTween?.Kill();
        colorTween?.Kill();

        // Fade out and shrink away
        mainStar.DOFade(0f, fadeOutDuration);
        outlineStar.DOFade(0f, fadeOutDuration);
        transform.DOScale(0f, fadeOutDuration).SetEase(Ease.InBack);

        if (starMaterial != null)
        {
            starMaterial.DOFloat(baseScalar, colorScalarProperty, fadeOutDuration);
        }

        if (burstParticles)
            burstParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void AnimateColorShift(int combo)
    {
        float t = Mathf.PingPong(Time.time * colorShiftSpeed, 1f);
        //Color hue = colorGradient.Evaluate(t);  
        Color hue = Color.Lerp(Color.white, Color.HSVToRGB(t, 1f, 1f), Mathf.Clamp01(combo / (float)maxCombo));

        mainStar.color = new Color(hue.r, hue.g, hue.b, mainStar.color.a);
        outlineStar.color = new Color(hue.r, hue.g, hue.b, outlineStar.color.a);
    }

    void HideInstant()
    {
        mainStar.color = new Color(1, 1, 1, 0);
        outlineStar.color = new Color(1, 1, 1, 0);
        transform.localScale = Vector3.zero;
        isActive = false;

        if (mainStar != null)
        {
            starMaterial = mainStar.material;
            starMaterial.SetFloat(colorScalarProperty, baseScalar);
        }
    }
}
