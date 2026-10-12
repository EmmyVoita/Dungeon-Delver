using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using UnityEngine.InputSystem;

public class PracticeMenuNavigator : BaseMenu
{
    public static PracticeMenuNavigator Instance { get; private set; }
    private enum PanelSide
    {
        Left,
        Right
    }

    [Header("References")]
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private RectTransform bossTogglePanel;
    [SerializeField] private BossModeSlider bossSlider;
    [SerializeField] private RectTransform rightSideContainer;
    [SerializeField] private GameObject startPracticeButton;
    
    [Header("Transition Settings")]
    [SerializeField] private MenuState returnState;

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;
    [SerializeField] private KeyPromptController keyPromptController;

    
    [Header("Left Panel (Obstacle List)")]
    [SerializeField] private ChallengeObjectDatabase database;
    [SerializeField] private GameObject optionTextPrefab;
    [SerializeField] private Transform leftListContainer;


    [Header("Right Panel (Name/Description)")]
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Transform rightListContainer;
    [SerializeField] private List<PracticeMenuOption> rightOptions; 
    

    [Header("Menu Options (Assign in Inspector)")]
    [SerializeField] private int startingLeftOptionIndex = 0;
    [SerializeField] private int startingRightOptionIndex = 0;


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

    [SerializeField] private float mouseScrollCooldown = 0.12f;

    private float _nextMouseScrollTime;


    private Vector2 _lastHeldDirection;
    private float _nextRepeatTime;
    private bool _waitingForRepeat;
    private PracticeObstacleOption _currentOption;
    private ObstacleTypeDefinition _currentObstacle;
    //private PanelSide _currentPanel = PanelSide.Left;
    private Tween _scrollTween;
    private bool _useBoss;
    private List<PracticeObstacleOption> _challenges; 


    public PracticeObstacleOption CurrentOption => _currentOption;
    public ObstacleTypeDefinition CurrentObstacle => _currentObstacle;


    private void OnEnable()
    {
        PracticeMenuOption.OnNavigateToOption += HandleNavigationRequest;
        PracticeMenuOption.OnPracticeOptionClick += HandlePracticeOptionEnter;
        //ObstacleListMenuOption.OnObstacleListOptionClick += HandleChallengeClickEvent;
    }

    private void OnDisable()
    {
        PracticeMenuOption.OnNavigateToOption -= HandleNavigationRequest;
        PracticeMenuOption.OnPracticeOptionClick -= HandlePracticeOptionEnter;
        //ObstacleListMenuOption.OnObstacleListOptionClick -= HandleChallengeClickEvent;
    }

    private void HandlePracticeOptionEnter(PracticeMenuOption option)
    {
        SwitchTo(option as PracticeObstacleOption);
    }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _challenges = new();

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(false);

