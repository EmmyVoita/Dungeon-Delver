using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class PracticeMenuNavigator : BaseMenu
{
    public static PracticeMenuNavigator Instance { get; private set; }
    private enum PanelSide
    {
        Left,
        Right
    }
    
    [Header("Transition Settings")]
    [SerializeField] private MenuState returnState;

    
    [Header("Left Panel (Obstacle List)")]
    [SerializeField] private ChallengeObjectDatabase database;
    [SerializeField] private GameObject optionTextPrefab;
    [SerializeField] private Transform leftListContainer;


    [Header("Right Panel (Name/Description)")]
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


    private Vector2 _lastHeldDirection;
    private float _nextRepeatTime;
    private bool _waitingForRepeat;
    private PracticeMenuOption _currentOption;
    private ObstacleTypeDefinition _currentObstacle;
    private PanelSide _currentPanel = PanelSide.Left;
    private Tween _scrollTween;
    private bool _useBoss;
    private List<PracticeMenuOption> _leftOptions; 


    public PracticeMenuOption CurrentOption => _currentOption;
    public ObstacleTypeDefinition CurrentObstacle => _currentObstacle;


    private void OnEnable()
    {
        PracticeMenuOption.OnNavigateToOption += HandleNavigationRequest;
        JumpDirectionModeMenuOption.MenuOptionIndexChanged += HandleMenuOptionChanged;
        ObstacleListMenuOption.OnObstacleListOptionClick += HandleChallengeClickEvent;
    }

    private void OnDisable()
    {
        PracticeMenuOption.OnNavigateToOption -= HandleNavigationRequest;
        JumpDirectionModeMenuOption.MenuOptionIndexChanged -= HandleMenuOptionChanged;
        ObstacleListMenuOption.OnObstacleListOptionClick -= HandleChallengeClickEvent;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _leftOptions = new();

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (lockInput || _currentOption == null || !isActive) 
            return;

        // GLOBAL ESCAPE — always returns to left panel
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            if (_currentPanel == PanelSide.Right)
            {
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
                SwitchTo(_leftOptions[startingLeftOptionIndex]);
                _currentPanel = PanelSide.Left;
                return;
            }
            else if (_currentPanel == PanelSide.Left)
            {
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
                MenuManager.Instance.RequestMenuTransition(returnState);
                return;
            }
        }

        // GLOBAL ESCAPE — always returns to left panel
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            if (_currentPanel == PanelSide.Left)
            {
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
                _currentPanel = PanelSide.Right;
                SwitchTo(rightOptions[startingRightOptionIndex]);
                return;
            }
            else if (_currentPanel == PanelSide.Right)
            {
                OnBossPromptConfirm();
            }
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
    }

    
    public override void OnOpen()
    {
        backgroundImage.enabled = true;
        base.OnOpen();

        BuildMenuOptions();

        // Activate starting menu option
        if (_leftOptions.Count > 0)
        {
            SwitchTo(_leftOptions[startingLeftOptionIndex], true);
        }

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(true);

        leftScrollRect.transform.gameObject.SetActive(true);
    }

    public override void OnClose()
    {
        backgroundImage.enabled = false;
        if (_currentOption != null)
            _currentOption.OnExit();
            

        base.OnClose();
        
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(false);

        leftScrollRect.transform.gameObject.SetActive(false);
    }



    private void HandleChallengeClickEvent(PracticeMenuOption option)
    {
        ScrollToSelected(option);

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        SwitchTo(option, true);
    }

    private void HandleMenuOptionChanged(int index, int count)
    {
        _useBoss = index == 1 ? true : false;
    }

    private void ScrollToSelected(PracticeMenuOption option)
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

        _scrollTween = content.DOAnchorPosY(desiredY, scrollTweenDuration)
                            .SetEase(Ease.OutCubic);
    }

    
    public void OnBossPromptConfirm()
    {
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

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
        // 1. Clear old UI
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        _leftOptions.Clear();

        // 3. Instantiate new left-side entries
        foreach (ObstacleTypeDefinition obstacle in database.challenges)
        {
            GameObject optionObj = Instantiate(optionTextPrefab, leftListContainer);

            // Set label
            TextMeshProUGUI text = optionObj.GetComponent<TextMeshProUGUI>();
            text.text = obstacle.displayName;

            // Convert into a PracticeMenuOption
            var menuOption = optionObj.AddComponent<ObstacleListMenuOption>();
            menuOption.Setup(obstacle);  // <-- Store obstacle reference

            // Register in navigation list
            _leftOptions.Add(menuOption); // add at beginning to keep left side first
        }
    }



    // --------------------------------------------------------------------
    // CORE NAVIGATION: Switch Between PracticeMenuOptions
    // --------------------------------------------------------------------
    private void SwitchTo(PracticeMenuOption newOption, bool updateDescription = false)
    {
        if (newOption == null) return;

        if(updateDescription && _currentPanel == PanelSide.Left)
        {
            ScrollToSelected(newOption);
            int index = _leftOptions.IndexOf(newOption);
            if (index != -1 && index < database.challenges.Count)
            {
                descriptionText.text = database.challenges[index].description;
                nameText.text = database.challenges[index].displayName.ToUpper();
                _currentObstacle = database.challenges[index];
            }
        }

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);

        if (_currentOption != null)
            _currentOption.OnExit();

        _currentOption = newOption;
        _currentOption.OnEnter();
    }


    // --------------------------------------------------------------------
    // Handle a Navigation Request from a PracticeMenuOption
    // --------------------------------------------------------------------
    private void HandleNavigationRequest(Vector2 direction)
    {
        // LEFT PANEL NAVIGATION
        if (_currentPanel == PanelSide.Left)
        {
            int index = _leftOptions.IndexOf(_currentOption);
            if (index == -1) return;

            if (direction == Vector2.down)
            {
                // Already at bottom
                if (index == _leftOptions.Count - 1)
                    return;

                SwitchTo(_leftOptions[index + 1], true);
            }
            else if (direction == Vector2.up)
            {
                // Already at top
                if (index == 0)
                    return;

                SwitchTo(_leftOptions[index - 1], true);
            }
            else if (direction == Vector2.right && rightOptions.Count > 0)
            {
                _currentPanel = PanelSide.Right;
                SwitchTo(rightOptions[startingRightOptionIndex]);
            }
        }

        // RIGHT PANEL NAVIGATION
        else if (_currentPanel == PanelSide.Right)
        {
            int index = rightOptions.IndexOf(_currentOption);
            if (index == -1) return;

            if (direction == Vector2.down)
            {
                // Already at bottom
                if (index == rightOptions.Count - 1)
                    return;

                SwitchTo(rightOptions[index + 1]);
            }
            else if (direction == Vector2.up)
            {
                // Already at top
                if (index == 0)
                    return;

                SwitchTo(rightOptions[index - 1]);
            }
            else if (direction == Vector2.left && _leftOptions.Count > 0)
            {
                _currentPanel = PanelSide.Left;
                SwitchTo(_leftOptions[startingLeftOptionIndex]);
            }
        }
    }


    // --------------------------------------------------------------------
    // Convert raw keystrokes into directional input
    // --------------------------------------------------------------------
    private Vector2 GetDirectionalInput()
    {
        Vector2 currentInput = Vector2.zero;

        if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveUp))
            currentInput = Vector2.up;
        else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveDown))
            currentInput = Vector2.down;
        else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveLeft))
            currentInput = Vector2.left;
        else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveRight))
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
