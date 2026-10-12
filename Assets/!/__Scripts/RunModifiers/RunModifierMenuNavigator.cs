using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using UnityEditor;
using UnityEngine.InputSystem;

public class RunModifierMenuNavigator : BaseMenu
{
    public static RunModifierMenuNavigator Instance { get; private set; }


    [Header("References")]
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private RectTransform rightSideContainer;
    [SerializeField] private PlayPagesController playPagesController;
    [SerializeField] private ScoreMultiplierSlider scoreMultiplierSlider;
    [SerializeField] private Image currentRunModifierIcon;
    [SerializeField] private TextMeshProUGUI currentRunModifierScoreMod;
    [SerializeField] private TextMeshProUGUI lockedRatioText;
    [SerializeField] private string suffix = "Score";
    [SerializeField] private GameObject challengeDividerPrefab;
    [SerializeField] private GameObject assistsDividerPrefab;
    
    [Header("Modifier Summary")]
    [SerializeField] private Transform modifierSummaryContainer;
    [SerializeField] private GameObject modifierSummaryRowPrefab;
    [SerializeField] private GameObject truncateObject;
    [SerializeField] private TextMeshProUGUI truncateText;
    [SerializeField] private int maxSummaryRows = 6;

    [Header("Points UI")]
    [SerializeField] private int pointsCount = 5;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private RectTransform pointsContainer;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private string countTextsuffix;

    
    
    [Header("Transition Settings")]
    [SerializeField] private MenuState returnState;

    [Header("Key Prompts")]
    [SerializeField] private List<InputActionType> closeKeys;
    [SerializeField] private List<KeyPrompt> keyPrompts;
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private InputActionType continueKey = InputActionType.Confirm;

    
    [Header("Left Panel (Obstacle List)")]
    [SerializeField] private GameObject optionTextPrefab;
    [SerializeField] private Transform leftListContainer;


    [Header("Right Panel (Name/Description)")]
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    //[SerializeField] private Image backgroundImage;

    [Header("Menu Options (Assign in Inspector)")]
    [SerializeField] private int startingLeftOptionIndex = 0;


    [Header("Input Repeat")]
    [SerializeField] private float repeatDelay = 0.35f;
    [SerializeField] private float repeatRate = 0.08f;


    [Header("Scroll Debug")]
    [SerializeField] private float contentHeight;
    [SerializeField] private float contentAnchoredY;
    [SerializeField] private float viewportHeight;
    [SerializeField] private float desiredY;
    [SerializeField] private float maxY;
    [SerializeField] private float targetLocalY;
    [SerializeField] private float targetCenterY;
    [SerializeField] private float deltaY;


    [Header("Scroll Settings")]
    [SerializeField] private float scrollTweenDuration = 0.25f;


    [Header("Scrolling")]
    [SerializeField] private ScrollRect leftScrollRect;
    [SerializeField] private float bottomScrollPadding = 40f;
    [SerializeField] private float mouseScrollCooldown= 0.03f;

    [Header("Menu Transition")]
    [SerializeField] private ScreenSpaceTransitionData gameSceneTransitionData;


     private float _nextMouseScrollTime; 
    private Vector2 _lastHeldDirection;
    private float _nextRepeatTime;
    private bool _waitingForRepeat;
    private int _selectedIndex = 0;
    private int _pointsRemaining = 0;
    private List<RunModifierPointUI> _points = new();
    
    private RunModifierDefinition _currentRunModifier;
    //private PanelSide _currentPanel = PanelSide.Left;
    private Tween _scrollTween;
    private List<RunModifierOptionUI> _runModifiers; 

    private RunModifierMenuOptionBase _currentOption;
      private Dictionary<RunModifierDefinition, RunModifierSummaryRowUI> _summaryRows = new();


    public RunModifierMenuOptionBase CurrentOption => _currentOption;
    public RunModifierDefinition CurrentObstacle => _currentRunModifier;
    public int PointsRemaining => _pointsRemaining;
    public UIPanelNudge PanelNudge => panelNudge;

