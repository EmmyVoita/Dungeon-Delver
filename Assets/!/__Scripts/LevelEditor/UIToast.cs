using UnityEngine;
using TMPro;
using System.Collections;

public class UIToast : MonoBehaviour
{
    public static UIToast Instance;


    [Header("References")]
    public Transform container;
    public GameObject toastPrefab;

    [Header("Timing")]
    public float defaultDuration = 2f;

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

    public static void Show(string message, Color color, float duration = -1f)
    {
        if (Instance == null)
        {
            return;
        }

        GameObject obj = Instantiate(Instance.toastPrefab, Instance.container);
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = message;
        text.color = color;

        float lifetime = duration > 0 ? duration : Instance.defaultDuration;
        Instance.StartCoroutine(Instance.DestroyAfter(obj, lifetime));
    }

    // -------------------------
    IEnumerator DestroyAfter(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }
}
