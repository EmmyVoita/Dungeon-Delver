using UnityEngine;
using TMPro;

public class CritComboUI : MonoBehaviour
{
    [Header("Combo Visuals")]
    public Transform particleSpawnPosition;
    public Gradient comboGradient;
    public GameObject particleEffectPrefab;
    public int startPSAtComboNum = 5;
    public int maxComboColor = 20;
    public TextMeshProUGUI comboText;
    public GameObject iconImage;

    [Header("Animation Settings")]
    public float basePopScale = 1.2f;
    public float scaleStep = 1.0f;
    public float maxScale = 5f;
    public float popScale = 1.5f;
    public float popDuration = 0.2f;

    [Header("Crit Multiplier Settings")]
    public float baseMultiplier = 1.0f;
    public float perComboIncrement = 0.1f;
    public float maxMultiplier = 5f; // optional clamp for sanity

    [Header("Hide Behavior")]
    public float hideDelay = 2f;
    public static System.Action<int> ExtendCritComboWindow;

    private Vector3 originalScale;
    private Coroutine popRoutine;
    private float lastUpdateTime;
    private bool hasReset = false;

    void Awake()
    {
        originalScale = comboText.transform.localScale;
        //comboText.text = "";
        hasReset = true;
        //iconImage.SetActive(false);
    }

    void OnEnable()
    {
        //ComboManager.OnCritComboUpdated += UpdateCombo;
        ExtendCritComboWindow += ExtendComboWindow;
    }

    void OnDisable()
    {
        //ComboManager.OnCritComboUpdated -= UpdateCombo;
        ExtendCritComboWindow -= ExtendComboWindow;
    }

    void ExtendComboWindow(int extraTime)
    {
        hideDelay += extraTime;
        Debug.Log("Extended crit combo window by " + extraTime + " seconds.");
    }

    void UpdateCombo(int count)
    {
        iconImage.SetActive(true);
        lastUpdateTime = Time.time;
        hasReset = false;

        // --- Calculate multiplier ---
        float multiplier = baseMultiplier + count * perComboIncrement;
        multiplier = Mathf.Min(multiplier, maxMultiplier);

        // --- Format text ---
        comboText.text = "x" + multiplier.ToString("0.0"); // show one decimal (e.g., x1.2)
        comboText.color = comboGradient.Evaluate(Mathf.Min((float)count / maxComboColor, 1f));

        // --- Scale animation ---
        float targetScale = Mathf.Min(basePopScale + (count - 1) * scaleStep, maxScale);
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopAnimation(targetScale));

        // --- Particle effect ---
        if (particleEffectPrefab != null && count >= startPSAtComboNum)
        {
            GameObject particleObj = Instantiate(particleEffectPrefab, particleSpawnPosition.position, Quaternion.identity);
            particleObj.transform.localScale *= targetScale;

            if (particleObj.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = comboText.color;
            }
        }
    }

    private System.Collections.IEnumerator PopAnimation(float targetScale)
    {
        float elapsed = 0f;

        // Scale up
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            comboText.transform.localScale = Vector3.Lerp(originalScale, originalScale * targetScale, t);
            yield return null;
        }

        // Scale back down
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
}
