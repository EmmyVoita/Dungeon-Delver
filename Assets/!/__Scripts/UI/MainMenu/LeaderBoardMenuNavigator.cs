using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class LeaderBoardMenuNavigator : BaseMenu
{
    [Header("References")]
    [SerializeField] private RectTransform parentTransform;
    [SerializeField] private GameObject listPrefab;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private FillRecordInventory inventory;
    [SerializeField] private InventoryOpenSettings settings;
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private RunSummaryDisplay runSummaryDisplay;
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private ResultsPageController resultsPageController;
    


    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;

    [Header("Animation")]
    [SerializeField] private bool fadeCanvasGroup = true;
    [SerializeField] private float selectedScale = 1.05f;
    [SerializeField] private float transitionSpeed = 1.0f;
    

    [Header("Audio")]
    [SerializeField] private SoundEffect addItemSound;
    [SerializeField] private float pitchStep = 0.025f;

    [Header("Display Settings")]
    [SerializeField] private int showAmount = 10;
    [SerializeField] private float delayAmount = 0.25f;
    [SerializeField] private float fadeInOutDuration = 0.25f;

    [Header("Timing Ease")]
    [SerializeField] private Ease delayEase = Ease.OutCubic;
    [SerializeField] private Vector2 delayScaling = new Vector2(0.1f,1.2f);

    [Header("Dynamic")]
    [SerializeField] private int _selectedIndex = 0;
    [SerializeField] List<RunRecordItem> _records;
    [SerializeField] List<RunRecord> _topRuns;

    private bool _listBuilt = false;

    private void OnEnable()
    {
        RunRecordItem.OnClicked += HandleRunRecordClicked;
    }

    private void OnDisable()
    {
        RunRecordItem.OnClicked -= HandleRunRecordClicked;
    }

    private void HandleRunRecordClicked(RunRecordItem item)
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        _selectedIndex = _records.IndexOf(item);
        UpdateVisuals();
    }

  
    private void Update()
    {
        if(!CanProcessInput)
            return;

        if(!isActive || !_listBuilt) return;
        
        if(!InputFocusManager.HasFocus(this))
            return;

        AnimateSelection();
        
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            isActive = false;
            Debug.LogWarning("LEADERBOARD BACK PRESSED");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
            MenuManager.Instance.RequestMenuTransition(MenuState.Main);
            return;
        }

        

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.ViewUpgrades))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            ActivateOption();
        }

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            runSummaryDisplay.Open(_topRuns[_selectedIndex]);
            resultsPageController.ShowSummary();
        }

        HandleNavigationInput();
    }

    private void HandleNavigationInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            _selectedIndex = (_selectedIndex + 1) % _records.Count;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            panelNudge.NudgeDown();
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            _selectedIndex = (_selectedIndex - 1 + _records.Count) % _records.Count;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            panelNudge.NudgeUp();
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < _records.Count; i++)
        {
            _records[i].SetHovered(i == _selectedIndex);
        }
    }

    
    private void ActivateOption()
    {
        inventory.SetRecord(_records[_selectedIndex].Record.upgrades);
        inventory.SetVisibilityData(resultsPageController,ResultsPageController.ResultsPage.Leaderboard);


        if(inventory.TryOpen(keyPromptController))
            resultsPageController.ShowInventory();
    }
    
    

    public override void OnOpen()
    {
        base.OnOpen();
        
        if(fadeCanvasGroup)
            canvasGroup.DOFade(1, fadeInOutDuration);

        InputFocusManager.Claim(this);

        keyPromptController.PushPrompts(keyPrompts);

        resultsPageController.ShowLeaderboard();

        StartCoroutine(BuildList(() =>
        {
            
        }));
    }

    public override void OnClose()
    {
        InputFocusManager.Release(this);
        
        keyPromptController.PopPrompts();

        _listBuilt = false;
        _records.Clear();

        if(fadeCanvasGroup)
            canvasGroup.DOFade(0, fadeInOutDuration);

        base.OnClose();
    }


    private IEnumerator BuildList(Action onComplete)
    {
        
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }

        SaveData saveData = ScoreManager.Instance.SaveData;
        _topRuns = GetTopRuns(saveData, showAmount);



        if (_topRuns.Count == 0)
        {
            Debug.Log("No runs to display");
            _listBuilt = true;
            _selectedIndex = 0;
            onComplete?.Invoke();
            yield break;
        }

        int count = _topRuns.Count;

        for (int i = 0; i < count; i++)
        {
            var record = _topRuns[i];

            GameObject obj = Instantiate(listPrefab, parentTransform);

            

            RunRecordItem item = obj.GetComponent<RunRecordItem>();
            item.Initialize(record, i + 1);

            _records.Add(item);

            item.SetHovered(false);

            // Normalized position (0 → 1)
            float t = count <= 1 ? 1f : (float)i / (count - 1);

            // DOTween ease evaluation
            float easedT = DOVirtual.EasedValue(0f, 1f, t, delayEase);

            // Delay scaling
            float delay = delayAmount * Mathf.Lerp(delayScaling.x, delayScaling.y, easedT);

            // Optional: curved pitch
            float pitchMult = Mathf.Lerp(1.0f, 1.0f + pitchStep * count, easedT);

            AudioHelpers.PlaySoundEffect(addItemSound, transform.position, pitchMult);

            yield return new WaitForSecondsRealtime(delay);
        }

        _listBuilt = true;
        _selectedIndex = 0;
        UpdateVisuals();

        onComplete?.Invoke();
    }

    void AnimateSelection()
    {
        for (int i = 0; i < _records.Count; i++)
        {
            float targetScale = (i == _selectedIndex) ? selectedScale : 1f;
                _records[i].transform.localScale = Vector3.Lerp(
                _records[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    public static List<RunRecord> GetTopRuns(SaveData saveData, int count = 10)
    {
        return saveData.records
            .OrderByDescending(r => r.score)
            .Take(count)
            .ToList();
    }

    public static List<RunRecord> GetTopRunsForAbility(AbilityType targetType, SaveData saveData, int count = 10)
    {
        return saveData.records
            .OrderByDescending(r => r.score)
            .Where(r => r.abilityUsed == targetType)
            .Take(count)
            .ToList();
    }
}