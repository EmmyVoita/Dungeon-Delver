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
    [SerializeField] private float beepPitchIncrement = 0.1f;

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

    private IEnumerator CountdownCoroutine(Action onComplete)
    {
        isCounting = true;
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            if (countdownBeep != null)
                AudioHelpers.PlayMyClipAtPoint(
                    countdownBeep,
                    AudioChannel.SFX,
                    Camera.main.transform.position,
                    pitch: 1 + (3 - i) * beepPitchIncrement
                );

            yield return new WaitForSecondsRealtime(interval);
        }

        countdownText.text = finalText;
        yield return new WaitForSecondsRealtime(0.5f);

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);

        isCounting = false;
        currentRoutine = null;

        onComplete?.Invoke();
    }
}
