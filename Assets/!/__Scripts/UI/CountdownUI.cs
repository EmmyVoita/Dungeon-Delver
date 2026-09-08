using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEditor;
using DG.Tweening;

public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private RectTransform textRect;
    [SerializeField] private CountdownHandController handController;

    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;

    [Header("Audio")]
    [SerializeField] private SoundEffect countdownBeep;
    [SerializeField] private SoundEffect finalBeep;
    [SerializeField] private float beepPitchIncrement = 0.1f;
    [SerializeField] private float beepVolumeIncrement = 0.1f;
    
    [Header("Text Animation")]
    [SerializeField] private Ease popInEase = Ease.OutBack;
    [SerializeField] private Ease popOutEase = Ease.InBack;
    [SerializeField] private Ease rotationEase = Ease.OutSine;
    [SerializeField] private float randomRotation = 15f;
    [SerializeField] private float rotateDuration = 0.2f;
    [SerializeField] private float scaleInDuration = 0.15f;
    [SerializeField] private float scaleOutDuration = 0.15f;
    [SerializeField] private float textHoldDuration = 0.25f;

    private Coroutine currentRoutine;
    private bool isCounting = false;
    private Sequence _NumberSequence;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        countdownText.text = "";
        textRect.localScale = Vector3.zero;
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
        textRect.localScale = Vector3.zero;

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
        //countdownText.gameObject.SetActive(false); // optional: no text

        // 🔥 HAND DRIVES EVERYTHING NOW
        //yield return StartCoroutine(handController.PlayCountdownRoutine());

        yield return new WaitForSecondsRealtime(startDelay);

        int i = 0;

        while(i < 3)
        {
            yield return new WaitUntil(() => !OverlayManager.Instance.IsPaused);
            float pitchMult = 1.0f + i * beepPitchIncrement;
            AudioHelpers.PlaySoundEffect(countdownBeep,transform.position,pitchMult, volumeScalar: 1f + beepVolumeIncrement * i);

            countdownText.text = $"{3-i}";
            textRect.localScale = Vector3.zero;

            _NumberSequence?.Kill();
            _NumberSequence = DOTween.Sequence();
            _NumberSequence.SetUpdate(true);


            _NumberSequence.Append(textRect.DOScale(1.2f, scaleInDuration).SetEase(popInEase));

            _NumberSequence.Join(countdownText.DOFade(1f, 0.1f));

            float angleZ = i % 2 == 0 ? randomRotation : -randomRotation;
            _NumberSequence.Join(textRect.DORotate(new Vector3(0,0,angleZ), rotateDuration)
                                 .SetEase(rotationEase));

            _NumberSequence.AppendInterval(textHoldDuration);

            _NumberSequence.Append(textRect.DOScale(0, scaleOutDuration).SetEase(popOutEase));
            _NumberSequence.Join(countdownText.DOFade(0f, 0.1f));

            yield return _NumberSequence.WaitForCompletion();

            i++;
        }

        isCounting = false;
        currentRoutine = null;

        onComplete?.Invoke();
    }


}
