using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class RandomQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] options; // [0] = option1, [1] = option2
    public TextMeshProUGUI timerText;
    public TextTypewriter typewriter;
    public Image timerFill;

    [Header("Visuals")]
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public Color correctFlashColor = Color.green;
    public Color wrongFlashColor = Color.red;
    public float selectedScale = 1.2f;
    public float transitionSpeed = 8f;
    public float feedbackDuration = 0.6f; // how long to show flash before closing

    [Header("Timer Settings")]
    public float maxAnswerTime = 5f;
    public Color normalTimerColor = Color.white;
    public Color warningTimerColor = Color.red;
    public float warningThreshold = 0.3f;

    [Header("Timer Sounds")]
    public AudioClip tickSound;
    public bool pitchIncreases = true; // true = higher pitch each tick, false = lower
    public float pitchRange = 0.3f; // how much pitch can vary total
    public float basePitch = 1f; // starting pitch


    private Action<bool> onAnswer;
    private int selectedIndex = 0;
    private bool inputLocked = false;
    private bool timerActive = false;
    private float timeRemaining;
    private int lastDisplayedSecond = -1;
    private QuestionData currentQuestion;

    // ----------------------------------------------------
    // Show Question
    // ----------------------------------------------------
    public void ShowQuestion(QuestionData data, Action<bool> callback)
    {
        //ScreenDimmerManager.Instance.HideWholeGameScreen();
        onAnswer = callback;
        currentQuestion = data;

        if (options.Length < 2)
        {
            Debug.LogError("⚠️ RandomQuestionUI: Need exactly 2 options!");
            return;
        }

        typewriter.StartTyping(data.questionText);
        options[0].text = data.option1Text;
        options[1].text = data.option2Text;

        selectedIndex = 0;
        UpdateVisuals(forceInstant: true);

        // Timer setup
        timeRemaining = maxAnswerTime;
        timerActive = true;
        lastDisplayedSecond = Mathf.CeilToInt(timeRemaining);

        UpdateTimerUI();

        // Fade in
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
    }



    void Update()
    {
        if (inputLocked) return;

        HandleInput();
        AnimateSelection();
        UpdateTimer();
    }

    // ----------------------------------------------------
    // Timer Logic
    // ----------------------------------------------------
    void UpdateTimer()
    {
        if (!timerActive) return;

        timeRemaining -= Time.unscaledDeltaTime;
        timeRemaining = Mathf.Max(timeRemaining, 0f);

        int currentSecond = Mathf.CeilToInt(timeRemaining);
        if (currentSecond != lastDisplayedSecond)
        {
            lastDisplayedSecond = currentSecond;
            UpdateTimerUI();
            PlayTickSound(currentSecond);
        }

        if (timeRemaining <= 0f)
        {
            timerActive = false;
            Timeout();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (timerFill != null)
        {
            float t = timeRemaining / maxAnswerTime;
            timerFill.fillAmount = t;
            timerFill.color = (t < warningThreshold) ? warningTimerColor : normalTimerColor;
        }
    }

    void PlayTickSound(int second)
    {
        if (tickSound == null) return;

        float progress = 1f - (second / maxAnswerTime); // 0 → 1 over time
        float pitchOffset = (pitchIncreases ? progress : -progress) * pitchRange;
        float finalPitch = basePitch + pitchOffset;

        AudioHelpers.PlayMyClipAtPoint(tickSound, AudioChannel.SFX, Camera.main.transform.position, finalPitch);
    }

    void Timeout()
    {
        Debug.Log("⏰ Time ran out! Auto-failing question.");
        SelectAnswer(false);
    }

    // ----------------------------------------------------
    // Answer Selection + Feedback
    // ----------------------------------------------------
    void SelectAnswer(bool choseOption1)
    {
        inputLocked = true;
        timerActive = false;

        bool wasCorrect = choseOption1 ? currentQuestion.option1IsCorrect : currentQuestion.option2IsCorrect;
        TextMeshProUGUI chosenOption = options[choseOption1 ? 0 : 1];

        // Flash green/red
        Color flashColor = wasCorrect ? correctFlashColor : wrongFlashColor;
        chosenOption.DOColor(flashColor, 0.1f).SetUpdate(true)
            .OnComplete(() =>
            {
                chosenOption.DOColor(defaultColor, 0.3f).SetDelay(0.2f).SetUpdate(true);
            });

        // Wait before fading out
        DOVirtual.DelayedCall(feedbackDuration, () =>
        {
            // Instead of destroying the whole object,
            // just hide the question panel.
            canvasGroup.DOFade(0f, 0.3f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    // Disable question visuals
                    //gameObject.SetActive(false);
                });

            // Notify ability whether answer was correct
            onAnswer?.Invoke(wasCorrect);

        }).SetUpdate(true);
    }

    // ----------------------------------------------------
    // Input Navigation
    // ----------------------------------------------------
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            SelectAnswer(selectedIndex == 0);
        }
    }

    // ----------------------------------------------------
    // Visual Feedback
    // ----------------------------------------------------
    void UpdateVisuals(bool forceInstant = false)
    {
        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = (i == selectedIndex);
            options[i].color = isSelected ? selectedColor : defaultColor;

            float targetScale = isSelected ? selectedScale : 1f;
            if (forceInstant)
                options[i].transform.localScale = Vector3.one * targetScale;
            else
                options[i].transform.DOScale(targetScale, 0.2f).SetUpdate(true);
        }
    }

    void AnimateSelection()
    {
        for (int i = 0; i < options.Length; i++)
        {
            float targetScale = (i == selectedIndex) ? selectedScale : 1f;
            options[i].transform.localScale = Vector3.Lerp(
                options[i].transform.localScale,
                Vector3.one * targetScale,
                Time.unscaledDeltaTime * transitionSpeed
            );
        }
    }
}
