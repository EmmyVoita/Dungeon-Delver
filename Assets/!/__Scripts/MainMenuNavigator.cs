using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Unity.VisualScripting;

public class MainMenuNavigator : BaseMenu
{

    public static Action OnReturnToMenu;
    public static Action OnPlaySelected;
    public static Action<string> OnTutorialSelected;
    public static Action OnSettingsSelected;
    public static Action OnExitSelected;

    

    [Header("UI Options")]
    public TextMeshProUGUI[] options;
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
    }

    // -------------------------------------------------------
    // MENU OPEN LOGIC
    // -------------------------------------------------------

    public override void OnOpen()
    {
        base.OnOpen();

        selectedIndex = 0;

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

        UpdateVisuals();
    }

    public override void OnClose()
    {
        foreach (var option in options)
            option.gameObject.SetActive(false);
            
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
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
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
                OnTutorialSelected?.Invoke(SceneNames.TutorialScene);
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
    }
}
