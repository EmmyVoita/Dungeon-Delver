using UnityEngine;
using TMPro;
using System.Collections;
using System.ComponentModel;
using UnityEngine.UI;

public class UIToast : MonoBehaviour
{
    public static UIToast Instance;


    [Header("References")]
    public RectTransform rootRect;
    public GameObject toastPrefab;

    [Header("Timing")]
    public float defaultDuration = 2f;

    [SerializeField, Range(0f, 1f)]
    private float colorVariation = 0.1f;

    private bool useLightVariation;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // -------------------------
    // Public API
    // -------------------------
    public static void Show(string message, float duration = -1f)
    {
        Show(message, Color.white, duration);
    }

    public static void Warn(string message, float duration = -1f)
    {
        Show(message, Color.yellow, duration);
    }

    public static void Error(string message, float duration = -1f)
    {
        Show(message, Color.red, duration);
    }

    private Color GetColorVariation(Color baseColor)
    {
        useLightVariation = !useLightVariation;

        if (useLightVariation)
        {
            return Color.Lerp(
                baseColor,
                Color.white,
                colorVariation
            );
        }

        return Color.Lerp(
            baseColor,
            Color.black,
            colorVariation
        );
    }

    public static void Show(string message, Color color, float duration = -1f)
    {
        if (Instance == null)
        {
            return;
        }

        GameObject obj = Instantiate(Instance.toastPrefab, Instance.rootRect);
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = message;
        text.color = Instance.GetColorVariation(color);

        LayoutRebuilder.ForceRebuildLayoutImmediate(Instance.rootRect);

        float lifetime = duration > 0 ? duration : Instance.defaultDuration;
        Instance.StartCoroutine(Instance.DestroyAfter(obj, lifetime));
    }

    // -------------------------
    IEnumerator DestroyAfter(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);

        LayoutRebuilder.ForceRebuildLayoutImmediate(Instance.rootRect);
    }
}