    private RectTransform Content => leftScrollRect.content;
    private RectTransform Viewport => leftScrollRect.viewport;
    


    private void OnEnable()
    {
        RunModifierManager.OnValueChanged += HandleValueChanged;
        RunModifierMenuOptionBase.OnNavigateToOption += HandleNavigationRequest;
        RunModifierOptionUI.OnClicked += HandleUIClicked;
    }

    private void OnDisable()
    {
        RunModifierManager.OnValueChanged -= HandleValueChanged;
        RunModifierMenuOptionBase.OnNavigateToOption -= HandleNavigationRequest;
        RunModifierOptionUI.OnClicked -= HandleUIClicked;
    }

    public void RebuildMenuOptions()
    {
        if (_currentOption != null)
        {
            _currentOption.OnExit();
            _currentOption = null;
        }

        BuildMenuOptions();

        _selectedIndex = 0;

        ResetScrollToTop();

        if (_runModifiers.Count > 0)
        {
            SwitchTo(
                _runModifiers[0],
                updateDescription: true,
                silent: true,
                scrollToOption: false
            );
        }
    }

    private void ResetScrollToTop()
    {
        _scrollTween?.Kill();
        Content.DOKill();

        leftScrollRect.StopMovement();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        Canvas.ForceUpdateCanvases();

        leftScrollRect.verticalNormalizedPosition = 1f;
    }

    private void HandleUIClicked(RunModifierOptionUI optionUI)
    {
        SwitchTo(optionUI, updateDescription: true);
    }

    private void HandleValueChanged(RunModifierSelection selection)
    {
        //scoreMultiplierSlider.SetMultiplier(RunModifierManager.Instance.GetScoreMultiplier());

        _pointsRemaining = pointsCount - RunModifierManager.Instance.GetSpentAssistPoints();

        for(int i = _points.Count-1; i >= 0; i--)
        {
            _points[i].SetFilled(i < _pointsRemaining);
        }

        countText.text = $"{_pointsRemaining} {countTextsuffix}";

        RefreshModifierSummary(selection);
    }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _runModifiers = new();

        BuildMenuOptions();

