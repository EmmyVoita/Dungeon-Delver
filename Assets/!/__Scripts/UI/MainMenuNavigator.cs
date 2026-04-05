using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using DG.Tweening;

public class MainMenuNavigator : BaseMenu
{

    public static Action OnReturnToMenu;
    public static Action OnPlaySelected;
    public static Action<SelectOption> OnTutorialSelected;
    public static Action OnSettingsSelected;
    public static Action OnExitSelected;

    public static Action<int> OnSelectionChanged;

    

    [Header("Arrow Indicator")]
    public RectTransform arrowIndicator;
    public float arrowFollowSpeed = 12f;
    public float arrowLag = 0.08f;
    public float arrowXOffset = -40f;

    private Vector3 arrowVelocity;

    [Header("UI Options")]
    public TextMeshProUGUI[] options;
    public List<RectTransform> otherDepedecies;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public float selectedScale = 1.2f;
    public float transitionSpeed = 8f;

    [SerializeField] private int selectedIndex = 0;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    void Awake()
    {
        MenuManager.Instance.RegisterMenu(this);
        MenuManager.Instance.OpenMenu(StartMenuWindows.MainMenu);
    }

    void Start()
    {
        if (SceneReturnHandler.ReturnToAbilitySelect)
        {
           MenuManager.Instance.TransitionToMenu(StartMenuWindows.PlayMenu, 0.2f);
        }
        selectedIndex = 0;
        OnSelectionChanged?.Invoke(selectedIndex);
    }

    void UpdateArrowIndicator()
    {
        if (arrowIndicator == null || options.Length == 0)
            return;

        RectTransform target = options[selectedIndex].rectTransform;

        Vector3 targetPos = new Vector3(arrowIndicator.position.x,target.position.y,arrowIndicator.position.z);
        //targetPos.x += arrowXOffset;

        arrowIndicator.position = Vector3.SmoothDamp(
            arrowIndicator.position,
            targetPos,
            ref arrowVelocity,
            arrowLag,
            arrowFollowSpeed
        );
    }

    // -------------------------------------------------------
    // MENU OPEN LOGIC
    // -------------------------------------------------------

    public override void OnOpen()
    {
        base.OnOpen();

        selectedIndex = 0;
        OnSelectionChanged?.Invoke(selectedIndex);

        if (options == null || options.Length == 0)
        {
            Debug.LogError("MainMenuNavigator: 'options' is empty or not assigned.");
            return;
        }

        foreach (var opt in options)
        {
            if (opt == null)
            {
                Debug.LogError("MainMenuNavigator: Null option found in array.");
                continue;
            }
            opt.gameObject.SetActive(true);
        }

        foreach (var dep in otherDepedecies)
        {
            dep.gameObject?.SetActive(true);
        }

        UpdateVisuals();
        arrowIndicator.DOPunchScale(Vector3.one * 0.15f, 0.15f, 5, 0.6f);
        //ScreenDimmerManager.Instance.AddDimSource(gameObject.name);
    }

    public override void OnClose()
    {
        foreach (var option in options)
            option.gameObject.SetActive(false);

        foreach (var dep in otherDepedecies)
        {
            dep.gameObject?.SetActive(false);
        }

        //ScreenDimmerManager.Instance.RemoveDimSource(gameObject.name);
            
        base.OnClose();
    }

    // -------------------------------------------------------
    // INPUT + ANIMATION
    // -------------------------------------------------------

    void Update()
    {
        if (lockInput) return;

        HandleInput();
        AnimateSelection();
        UpdateArrowIndicator();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
            OnSelectionChanged?.Invoke(selectedIndex);
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
            OnSelectionChanged?.Invoke(selectedIndex);
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioSettingsManager.PlaySelectSound();
            ActivateOption();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].color = (i == selectedIndex) ? selectedColor : defaultColor;
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
                Time.deltaTime * transitionSpeed
            );
        }
    }

    // -------------------------------------------------------
    // SELECTION
    // -------------------------------------------------------


    void ActivateOption()
    {
        string optionName = options[selectedIndex].text.ToLower();

        switch (selectedIndex)
        {
            case 0:
                MenuManager.Instance.TransitionToMenu(StartMenuWindows.PlayMenu, 0.2f);
                break;
            case 1:
                OnTutorialSelected?.Invoke(SelectOption.Tutorial);
                break;
            case 2:
                MenuManager.Instance.TransitionToMenu(StartMenuWindows.ObstacleLabMenu, 0.2f);
                break;
            case 3:
                MenuManager.Instance.TransitionToMenu(StartMenuWindows.SettingsMenu, 0.2f);
                break;
            case 4:
                OnExitSelected?.Invoke();
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;

            default:
                Debug.Log("No action assigned to " + optionName);
                break;
        }

        selectedIndex = -1;
        OnSelectionChanged?.Invoke(selectedIndex);
    }
}
