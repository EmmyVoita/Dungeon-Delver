using UnityEngine;
using TMPro;
using DG.Tweening;
  using UnityEngine.VFX;

public class ComboUI : MonoBehaviour
{
    [Header("Combo Visuals")]
    public Transform parentTransform;
    public Transform particleSpawnPosition;
    public GameObject particleEffectPrefab;
    public Gradient comboGradient;
    public int psComboCountThreshold = 15;

    [Header("Text Settings")]
    public string comboPrefix = "X";
    public TextMeshProUGUI comboText;
    public int maxComboColor = 20;
    public float basePopScale = 1.2f;
    public float scaleStep = 1.0f;
    public float maxScale = 5f;
    public float popDuration = 0.2f;

    [Header("Tilt Settings")]
    public float maxTiltAngle = 12f;
    public float tiltReturnDuration = 0.3f;
    public Ease tiltEase = Ease.OutBack;

    [Header("Break Shake Settings")]
    public float shakeDuration = 0.4f;
    public float shakeStrength = 20f;
    public int shakeVibrato = 30;
    public float shakeRandomness = 90f;
    public Ease shakeEase = Ease.OutQuad;

    [Header("Hide Behavior")]
    public float hideDelay = 2f;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Vector3 originalPos;
    private Coroutine popRoutine;
    private Tween tiltTween;
    private Tween shakeTween;
    private float lastUpdateTime;
    private bool hasReset = false;

    void Awake()
    {
        originalScale = comboText.transform.localScale;
        originalRotation = comboText.transform.localRotation;
        originalPos = comboText.transform.localPosition;
        comboText.text = comboPrefix + "0";
        hasReset = true;
    }

    void OnEnable()
    {
        ComboManager.OnComboUpdated += UpdateCombo;
        ComboManager.OnComboUpdated += PlayBreakShake; // ✅ subscribe to combo break
        ComboManager.OnComboAccent += HandleAccent;
        GameStateManager.OnStateChanged += HandleUIState;
    }

    void OnDisable()
    {
        ComboManager.OnComboUpdated -= UpdateCombo;
        ComboManager.OnComboUpdated -= PlayBreakShake;
        ComboManager.OnComboAccent -= HandleAccent;
        GameStateManager.OnStateChanged -= HandleUIState;
    }


    private void HandleUIState(GameState previous, GameState newState)
    {
        if (newState == GameState.UpgradeSelection)
        {
            comboText.DOColor(Color.clear, 0.3f);
        }

        if(previous == GameState.UpgradeSelection)
        {
            comboText.DOColor(Color.white, 0.3f);
        }
    }

    void HandleAccent(int combo, int accentIndex)
    {
        float accentBoost =
            Mathf.Min(1.2f + accentIndex * 0.05f, 1.6f);

        PunchScale(accentBoost);
    }

  

    void PunchScale(float boost)
    {
        if (particleEffectPrefab != null)
        {
            GameObject particleObj = Instantiate(
                particleEffectPrefab,
                particleSpawnPosition.position,
                Quaternion.identity
            );

            if (particleObj.TryGetComponent(out VisualEffect vfx))
            {
                vfx.SetVector4("Main Color", comboText.color * 1.5f);
            }
        }

        parentTransform.DOPunchScale(
            Vector3.one * 0.3f * boost,
            0.2f,
            10,
            1f
        );
    }



    void UpdateCombo(int count)
    {
        comboText.text = comboPrefix + count;
        lastUpdateTime = Time.time;
        hasReset = false;

        // Color & scale by combo
        float t = Mathf.Min((float)count / maxComboColor, 1f);
        comboText.color = comboGradient.Evaluate(t);
        float targetScale = Mathf.Min(basePopScale + (count - 1) * scaleStep, maxScale);

        // --- Tilt left/right ---
        float tiltAngle = Random.Range(-maxTiltAngle, maxTiltAngle);
        tiltTween?.Kill();
        comboText.transform.localRotation = originalRotation;
        tiltTween = comboText.transform.DOLocalRotate(
            new Vector3(0, 0, tiltAngle),
            popDuration * 0.8f
        ).SetEase(tiltEase)
         .OnComplete(() =>
            comboText.transform.DOLocalRotateQuaternion(originalRotation, tiltReturnDuration)
                              .SetEase(Ease.InOutSine)
         );

        // --- Pop scale animation ---
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopAnimation(targetScale));

        /*
        // --- Particle burst ---
        if (particleEffectPrefab != null && count >= psComboCountThreshold)
        {
            GameObject particleObj = Instantiate(particleEffectPrefab, particleSpawnPosition.position, Quaternion.identity);
            particleObj.transform.localScale *= targetScale;

            if (particleObj.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = comboText.color;
            }
        }
        */
    }

    private System.Collections.IEnumerator PopAnimation(float targetScale)
    {
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            comboText.transform.localScale = Vector3.Lerp(originalScale, originalScale * targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            comboText.transform.localScale = Vector3.Lerp(originalScale * targetScale, originalScale, t);
            yield return null;
        }

        comboText.transform.localScale = originalScale;
    }

    // 🔥 Shake when combo breaks
    public void PlayBreakShake(int combo)
    {
        if (combo > 0) return; // only on break to 0
        
        shakeTween?.Kill();
        comboText.transform.localPosition = originalPos; // reset

        // Shake position (can also use DOShakeRotation if you want angular)
        shakeTween = comboText.transform.DOShakePosition(
            shakeDuration,
            shakeStrength,
            shakeVibrato,
            shakeRandomness,
            false,
            true
        ).SetEase(shakeEase)
         .OnComplete(() => comboText.transform.localPosition = originalPos);

        // Optional: flash red color briefly
        comboText.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
    }
}
