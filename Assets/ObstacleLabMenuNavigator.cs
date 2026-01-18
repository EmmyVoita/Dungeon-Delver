using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class ObstacleLabMenuNavigator : BaseMenu
{
    public static ObstacleLabMenuNavigator Instance { get; private set; }
    private enum PanelSide
    {
        Left,
        Right
    }

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    [Header("Left Panel (Obstacle List)")]
    public List<ObstacleDefinition> obstacleList;
    public GameObject optionTextPrefab;
    public Transform leftListContainer;


    [Header("Right Panel (Obstacle List)")]
    public Transform rightListContainer;
    public Transform seperatorLine;
    public List<PracticeMenuOption> rightOptions; 
    

    [Header("Menu Options (Assign in Inspector)")]
    public List<PracticeMenuOption> leftOptions; 
    public int startingLeftOptionIndex = 0;
    public int startingRightOptionIndex = 0;
    private PanelSide currentPanel = PanelSide.Left;


    private PracticeMenuOption currentOption;
    private ObstacleDefinition currentObstacle;
    public PracticeMenuOption CurrentOption => currentOption;
    public ObstacleDefinition CurrentObstacle => currentObstacle;


    [Header("Scrolling")]
    public ScrollRect leftScrollRect;
[SerializeField] private float bottomScrollPadding = 40f;

    public RectTransform listContentTransform;



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

    private Tween scrollTween;



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


        if (scrollTween != null && scrollTween.IsActive())
            scrollTween.Kill();

        scrollTween = content.DOAnchorPosY(desiredY, scrollTweenDuration)
                            .SetEase(Ease.OutCubic);
    }






    private Vector2 ClampContentPosition(Vector2 pos, RectTransform content, RectTransform viewport)
    {
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // Content smaller than viewport → no movement needed
        if (contentHeight <= viewportHeight)
        {
            pos.y = 0f;
            return pos;
        }

        // clamp Y so content cannot scroll beyond its bounds
        float minY = 0f;
        float maxY = contentHeight - viewportHeight;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }







    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        MenuManager.Instance.RegisterMenu(this);

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(false);

        seperatorLine.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PracticeMenuOption.OnNavigateToOption += HandleNavigationRequest;
    }

    private void OnDisable()
    {
        PracticeMenuOption.OnNavigateToOption -= HandleNavigationRequest;
    }

    public override void OnOpen()
    {
        base.OnOpen();

        BuildMenuOptions();

        // Activate starting menu option
        if (leftOptions.Count > 0)
        {
            SwitchTo(leftOptions[startingLeftOptionIndex], true);
        }

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(true);

        seperatorLine.gameObject.SetActive(true);

        leftScrollRect.transform.gameObject.SetActive(true);
    }

    public override void OnClose()
    {
        if (currentOption != null)
            currentOption.OnExit();

        base.OnClose();
        
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        foreach (Transform child in rightListContainer)
            child.gameObject.SetActive(false);

        seperatorLine.gameObject.SetActive(false);

        leftScrollRect.transform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (lockInput || currentOption == null) 
            return;

        // GLOBAL ESCAPE — always returns to left panel
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            if (currentPanel == PanelSide.Right)
            {
                AudioSettingsManager.PlayBackSound();
                SwitchTo(leftOptions[startingLeftOptionIndex]);
                currentPanel = PanelSide.Left;
                return;
            }
            else if (currentPanel == PanelSide.Left)
            {
                AudioSettingsManager.PlayBackSound();
                MenuManager.Instance.TransitionToMenu(StartMenuWindows.MainMenu, 0.2f);
                return;
            }
        }

        // GLOBAL ESCAPE — always returns to left panel
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            if (currentPanel == PanelSide.Left)
            {
                AudioSettingsManager.PlaySelectSound();
                currentPanel = PanelSide.Right;
                SwitchTo(rightOptions[startingRightOptionIndex]);
                return;
            }
            else if (currentPanel == PanelSide.Right)
            {
                AudioSettingsManager.PlaySelectSound();
                ObstaclePracticeSession.SelectedObstacle = CurrentObstacle; 
                SceneManager.LoadScene(SceneNames.ObstaclePractice);
            }
        }

        Vector2 input = GetDirectionalInput();

        if (input != Vector2.zero)
        {
            if(currentOption == null)
            {
                Debug.LogWarning("No current menu option selected!");
                 return;
            }
               
            currentOption.HandleDirectionalInput(input);
        }

        // Confirm key
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            currentOption.OnConfirm();
        }
    }

    void BuildMenuOptions()
    {
        // 1. Clear old UI
        foreach (Transform child in leftListContainer)
            Destroy(child.gameObject);

        leftOptions.Clear();

        // 3. Instantiate new left-side entries
        foreach (ObstacleDefinition obstacle in obstacleList)
        {
            GameObject optionObj = Instantiate(optionTextPrefab, leftListContainer);

            // Set label
            TextMeshProUGUI text = optionObj.GetComponent<TextMeshProUGUI>();
            text.text = obstacle.displayName;

            // Convert into a PracticeMenuOption
            var menuOption = optionObj.AddComponent<ObstacleListMenuOption>();
            menuOption.Setup(obstacle);  // <-- Store obstacle reference

            // Register in navigation list
            leftOptions.Add(menuOption); // add at beginning to keep left side first
        }
    }



    // --------------------------------------------------------------------
    // CORE NAVIGATION: Switch Between PracticeMenuOptions
    // --------------------------------------------------------------------
    private void SwitchTo(PracticeMenuOption newOption, bool updateDescription = false)
    {
        if (newOption == null) return;

        if(updateDescription && currentPanel == PanelSide.Left)
        {
            ScrollToSelected(newOption);
            int index = leftOptions.IndexOf(newOption);
            if (index != -1 && index < obstacleList.Count)
            {
                descriptionText.text = obstacleList[index].description;
                nameText.text = obstacleList[index].displayName;
                currentObstacle = obstacleList[index];
            }
        }

        AudioSettingsManager.PlayNavigateSound();

        if (currentOption != null)
            currentOption.OnExit();

        currentOption = newOption;
        currentOption.OnEnter();
    }


    // --------------------------------------------------------------------
    // Handle a Navigation Request from a PracticeMenuOption
    // --------------------------------------------------------------------
    private void HandleNavigationRequest(Vector2 direction)
    {
        // LEFT PANEL NAVIGATION
        if (currentPanel == PanelSide.Left)
        {
            int index = leftOptions.IndexOf(currentOption);
            if (index == -1) return;

            if (direction == Vector2.down)
                SwitchTo(leftOptions[Mathf.Min(leftOptions.Count - 1, index + 1)], true);
            else if (direction == Vector2.up)
                SwitchTo(leftOptions[Mathf.Max(0, index - 1)], true);
            else if (direction == Vector2.right && rightOptions.Count > 0)
            {
                currentPanel = PanelSide.Right;
                SwitchTo(rightOptions[startingRightOptionIndex]);
            }
        }
        // RIGHT PANEL NAVIGATION
        else if (currentPanel == PanelSide.Right)
        {
            int index = rightOptions.IndexOf(currentOption);
            if (index == -1) return;

            if (direction == Vector2.down)
                SwitchTo(rightOptions[Mathf.Min(rightOptions.Count - 1, index + 1)]);
            else if (direction == Vector2.up)
                SwitchTo(rightOptions[Mathf.Max(0, index - 1)]);
            else if (direction == Vector2.left && leftOptions.Count > 0)
            {
                currentPanel = PanelSide.Left;
                SwitchTo(leftOptions[startingLeftOptionIndex]);
            }
        }
    }


    // --------------------------------------------------------------------
    // Convert raw keystrokes into directional input
    // --------------------------------------------------------------------
    private Vector2 GetDirectionalInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
            return Vector2.up;
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
            return Vector2.down;
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
            return Vector2.left;
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
            return Vector2.right;

        return Vector2.zero;
    }
}
