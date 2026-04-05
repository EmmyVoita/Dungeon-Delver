using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float interval = 1f;
    [SerializeField] private string finalText = "GO!";
    [SerializeField] private AudioClip countdownBeep;
    [SerializeField] private SoundEffect finalBeep;
    [SerializeField] private float beepPitchIncrement = 0.1f;
    [SerializeField] private CountdownHandController handController;

    private Coroutine currentRoutine;
    private bool isCounting = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        countdownText.text = "";
    }

    /// <summary>
    /// Start a countdown and invoke the provided callback when done.
    /// </summary>
    public void BeginCountdown(Action onComplete = null)
    {
        if (isCounting) return; // prevent overlap
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(CountdownCoroutine(onComplete));
    }

    public void KillActiveCountdown(Action onComplete = null)
    {
        if (!isCounting) return; // prevent overlap
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        isCounting = false;
        currentRoutine = null;
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private IEnumerator CountdownCoroutine(Action onComplete)
    {
        if (countdownText == null)
        {
            Debug.LogWarning("CountdownUI: countdownText is null. Skipping countdown.");
            onComplete?.Invoke();
            yield break;
        }

        isCounting = true;
        countdownText.gameObject.SetActive(false); // optional: no text

        // 🔥 HAND DRIVES EVERYTHING NOW
        yield return StartCoroutine(handController.PlayCountdownRoutine());

        isCounting = false;
        currentRoutine = null;

        onComplete?.Invoke();
    }
}
