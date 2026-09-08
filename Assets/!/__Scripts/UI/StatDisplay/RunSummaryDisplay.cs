using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;




public class RunSummaryDisplay : MonoBehaviour
{
    public static event Action OnContinuePressed;
    public static event Action OnStatsTallyComplete;


    [Header("References")]
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private RectTransform statRowContainer;
    [SerializeField] private GameObject statRowAnimatorPrefab;
    [SerializeField] private List<StatRowData> breakdownRowDataList;
    [SerializeField] private StatTextAnimator animator;
    [SerializeField] private ResultsPageController resultsPageController;

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;
    [SerializeField] private KeyPromptController keyPromptController;



    [Header("Timing")]
    [SerializeField] private float rowOutroDelay = 0.2f;
    [SerializeField] private float individualDisplayDelay = 0.5f;


    [Header("Audio & VFX")]
    [SerializeField] private float animateStatPitchStep = 0.2f;
    [SerializeField] private SoundEffect rowIntroSoundEffect;
    [SerializeField] private SoundEffect rowOutroSoundEffect;


    [Header("VFX")]
    [SerializeField] private List<ParticleSystem> confettiSystems;

    [Header("Background")]
    [SerializeField] private Material backgroundMaterial;
    [SerializeField] private string transtionPropertyName = "_Transition";
    [SerializeField] private float transitionInTargetValue = 1f;
    [SerializeField] private float transitionOutTargetValue = 1f;
    [SerializeField] private float transitionDuration = 0.5f;


    private List<GameObject> _rowObjs = new List<GameObject>();
    private List<StatRowAnimator> _rowAnimators = new List<StatRowAnimator>();
    private List<string> _prefixTexts = new List<string>();
    private List<ScoreSource> _scoreSources = new List<ScoreSource>();
    private bool _skipRequested = false;

    private bool BeakdownAnimatorsExists => _rowAnimators.Count > 0;
    private RunRecord _runRecord;
    private bool _isOpen;

    /*
    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;

    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }


    void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.Paused || previousState == GameState.Paused) return;
        
        bool wasActive = activeStates.Contains(previousState);
        bool isActive = activeStates.Contains(newState);

        // Entering active range
        if (!wasActive && isActive)
        {
            _skipRequested = false;
            Cleanup();
            StartCoroutine(DisplaySequence());
            mainContainer?.SetActive(true);
        }

        // Exiting active range
        if (wasActive && !isActive)
        {
            Cleanup();
            mainContainer?.SetActive(false);
        }
    }*/

    private void OnDestroy()
    {
        if(backgroundMaterial)
            backgroundMaterial.SetFloat(transtionPropertyName, transitionOutTargetValue);
    }


