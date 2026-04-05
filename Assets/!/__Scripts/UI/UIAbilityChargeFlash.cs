using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIAbilityChargeFlash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image targetImage;

    [Header("Flash Settings")]
    [SerializeField] private float flashAlpha = 1.5f;      // how bright the flash goes
    [SerializeField] private float flashDuration = 0.25f; // up + down time
    [SerializeField] private Ease flashEase = Ease.OutQuad;

    [Header("Shader Property")]
    [SerializeField] private string alphaProperty = "_Alpha"; // or "_Color" if needed

    private Material runtimeMat;
    private float baseAlpha = 1f;

    private Tween currentTween;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        // IMPORTANT: clone material so we don't mutate shared UI material
        runtimeMat = Instantiate(targetImage.material);
        targetImage.material = runtimeMat;

        // Cache starting alpha
        if (runtimeMat.HasProperty(alphaProperty))
        {
            baseAlpha = runtimeMat.GetFloat(alphaProperty);
        }
        else if (runtimeMat.HasProperty("_Color"))
        {
            baseAlpha = runtimeMat.color.a;
        }
        else
        {
            Debug.LogWarning($"UIAbilityChargeFlash: Material has no {alphaProperty} or _Color.a");
        }
    }

    void OnEnable()
    {
        // 🔔 Hook into your existing event
        Player.OnAbilityChargeChanged += OnAbilityChargeChanged;
    }

    void OnDisable()
    {
        Player.OnAbilityChargeChanged -= OnAbilityChargeChanged;
    }

    private void OnAbilityChargeChanged(int previous, int attemptedDelta, int appliedDelta)
    {
        if(appliedDelta <= 0)
            return; // only flash on gain
        PlayFlash();
    }

    public void PlayFlash()
    {
        currentTween?.Kill();

        float start = baseAlpha;
        float peak  = flashAlpha;

        // Tween up → down
        currentTween = DOTween.Sequence()
            .Append(DOTween.To(
                () => GetAlpha(),
                a => SetAlpha(a),
                peak,
                flashDuration * 0.5f
            ).SetEase(flashEase))

            .Append(DOTween.To(
                () => GetAlpha(),
                a => SetAlpha(a),
                start,
                flashDuration * 0.5f
            ).SetEase(flashEase));
    }

    private float GetAlpha()
    {
        if (runtimeMat.HasProperty(alphaProperty))
            return runtimeMat.GetFloat(alphaProperty);

        return runtimeMat.color.a;
    }

    private void SetAlpha(float value)
    {
        if (runtimeMat.HasProperty(alphaProperty))
        {
            runtimeMat.SetFloat(alphaProperty, value);
        }
        else
        {
            Color c = runtimeMat.color;
            c.a = value;
            runtimeMat.color = c;
        }
    }
}
