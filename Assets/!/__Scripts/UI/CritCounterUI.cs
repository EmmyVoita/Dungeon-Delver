using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.VFX;

public class CritCounterUI : MonoBehaviour
{
    [Header("Hierarchy")]
    [Tooltip("Wrapper that scales in/out for visibility")]
    public Transform scaleTransform;

    [Tooltip("TMP text transform (punch / tilt only)")]
    public TextMeshProUGUI textComponent;

    [Header("Crit Display")]
    public string comboPrefix = "X";
    [SerializeField] private int showThreshold = 2;

    [Header("Pop In / Out")]
    public float popInDuration = 0.25f;
    public float popOutDuration = 0.2f;
    public float overshootScale = 1.2f;

    [Header("Punch")]
    public float basePopScale = 1.2f;
    public float scaleStep = 0.15f;
    public float maxScale = 1.8f;
    public float popDuration = 0.12f;

    [Header("Tilt")]
    public float maxTiltAngle = 10f;
    public float tiltReturnDuration = 0.25f;
    public Ease tiltEase = Ease.OutBack;

    [Header("VFX")]
    public Transform particleSpawnPosition;
    public GameObject particleEffectPrefab;

    // -------------------------
    // Internal State
    // -------------------------

    private int previousValue = 0;
    private bool isVisible = false;

    private Vector3 originalTextScale;
    private Quaternion originalRotation;

    private Tween scaleTween;
    private Tween tiltTween;
    private Coroutine punchRoutine;

    // -------------------------
    // Lifecycle
    // -------------------------

    private void Awake()
    {
        originalTextScale = textComponent.transform.localScale;
        originalRotation = textComponent.transform.localRotation;

        textComponent.text = comboPrefix + "0";
        scaleTransform.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        ComboManager.OnCritStreakUpdated += HandleCritStreakUpdated;

        // sync immediately in case UI enables mid-streak
        int current = ComboManager.Instance != null
            ? ComboManager.Instance.CritsInARow
            : 0;

        previousValue = current;

        if (current >= showThreshold)
            PopIn();
        else
            scaleTransform.localScale = Vector3.zero;
    }

    private void OnDisable()
    {
        ComboManager.OnCritStreakUpdated -= HandleCritStreakUpdated;
        scaleTween?.Kill();
        tiltTween?.Kill();
    }

    // -------------------------
    // Core Logic
    // -------------------------

    private void HandleCritStreakUpdated(int current)
    {
        // ---- Visibility transitions ----
        if (previousValue < showThreshold && current >= showThreshold)
            PopIn();

        //if (previousValue >= showThreshold && current < showThreshold)
        if(current == 0)
            PopOut();

        previousValue = current;

        // ---- Update text ----
        textComponent.text = comboPrefix + current;

        if (!isVisible)
            return;

        // ---- Punch scale ----
        float targetScale = Mathf.Min(
            basePopScale + (current - showThreshold) * scaleStep,
            maxScale
        );

        if (punchRoutine != null)
            StopCoroutine(punchRoutine);

        punchRoutine = StartCoroutine(PunchScale(targetScale));

        // ---- Tilt ----
        float tilt = Random.Range(-maxTiltAngle, maxTiltAngle);

        tiltTween?.Kill();
        textComponent.transform.localRotation = originalRotation;

        tiltTween = textComponent.transform
            .DOLocalRotate(new Vector3(0, 0, tilt), popDuration)
            .SetEase(tiltEase)
            .OnComplete(() =>
                textComponent.transform
                    .DOLocalRotateQuaternion(originalRotation, tiltReturnDuration)
                    .SetEase(Ease.InOutSine)
            );

        // ---- VFX ----
        SpawnVFX();
    }

    // -------------------------
    // Animations
    // -------------------------

    public void PopIn()
    {
        if (isVisible) return;
        isVisible = true;

        scaleTween?.Kill();
        scaleTransform.localScale = Vector3.zero;

        scaleTween = scaleTransform
            .DOScale(overshootScale, popInDuration * 0.6f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
                scaleTransform
                    .DOScale(1f, popInDuration * 0.4f)
                    .SetEase(Ease.OutQuad)
            );
    }

    public void PopOut()
    {
        if (!isVisible) return;
        isVisible = false;

        textComponent.enabled = true;
        
        scaleTween?.Kill();

        Sequence seq = DOTween.Sequence();
        seq.Append(
            scaleTransform
                .DOScale(0.0f, popOutDuration)
                .SetEase(Ease.InBack)
        );
        

        scaleTween = seq;
        
    }

    private System.Collections.IEnumerator PunchScale(float targetScale)
    {
        float t = 0f;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            float lerp = t / popDuration;
            textComponent.transform.localScale =
                Vector3.Lerp(originalTextScale, originalTextScale * targetScale, lerp);
            yield return null;
        }

        t = 0f;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            float lerp = t / popDuration;
            textComponent.transform.localScale =
                Vector3.Lerp(originalTextScale * targetScale, originalTextScale, lerp);
            yield return null;
        }

        textComponent.transform.localScale = originalTextScale;
    }

    private void SpawnVFX()
    {
        if (particleEffectPrefab == null || particleSpawnPosition == null)
            return;

        GameObject obj = Instantiate(
            particleEffectPrefab,
            particleSpawnPosition.position,
            Quaternion.identity
        );

        if (obj.TryGetComponent(out VisualEffect vfx))
        {
            vfx.SetVector4("Main Color", textComponent.color * 1.2f);
        }
    }
}