        base.Awake();
    }

    private void SetBossMode(bool value)
    {
        _useBoss = value;
        bossSlider.SetValue(value);
    }

    private void Update()
    {
        if (!CanProcessInput)
            return;

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Jump))
        {
            if(_currentObstacle.hasBossOverride)
            {
                SetBossMode(!_useBoss);

                if(_useBoss)
                    panelNudge.NudgeRight();
                else
                    panelNudge.NudgeLeft();
            }
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            isActive = false;
            MenuManager.Instance.RequestMenuTransition(returnState);
            Debug.LogWarning("PRACTICE BACK PRESSED");
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
            return;
        }

        // GLOBAL ESCAPE — always returns to left panel
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {

            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            OnConfirm();
        }

        Vector2 input = GetDirectionalInput();

        if (input != Vector2.zero)
        {
            if(_currentOption == null)
            {
                Debug.LogWarning("No current menu option selected!");
                 return;
            }
               
            _currentOption.HandleDirectionalInput(input);
        }

        // Confirm key
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            _currentOption.OnConfirm();
        }

        HandleMouseScroll();
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

        _nextMouseScrollTime =
            Time.unscaledTime + mouseScrollCooldown;
    }

    private void OnDestroy()
    {
        _scrollTween.Kill();
    }

    
    public override void OnOpen()
    {
        backgroundImage.enabled = true;
        base.OnOpen();

        InputFocusManager.Claim(this);
        keyPromptController.PushPrompts(keyPrompts);

        BuildMenuOptions();

        // Make sure the ScrollRect is active before doing layout/scroll calculations
        leftScrollRect.gameObject.SetActive(true);

        // Force the newly-created list to finish laying itself out
        Canvas.ForceUpdateCanvases();

        // Reset previous scroll state
        _scrollTween?.Kill();
        leftScrollRect.content.anchoredPosition =
            new Vector2(leftScrollRect.content.anchoredPosition.x, 0f);

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(true);

        // Select starting option
        if (_challenges.Count > 0)
        {
            SwitchTo(_challenges[startingLeftOptionIndex], true);
        }
    }

    public override void OnClose()
    {
        backgroundImage.enabled = false;
        
        if (_currentOption != null)
        {
            _currentOption.OnExit();
            _currentOption = null;
        }

        _currentObstacle = null;
        
        InputFocusManager.Release(this);

        keyPromptController.PopPrompts();

        base.OnClose();
        
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(false);

        leftScrollRect.transform.gameObject.SetActive(false);
    }


    /*
    private void HandleChallengeClickEvent(PracticeObstacleOption option)
    {
        ScrollToSelected(option);

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        SwitchTo(option, true);
    }
    */

  

    private void ScrollToSelected(PracticeObstacleOption option)
    {
        if (leftScrollRect == null || option == null)
            return;

        Canvas.ForceUpdateCanvases();

        RectTransform content  = leftScrollRect.content;
        RectTransform viewport = leftScrollRect.viewport;
        RectTransform target   = option.GetComponent<RectTransform>();

        contentHeight  = content.rect.height;
        viewportHeight = viewport.rect.height;

        

        // If content doesn't overflow, don't bother scrolling
        if (contentHeight <= viewportHeight)
        {
            // Optionally snap back to zero so it's always nicely aligned
            content.anchoredPosition = Vector2.zero;
            return;
        }

        float viewportCenterY = viewport.rect.center.y;

        Vector2 targetLocal = viewport.InverseTransformPoint(target.position);
        targetLocalY = targetLocal.y;

        deltaY = targetLocalY - viewportCenterY;

        contentAnchoredY = content.anchoredPosition.y;
        desiredY = contentAnchoredY - deltaY;

        // Allow some padding so the bottom item fully enters the viewport
        maxY = Mathf.Max(0f, (contentHeight - viewportHeight) - bottomScrollPadding);
        desiredY = Mathf.Clamp(desiredY, 0f, maxY);


        if (_scrollTween != null && _scrollTween.IsActive())
            _scrollTween.Kill();

        _scrollTween = content.DOAnchorPosY(desiredY, scrollTweenDuration).SetEase(Ease.OutCubic);
    }

    
    public void OnConfirm()
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

        Debug.Log($"Tyring confirm with useBoss => {_useBoss}");

        GameMode targetGameMode = _useBoss ? GameMode.ObstaclePracticeBoss : GameMode.ObstaclePractice;

        GameSceneLoader.PendingConfig = new GameSceneConfig(
            targetGameMode,
            0,
            CurrentObstacle,
            MenuState.Practice);


        SceneManager.LoadScene(SceneNames.ArrowGameScene);
    }

    void BuildMenuOptions()
    {
        // Clear old UI
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        _challenges.Clear();

        // Sort by stage first, then alphabetically within each stage
        var sortedChallenges = database.challenges
            .OrderBy(obstacle => obstacle.stageNumber)
            .ThenBy(obstacle => obstacle.displayName)
            .ToList();

        foreach (ObstacleTypeDefinition obstacle in sortedChallenges)
        {
            GameObject optionObj = Instantiate(optionTextPrefab, leftListContainer);

            PracticeObstacleOption manager = optionObj.GetComponent<PracticeObstacleOption>();

            if(manager)
                manager.Initialize(obstacle);

            _challenges.Add(manager);
        }
    }



    // --------------------------------------------------------------------
    // CORE NAVIGATION: Switch Between PracticeMenuOptions
    // --------------------------------------------------------------------
    private void SwitchTo(PracticeObstacleOption newOption, bool updateDescription = false)
    {
        if (newOption == null)
            return;

        SetBossMode(false);

        ScrollToSelected(newOption);

        if (newOption is PracticeObstacleOption obstacleOption)
        {
            _currentObstacle = obstacleOption.Obstacle;

            bossTogglePanel.gameObject.SetActive(_currentObstacle.hasBossOverride);

            descriptionText.text = _currentObstacle.description;

            stageText.text = $"Stage {_currentObstacle.stageNumber}";

            RefreshLayout();
        }

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor,transform.position);

        if (_currentOption != null)
            _currentOption.OnExit();

        _currentOption = newOption;
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
        int index = _challenges.IndexOf(_currentOption);
        if (index == -1) return;

        if (direction == Vector2.down)
        {
            // Already at bottom
            if (index == _challenges.Count - 1)
                return;

            SwitchTo(_challenges[index + 1], true);

            panelNudge.NudgeDown();
        }
        else if (direction == Vector2.up)
        {
            // Already at top
            if (index == 0)
                return;

            SwitchTo(_challenges[index - 1], true);

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
}
