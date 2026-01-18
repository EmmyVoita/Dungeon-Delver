using UnityEngine;
using DG.Tweening;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float modifier = 1f;
    [SerializeField] private float impulse = 1f;

    private Tween impulseTween;
    private Tween baseTween;
    private Tween modifierTween;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
    }

    // ------------------------------
    // Public Controls
    // ------------------------------

    public void SetBaseScale(float newBase, float duration = 0.2f)
    {
        baseTween?.Kill();
        baseTween = DOTween.To(() => baseScale, x => baseScale = x, newBase, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .OnUpdate(ApplyCombinedScale);
    }

    public void SetModifier(float newModifier, float duration = 0.2f)
    {
        modifierTween?.Kill();
        modifierTween = DOTween.To(() => modifier, x => modifier = x, newModifier, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .OnUpdate(ApplyCombinedScale);
    }

    public void ResetAll(float duration = 0.3f)
    {
        SetBaseScale(1f, duration);
        SetModifier(1f, duration);
    }

    // ------------------------------
    // Core Combiner
    // ------------------------------
    private void ApplyCombinedScale()
    {
        Time.timeScale = Mathf.Clamp(baseScale * modifier * impulse, 0f, 2f);
    }

    public float GetCurrentScale() => Time.timeScale;

    public void PlayImpulseSlow(
        float slowMultiplier,
        float inDuration,
        float holdDuration,
        float outDuration
    )
    {
        
        impulseTween?.Kill();

        impulseTween = DOTween.Sequence()
            .SetUpdate(true)

            // Ease into slow
            .Append(DOTween.To(() => impulse, x => impulse = x, slowMultiplier, inDuration)
                .SetEase(Ease.OutSine)
                .OnUpdate(ApplyCombinedScale))

            // Hold slow
            .AppendInterval(holdDuration)

            // Ease back to normal
            .Append(DOTween.To(() => impulse, x => impulse = x, 1f, outDuration)
                .SetEase(Ease.InSine)
                .OnUpdate(ApplyCombinedScale));
        
    }

}
