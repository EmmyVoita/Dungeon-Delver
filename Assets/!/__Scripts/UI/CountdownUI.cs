using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEditor;

public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float interval = 1f;
    [SerializeField] private string finalText = "GO!";
    [SerializeField] private SoundEffect countdownBeep;
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
        Debug.Log("Being Countdown");

        if (isCounting) return; // prevent overlap
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        

        currentRoutine = StartCoroutine(CountdownCoroutine(onComplete));
        Debug.Log("Starting Countdown");
    }

    public void KillActiveCountdown(Action onComplete = null)
    {
        Debug.Log("trying to killing Countdown");
        if (!isCounting)
        {
            onComplete?.Invoke();
            return;
        }  // prevent overlap
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        isCounting = false;
        currentRoutine = null;
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
        Debug.Log("killing Countdown");
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
        //yield return StartCoroutine(handController.PlayCountdownRoutine());

        yield return new WaitForSecondsRealtime(startDelay);

        int i = 0;

        while(i < 3)
        {
            yield return new WaitUntil(() => !OverlayManager.Instance.IsPaused);
            float pitchMult = 1.0f + i * beepPitchIncrement;
            AudioHelpers.PlaySoundEffect(countdownBeep,transform.position,pitchMult);
            yield return new WaitForSecondsRealtime(0.5f);
            i++;
        }

        isCounting = false;
        currentRoutine = null;

        onComplete?.Invoke();
    }
}
