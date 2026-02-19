using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct RatingDisplayData
{
    [Range(0f, 1f)]
    [Tooltip("0 - 1")]
    public float accuracyThreshold;  
    public string ratingText;
    public AudioClip ratingSound;
    public Gradient gradient;
    public bool animateGradient;
    public GameObject effect;
}

[Serializable]
public struct StatRowData
{
    public string prefixLabel;
    public StatValueType statValueType;
    public ScoreSource scoreSource;
}

public enum StatValueType
{
    ScoreSource,   // comes from ScoreManager breakdown
    TotalScore,
    Hits,
}



public class RoundStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIFadeGroup continuePrompt;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private RatingTextPresenter ratingPresenter;

    //[SerializeField] private TextMeshProUGUI hitsRatingText;    
    //[SerializeField] private TextMeshProUGUI critsRatingText;
    [SerializeField] private TextMeshProUGUI ratingTextObject;
    [SerializeField] private StatRowAnimator imageStatRowAnimator;
    //[SerializeField] private StatRowAnimator imageStatRowAnimator2;
    //[SerializeField] private StatRowAnimator imageStatRowAnimator3;



    [Header("Display Settings")]
    [SerializeField] private List<RatingDisplayData> ratingDisplayData;
    //[SerializeField] private string hitsTextPrefix = "Hits: ";
    //[SerializeField] private string critsTextPrefix = "Crits: ";


    [Header("Display Sequence Settings")]
    [SerializeField] private float individualDisplayDelay = 0.5f;
    [SerializeField] private float ratingDisplayEffectsDelay = 0.5f;
    [SerializeField] private float acceptInputDelay = 0.5f;


    [Header("Audio Settings")]
    [SerializeField] private AudioClip animateStatCompleteSound;
    [SerializeField] private float animateStatPitchStep = 0.2f;

    [SerializeField] private AudioClip animateTickSound;
    [SerializeField] private float animateTickMinInterval = 0.05f;   // ← FIXED
    [SerializeField] private float animateTickPitchStart = 1.0f;
    [SerializeField] private float animateTickPitchIncrease = 0.02f;
    private bool skipRequested = false;





    public static Action OnContinuePressed;



   


    [Header("Audio & VFX")]
    public AudioClip showSound;
    public AudioClip backgroundMusic;
    public List<ParticleSystem> confettiSystems;

    [Header("Timing")]
    public float countDuration = 1.5f;   // how long the counter animation should last
    public float lingerAfterCount = 1f;  // how long before showing rating
    public float fadeInDuration = 0.4f;  // how fast rating fades in
    private float ratingPercentage = 0f;




    public float rowOutroDelay = 0.2f;
    public int playRatingImageAtIndex = 1;


    [Header("References")]
    [SerializeField] private RectTransform statRowContainer;
    [SerializeField] private GameObject statRowAnimatorPrefab;

    [SerializeField] private List<StatRowData> breakdownRowDataList;

    private List<StatRowAnimator> breakdownRowAnimators = new List<StatRowAnimator>();
    private List<string> prefixTexts = new List<string>();
    private List<ScoreSource> scoreSources = new List<ScoreSource>();

    [SerializeField] private SoundEffect rowIntroSoundEffect;
    [SerializeField] private SoundEffect rowOutroSoundEffect;

    private bool acceptInput = false;


    private bool BeakdownAnimatorsExists => breakdownRowAnimators.Count > 0;



    void OnEnable()
    {
        GameStateManager.OnStateChanged += ShowStats;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= ShowStats;
    }

    void Awake()
    {
        
    }   

    void Start()
    {
        mainContainer?.SetActive(false);
        continuePrompt?.Hide(true);
    }

    void ShowStats(GameState previousState, GameState newState)
    {
        if(newState != GameState.RoundSummary || 
           previousState == GameState.Paused) 
           return;

        skipRequested = false;
        StartCoroutine(DisplaySequence());

        ScreenDimmerManager.Instance.AddDimSource("roundstats");
        acceptInput = true;
    }


    void Update()
    {
        if(GameStateManager.Instance.CurrentState != GameState.RoundSummary && 
           GameStateManager.Instance.CurrentState != GameState.RoundSummaryEnd) 
           return;

        if (!acceptInput) return;

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            Debug.Log("Continue pressed on Round Summary screen." + $"Gamestate: {GameStateManager.Instance.CurrentState} ");
            // If still animating → request skip
            if (GameStateManager.Instance.CurrentState == GameState.RoundSummary)
            {
                skipRequested = true;
            }
            // If animation finished → proceed
            else if (GameStateManager.Instance.CurrentState == GameState.RoundSummaryEnd)
            {
                StartCoroutine(PlayOutroAnimations());
                ScreenDimmerManager.Instance.RemoveDimSource("roundstats");
                acceptInput = false;
            }
        }
    }   

    private Vector2Int ResolveStatValue(StatRowData data)
    {
        switch (data.statValueType)
        {
            case StatValueType.ScoreSource:
                int count = ScoreManager.Instance
                    .GetBreakdown()
                    .GetValueOrDefault(data.scoreSource);
                return new Vector2Int(count, -1);

            case StatValueType.TotalScore:
                return new Vector2Int(ScoreManager.Instance.RoundScoreTotal, -1);

            case StatValueType.Hits:
                return new Vector2Int(RoundManager.Instance.stats.Hit, RoundManager.Instance.stats.Spawned);

            default:
                return Vector2Int.zero;
        }
    }



    private void CreateBreakdownRowAnimators()
    {
        for(int i = 0; i < breakdownRowDataList.Count; i++)
        {
            GameObject rowObj = Instantiate(statRowAnimatorPrefab, statRowContainer);
            StatRowAnimator rowAnimator = rowObj.GetComponentInChildren<StatRowAnimator>();   
 
            breakdownRowAnimators.Add(rowAnimator);
            scoreSources.Add(breakdownRowDataList[i].scoreSource);
            prefixTexts.Add(breakdownRowDataList[i].prefixLabel);
        }
    }

    private IEnumerator PlayBreakdownIntros()
    {
        var breakdown = ScoreManager.Instance.GetBreakdown();

        for(int i = 0; i < breakdownRowAnimators.Count; i++)
        {
            if (skipRequested)
                break;

            if (i == playRatingImageAtIndex)
                yield return StartCoroutine(PlayRatingImageIntro());

            TextMeshProUGUI displayText = breakdownRowAnimators[i].GetComponentInChildren<TextMeshProUGUI>();
        
            breakdownRowAnimators[i].PlayIntro();

            Vector2Int scoreValue = ResolveStatValue(breakdownRowDataList[i]);

            yield return StartCoroutine(AnimateStatText(targetText: displayText, 
                                                        count: scoreValue.x,
                                                        total: scoreValue.y,
                                                        prefix: prefixTexts[i]));  

            float pitch = 1.0f + (i * animateStatPitchStep);
            AudioHelpers.PlaySoundEffect(rowIntroSoundEffect, Camera.main.transform.position, pitch);

            yield return StartCoroutine(WaitOrSkip(individualDisplayDelay));
        }

        if (skipRequested)
        {
            var roundAccuracy = RoundManager.Instance.stats.RoundAccuracy;
            RatingDisplayData chosenImageData = GetRatingForAccuracy(roundAccuracy);

            ratingTextObject.text = chosenImageData.ratingText;

            imageStatRowAnimator.SnapToFinal();

            for (int i = 0; i < breakdownRowAnimators.Count; i++)
            {
                breakdownRowAnimators[i].SnapToFinal();

                TextMeshProUGUI displayText =
                    breakdownRowAnimators[i].GetComponentInChildren<TextMeshProUGUI>();

                Vector2Int scoreValue = ResolveStatValue(breakdownRowDataList[i]);

                // Directly set final text (NO animation coroutine)
                if (scoreValue.y > 0)
                    displayText.text = $"{prefixTexts[i]}{scoreValue.x}/{scoreValue.y}";
                else
                    displayText.text = $"{prefixTexts[i]}{scoreValue.x}";
            }

            // Snap rating instantly
            var chosenData = GetRatingForAccuracy(RoundManager.Instance.stats.RoundAccuracy);
            ratingPresenter.ShowRating(chosenData);

            yield break; // ← IMPORTANT
        }


    }

    private IEnumerator PlayRatingImageIntro()
    {
        var roundAccuracy = RoundManager.Instance.stats.RoundAccuracy;
        RatingDisplayData chosenData = GetRatingForAccuracy(roundAccuracy);

        ratingPresenter.ShowRating(chosenData);

        imageStatRowAnimator.PlayIntro();

        yield return new WaitForSeconds(ratingDisplayEffectsDelay);

        AudioHelpers.PlayMyClipAtPoint(chosenData.ratingSound, AudioChannel.UI, Camera.main.transform.position, 1.0f);
        if (chosenData.effect != null)  Instantiate(chosenData.effect, ratingTextObject.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(individualDisplayDelay);
    }

    private IEnumerator PlayBreakdownRowOutroAnimations()
    {
        for(int i = 0; i < breakdownRowAnimators.Count; i++)
        {
            breakdownRowAnimators[i].PlayOutro();
            AudioHelpers.PlaySoundEffect(rowOutroSoundEffect, Camera.main.transform.position);
            yield return new WaitForSeconds(rowOutroDelay);
        }
    }

    private IEnumerator WaitOrSkip(float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (skipRequested)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }



    private IEnumerator DisplaySequence()
    {
        if (!BeakdownAnimatorsExists)
        CreateBreakdownRowAnimators();

        mainContainer?.SetActive(true);

        

        if (continuePrompt.TryGetComponent(out TextMeshProUGUI textComponent))
        {
            textComponent.text = $"[<color=#FFD700>{InputBindingManager.Instance.GetKey(InputActionType.Confirm)}</color>] to continue";
            continuePrompt?.Show();
        }


        AudioHelpers.PlayMyClipAtPoint(showSound, AudioChannel.UI, Camera.main.transform.position, 1.0f);

        foreach (var ps in confettiSystems)
                if (ps != null) ps.Play();

    
        yield return StartCoroutine(PlayBreakdownIntros());

           
        yield return new WaitForSeconds(acceptInputDelay);

        GameStateManager.Instance.SetState(GameState.RoundSummaryEnd);
    }

    public IEnumerator PlayOutroAnimations()
    {
        AudioSettingsManager.PlaySelectSound();
        OnContinuePressed?.Invoke();

        imageStatRowAnimator.PlayOutro();
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(PlayBreakdownRowOutroAnimations());

        foreach (var ps in confettiSystems)
            if (ps != null) ps.Stop();

        mainContainer?.SetActive(false);
        continuePrompt?.Hide();

        yield return new WaitForSeconds(0.5f);
    }
    
    // Helper functions
    // ---------------------------------------------------------------------------------------------

    private RatingDisplayData GetRatingForAccuracy(float accuracy)
    {
        RatingDisplayData best = ratingDisplayData[0];
        float bestThreshold = -1f;

        foreach (var data in ratingDisplayData)
        {
            if (accuracy >= data.accuracyThreshold && data.accuracyThreshold > bestThreshold)
            {
                best = data;
                bestThreshold = data.accuracyThreshold;
            }
        }

        return best;
    }


    private IEnumerator AnimateStatText(
    TextMeshProUGUI targetText,
    int count,
    int total = -1,
    string prefix = "",
    string suffix = "",
    Action onComplete = null)
    {
        if (targetText == null)
        {
            Debug.LogWarning("AnimateStatText called with null TMP target!");
            yield break;
        }

        targetText.gameObject.SetActive(true);

        float elapsed = 0f;
        int displayedHits = 0;

        // Tick sound timing
        float lastTickTime = -999f;
        float currentPitch = animateTickPitchStart;

        // Helper text formatter
        string BuildText(int current)
        {
            if (total > 0)
                return $"{prefix}{current}/{total}{suffix}";
            else
                return $"{prefix}{current}{suffix}";
        }

        // Initialize
        targetText.text = BuildText(0);

        while (displayedHits < count)
        {
            if (skipRequested)
            {
                targetText.text = BuildText(count);
                yield break;
            }


            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            int newValue = Mathf.FloorToInt(Mathf.Lerp(0, count, smooth));

            // Only update + tick when value actually changes
            if (newValue != displayedHits)
            {
                displayedHits = newValue;
                targetText.text = BuildText(displayedHits);

                // 🔊 Play tick with min interval spacing
                if (animateTickSound != null &&
                    Time.time - lastTickTime >= animateTickMinInterval)
                {
                    AudioSettingsManager.PlayTallySound(currentPitch, 1f);

                    currentPitch += animateTickPitchIncrease;
                    lastTickTime = Time.time;
                }
            }

            yield return null;
        }

        // Snap to final
        targetText.text = BuildText(count);

        // Small pause before next step
        yield return new WaitForSeconds(lingerAfterCount);

        onComplete?.Invoke();
    }



}