    public void Open(RunRecord runRecord)
    {
        _runRecord = runRecord;
        keyPromptController.PushPrompts(keyPrompts);

        InputFocusManager.Claim(this);

        backgroundMaterial.SetFloat(transtionPropertyName,transitionOutTargetValue);
        backgroundMaterial.DOFloat(transitionInTargetValue, transtionPropertyName, transitionDuration)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            _isOpen = true;
            _skipRequested = true;
            
            Cleanup();
            StartCoroutine(DisplaySequence());
            mainContainer?.SetActive(true);
        });
    }

    public void Close()
    {
        Cleanup();
        mainContainer?.SetActive(false);
        _isOpen = false;
        keyPromptController.PopPrompts();

        
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);

        backgroundMaterial.DOFloat(transitionOutTargetValue, transtionPropertyName, transitionDuration)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            resultsPageController.ShowPage(ResultsPageController.ResultsPage.Leaderboard, ()=>
            {
                InputFocusManager.Release(this);
            });
        });
    }

    void Update()
    {
        if(!InputFocusManager.HasFocus(this) || !_isOpen)
            return;

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm) && !_skipRequested)
        {
            
            _skipRequested = true;
        }

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            //InputBindingManager.Instance.BlockUntilRelease(InputActionType.Back);
            Debug.LogWarning("RUN SUMMARY BACK PRESSED");
            Close();
        }
    }


    private void CreateBreakdownRowAnimators()
    {
        for(int i = 0; i < breakdownRowDataList.Count; i++)
        {
            GameObject rowObj = Instantiate(statRowAnimatorPrefab, statRowContainer);
            StatRowAnimator rowAnimator = rowObj.GetComponentInChildren<StatRowAnimator>();   

            _rowAnimators.Add(rowAnimator);
            _rowObjs.Add(rowObj);
            _prefixTexts.Add(breakdownRowDataList[i].prefixLabel);
        }
    }

    private IEnumerator PlayBreakdownIntros()
    {
        //ScoreManager.Instance.RunStatsTracker.PrintStats();

        for (int i = 0; i < _rowAnimators.Count; i++)
        {
            if (_skipRequested)
                break;

            yield return StartCoroutine(PlaySingleRow(i));
        }
        
        if (_skipRequested)
        {
            for (int i = 0; i < _rowAnimators.Count; i++)
                SnapRowToFinal(i);

            yield break;
        }
    }

    private IEnumerator PlaySingleRow(int i)
    {
        if (_skipRequested) yield break;

    
        var rowAnimator = _rowAnimators[i];
        StatRowData rowData = breakdownRowDataList[i];
        var rowObj = _rowObjs[i];
        string prefix = _prefixTexts[i];

        int spaces = Mathf.Max(0, rowData.nameStatSpacing - prefix.Length);
        prefix += $"<space={spaces}px>";

        _prefixTexts[i] = prefix;
        
        
        if(rowAnimator == null)
        {
            Debug.LogError("When playing stat breakdown animator, the row animator was found to be null");
            yield break;
        }

        if(rowObj == null)
        {
            Debug.LogError("When playing stat breakdown animator, the row gameobject was found to be null");
            yield break;
        }

        TextMeshProUGUI displayText = rowAnimator.GetComponentInChildren<TextMeshProUGUI>();

        if(displayText == null)
        {
            Debug.LogError("When playing stat breakdown animator, the displayText was found to be null");
            yield break;
        }


        rowAnimator.PlayIntro();

        StatDisplayHelpers.SetupStatRow(displayText, rowData, rowObj);

        if (_skipRequested) 
            yield break;

        StatValue stat = StatDisplayHelpers.ResolveRunRecordStatValue(rowData, _runRecord);

        float pitchMult = 1.0f + (i * animateStatPitchStep);
        AudioHelpers.PlaySoundEffect(rowIntroSoundEffect, Camera.main.transform.position, pitchMult);


        Debug.Log("Resolved stat successfully.");

        yield return StartCoroutine(
            animator.AnimateStatText(
                displayText,
                stat,
                prefix,
                "",
                () => _skipRequested
            )
        );

        Debug.Log("Finished AnimateStatText.");

        if (_skipRequested) yield break;

  
        yield return StartCoroutine(WaitOrSkip(individualDisplayDelay));
    }

    private void SnapRowToFinal(int i)
    {
        var rowAnimator = _rowAnimators[i];
        var rowData = breakdownRowDataList[i];
        var prefix = _prefixTexts[i];
        var rowObj = _rowObjs[i];

        int spaces = Mathf.Max(0, rowData.nameStatSpacing - prefix.Length);
        prefix += new string(' ', spaces);

        TextMeshProUGUI displayText = rowAnimator.GetComponentInChildren<TextMeshProUGUI>();

        rowAnimator.SnapToFinal();

        StatDisplayHelpers.SetupStatRow(displayText, rowData, rowObj);

        var stat = StatDisplayHelpers.ResolveRunRecordStatValue(rowData, _runRecord);

        displayText.text = StatDisplayHelpers.FormatStatValue(stat, prefix);
    }

    private IEnumerator PlayBreakdownRowOutroAnimations()
    {
        for(int i = 0; i < _rowAnimators.Count; i++)
        {
            _rowAnimators[i].PlayOutro();
            AudioHelpers.PlaySoundEffect(rowOutroSoundEffect, Camera.main.transform.position);
            yield return new WaitForSeconds(rowOutroDelay);
        }
    }

    private IEnumerator WaitOrSkip(float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (_skipRequested)
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

        foreach (var ps in confettiSystems)
                if (ps != null) ps.Play();

        yield return StartCoroutine(PlayBreakdownIntros());

        OnStatsTallyComplete?.Invoke();
    }

    public IEnumerator PlayOutroAnimations()
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        OnContinuePressed?.Invoke();


        yield return StartCoroutine(PlayBreakdownRowOutroAnimations());

        foreach (var ps in confettiSystems)
            if (ps != null) ps.Stop();

        mainContainer?.SetActive(false);

        yield return new WaitForSeconds(0.5f);
    }

    void Cleanup()
    {
        _rowAnimators.Clear();
        _scoreSources.Clear();
        _prefixTexts.Clear();

        foreach (GameObject rowObj in _rowObjs)
        {
            Destroy(rowObj);
        }

        _rowObjs.Clear();
    }
}