        BuildPointsUI();

        Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, 0f);

        _pointsRemaining = pointsCount;

        truncateObject.SetActive(false);

        SwitchTo(
            _runModifiers[0],
            true,
            true,
            false
        );

        base.Awake();
    }

    private void Update()
    {
        if (!CanProcessInput)
            return;

        if(HandleStartInput())
            return;

        foreach(InputActionType inputAction in closeKeys)
        {
            if(InputBindingManager.Instance.GetKeyDown(inputAction))
            {
                InputBindingManager.Instance.BlockUntilRelease(inputAction);
                OnClose();
                return;
            }
        }

        HandleDirectionalInput();
        HandleMouseScroll();
    }

    private void HandleDirectionalInput()
    {
        Vector2 input = GetDirectionalInput();
        
        if (input != Vector2.zero)
        {
            if(_currentOption == null)
                 return;
                 
            _currentOption.HandleDirectionalInput(input);
        }
    }

    private void HandleMouseScroll()
    {
        if (Mouse.current == null)
            return;

        if (Time.unscaledTime < _nextMouseScrollTime)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (scroll > 0)
        {
            HandleNavigationRequest(Vector2.up);
        }
        else
        {
            HandleNavigationRequest(Vector2.down);
        }

        _nextMouseScrollTime = Time.unscaledTime + mouseScrollCooldown;
    }

    private bool HandleStartInput()
    {
        if(!InputBindingManager.Instance.GetKeyDown(continueKey))
            return false;


        if(_pointsRemaining != 0)
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, transform.position);

            pointsContainer.DOKill();

            pointsContainer.localScale = Vector3.one;

            pointsContainer.DOPunchScale(
                Vector3.one * 0.15f,
                0.35f,
                vibrato: 7,
                elasticity: 0.6f
            ).SetUpdate(true);

            return false;
        }

        GameSceneLoader.PendingConfig = new GameSceneConfig(GameMode.StandardRun,0,null);

        RunModifierManager.Instance.ApplyModifiers();

        
        SceneTransitionManager.Instance.LoadScene(SceneNames.ArrowGameScene, gameSceneTransitionData);
       

        return true;
    }

    private void OnDestroy()
    {
        _scrollTween.Kill();
    }

    
    public override void OnOpen()
    {
        InputFocusManager.Claim(this);

        if (_summaryRows.Count == 0)
            BuildModifierSummaryUI();

        keyPromptController.PushPrompts(keyPrompts);

        leftScrollRect.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        _scrollTween?.Kill();
        Content.DOKill();

        if (_runModifiers.Count > 0)
        {
            _selectedIndex = Mathf.Clamp(_selectedIndex,0,_runModifiers.Count - 1);

            SwitchTo(
                _runModifiers[_selectedIndex],
                true,
                false,
                false
            );
        }

        /*
        scoreMultiplierSlider.SetMultiplier(
            RunModifierManager.Instance.GetScoreMultiplier()
        );
        */

        base.OnOpen();
    }

    public override void OnClose()
    {
        if (_currentOption != null)
        {
            _currentOption.OnExit();
            _currentOption = null;
        }

        _currentRunModifier = null;

        // Kill DoTweens
        _scrollTween?.Kill();
        Content.DOKill();

        // Pop Prompts
        keyPromptController.PopPrompts();

        /*
        // Destroy the run modifier options
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);
        */

        // Play Close Audio
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back,Camera.main.transform.position);


        //leftScrollRect.transform.gameObject.SetActive(false);

        base.OnClose();

        playPagesController.ShowPage(PlayPagesController.PlayPage.AbilitySelect, ()=>
        {
            InputFocusManager.Release(this);
        });
    }


    private void ScrollToSelected(RunModifierMenuOptionBase option)
    {
        if (leftScrollRect == null || option == null)
            return;

        Canvas.ForceUpdateCanvases();

        
        RectTransform target   = option.GetComponent<RectTransform>();

        contentHeight  = Content.rect.height;
        viewportHeight = Viewport.rect.height;

        

        // If content doesn't overflow, don't bother scrolling
        if (contentHeight <= viewportHeight)
        {
            // Optionally snap back to zero so it's always nicely aligned
            Content.anchoredPosition = Vector2.zero;
            return;
        }

        float viewportCenterY = Viewport.rect.center.y;

        Vector2 targetLocal = Viewport.InverseTransformPoint(target.position);
        targetLocalY = targetLocal.y;

        deltaY = targetLocalY - viewportCenterY;

        contentAnchoredY = Content.anchoredPosition.y;
        desiredY = contentAnchoredY - deltaY;

        // Allow some padding so the bottom item fully enters the viewport
        maxY = Mathf.Max(0f, (contentHeight - viewportHeight) - bottomScrollPadding);
        desiredY = Mathf.Clamp(desiredY, 0f, maxY);


        if (_scrollTween != null && _scrollTween.IsActive())
            _scrollTween.Kill();

        _scrollTween = Content.DOAnchorPosY(desiredY, scrollTweenDuration).SetEase(Ease.OutCubic);
    }

    void BuildPointsUI()
    {
        _points.Clear();

        for(int i = 0; i < pointsCount; i++)
        {
            GameObject pointObject = Instantiate(pointPrefab, pointsContainer);

            RunModifierPointUI pointUI = pointObject.GetComponent<RunModifierPointUI>();

            if(pointUI == null)
            {
                Destroy(pointObject);
                continue;
            }

            pointUI.SetFilled(true);

            _points.Add(pointUI);
        }
        
    }


    void BuildMenuOptions()
    {
        // Clear old UI
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        _runModifiers.Clear();

        IReadOnlyList<RunModifierSelection> selections = RunModifierManager.Instance.Selections;

        var sortedList = selections.OrderBy(item => !item.definition.IsUnlocked).ToList(); 

        // Divider between the two sections
        Instantiate(assistsDividerPrefab, leftListContainer);

        

        // Build assists first
        foreach (RunModifierSelection selection in sortedList)
        {
            if (selection.definition.category != RunModifierCategory.Assist)
                continue;

            CreateModifierOption(selection);
        }



        // Divider between the two sections
        Instantiate(challengeDividerPrefab, leftListContainer);

        // Build challenges second
        foreach (RunModifierSelection selection in sortedList)
        {
            if (selection.definition.category != RunModifierCategory.Challenge)
                continue;

            CreateModifierOption(selection);
        }

        Canvas.ForceUpdateCanvases();
    }

    private void CreateModifierOption(RunModifierSelection selection)
    {
        GameObject optionObj = Instantiate(optionTextPrefab, leftListContainer);

        RunModifierOptionUI option = optionObj.GetComponent<RunModifierOptionUI>();

        if (option != null)
        {
            option.Initialize(selection.definition);
            _runModifiers.Add(option);
        }
    }



    // --------------------------------------------------------------------
    // CORE NAVIGATION: Switch Between PracticeMenuOptions
    // --------------------------------------------------------------------
    private void SwitchTo(RunModifierMenuOptionBase newOption, 
                          bool updateDescription = false, 
                          bool silent = false, 
                          bool scrollToOption = true)
    {
        if (newOption == null)
            return;
        
        if(scrollToOption)
            ScrollToSelected(newOption);

        if (newOption is RunModifierOptionUI runModifier)
        {
            _currentRunModifier = runModifier.Definition;


            if(_currentRunModifier.IsUnlocked)
            {
                descriptionText.text = _currentRunModifier.GetDescription();
                currentRunModifierIcon.sprite = _currentRunModifier.icon;
                lockedRatioText.gameObject.SetActive(false);
                currentRunModifierScoreMod.gameObject.SetActive(_currentRunModifier.scoreMultiplierPerStep != 0f);
                string prefix = _currentRunModifier.category == RunModifierCategory.Assist ? "-" : "+";
                currentRunModifierScoreMod.text = $"{prefix}{_currentRunModifier.scoreMultiplierPerStep}{suffix}";
            }
            else
            {
                descriptionText.text = _currentRunModifier.UnlockRequirementText;
                currentRunModifierIcon.sprite = _currentRunModifier.icon;
                lockedRatioText.gameObject.SetActive(true);
                currentRunModifierScoreMod.gameObject.SetActive(false);
                lockedRatioText.text = _currentRunModifier.UnlockRatioText;
            }

            RefreshLayout();

            // Remember where we are.
            int index = _runModifiers.IndexOf(runModifier);

            if (index >= 0)
                _selectedIndex = index;
        }


        if(!silent)
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor,transform.position);
        
        if (_currentOption != null)
            _currentOption.OnExit();

        _currentOption = newOption;

        if(silent)
            return;

        _currentOption.OnEnter();
    }

    public void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rightSideContainer);
    }


    // --------------------------------------------------------------------
    // Handle a Navigation Request from a PracticeMenuOption
    // --------------------------------------------------------------------
    private void HandleNavigationRequest(Vector2 direction)
    {
        // LEFT PANEL NAVIGATION
        int index = _runModifiers.IndexOf(_currentOption as RunModifierOptionUI);
        if (index == -1) return;

        if (direction == Vector2.down)
        {
            // Already at bottom
            if (index == _runModifiers.Count - 1)
                return;

            SwitchTo(_runModifiers[index + 1], true);

            panelNudge.NudgeDown();
        }
        else if (direction == Vector2.up)
        {
            // Already at top
            if (index == 0)
                return;

            SwitchTo(_runModifiers[index - 1], true);

            panelNudge.NudgeUp();
        }
    }


    // --------------------------------------------------------------------
    // Convert raw keystrokes into directional input
    // --------------------------------------------------------------------
    private Vector2 GetDirectionalInput()
    {
        Vector2 currentInput = Vector2.zero;

        if (InputBindingManager.Instance.GetKeyHeld(InputActionType.MoveUp))
            currentInput = Vector2.up;
        else if (InputBindingManager.Instance.GetKeyHeld(InputActionType.MoveDown))
            currentInput = Vector2.down;
        else if (InputBindingManager.Instance.GetKeyHeld(InputActionType.MoveLeft))
            currentInput = Vector2.left;
        else if (InputBindingManager.Instance.GetKeyHeld(InputActionType.MoveRight))
            currentInput = Vector2.right;

        // No input held
        if (currentInput == Vector2.zero)
        {
            _waitingForRepeat = false;
            _lastHeldDirection = Vector2.zero;
            return Vector2.zero;
        }

        // Fresh press
        if (currentInput != _lastHeldDirection)
        {
            _lastHeldDirection = currentInput;
            _waitingForRepeat = true;
            _nextRepeatTime = Time.unscaledTime + repeatDelay;

            return currentInput;
        }

        // Held repeat
        if (_waitingForRepeat && Time.unscaledTime >= _nextRepeatTime)
        {
            _nextRepeatTime = Time.unscaledTime + repeatRate;
            return currentInput;
        }

        return Vector2.zero;
    }

    private void BuildModifierSummaryUI()
    {
        foreach (RunModifierSelection selection in RunModifierManager.Instance.Selections)
        {
            if (selection.definition == null)
                continue;

            RunModifierType type = selection.definition.type;

            string finalValue = RunModifierManager.Instance.GetFinalValue(type);

            GameObject rowObject =
                Instantiate(modifierSummaryRowPrefab, modifierSummaryContainer);

            RunModifierSummaryRowUI rowUI =
                rowObject.GetComponent<RunModifierSummaryRowUI>();

            if (rowUI == null)
            {
                Destroy(rowObject);
                continue;
            }

            rowUI.Setup(selection.definition, finalValue);

            _summaryRows[selection.definition] = rowUI;

            rowObject.SetActive(false);
        }

        RefreshModifierSummaryVisibility();
    }

    private void RefreshModifierSummary(RunModifierSelection changedSelection)
    {
        // Update the value for the modifier that changed.
        if (changedSelection?.definition != null &&
            _summaryRows.TryGetValue(changedSelection.definition, out RunModifierSummaryRowUI changedRow))
        {
            RunModifierType type = changedSelection.definition.type;

            string finalValue =
                RunModifierManager.Instance.GetFinalValue(type);

            changedRow.SetValue(finalValue);
        }

        RefreshModifierSummaryVisibility();
    }

    private void RefreshModifierSummaryVisibility()
    {
        IReadOnlyList<RunModifierSelection> selections =
            RunModifierManager.Instance.Selections;

        // All currently active modifiers, in the same order as the manager.
        List<RunModifierSelection> activeSelections = selections
            .Where(selection =>
                selection.definition != null &&
                selection.value != 0)
            .ToList();

        int visibleCount = Mathf.Min(
            activeSelections.Count,
            maxSummaryRows
        );

        // Hide every row first.
        foreach (RunModifierSummaryRowUI row in _summaryRows.Values)
        {
            row.gameObject.SetActive(false);
        }

        // Show only the rows that fit.
        for (int i = 0; i < visibleCount; i++)
        {
            RunModifierSelection selection = activeSelections[i];

            if (!_summaryRows.TryGetValue(
                    selection.definition,
                    out RunModifierSummaryRowUI row))
            {
                continue;
            }

            RunModifierType type = selection.definition.type;

            string finalValue =
                RunModifierManager.Instance.GetFinalValue(type);

            row.SetValue(finalValue);
            row.gameObject.SetActive(true);
        }

        truncateObject.transform.SetAsLastSibling();

        int hiddenCount =
            Mathf.Max(0, activeSelections.Count - maxSummaryRows);

        bool shouldTruncate = hiddenCount > 0;

        truncateObject.SetActive(shouldTruncate);

        if (shouldTruncate)
        {
            truncateText.text = $"+{hiddenCount} MORE";
        }
    }
}
