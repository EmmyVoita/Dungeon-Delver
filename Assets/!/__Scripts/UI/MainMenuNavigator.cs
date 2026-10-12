using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuNavigator : BaseMenu
{
    public static Action<int> OnSelectionChanged;

    [Header("References")]
    [SerializeField] private StartOptionsNavigator startOptions;

    [Header("MenuOptions")]
    [SerializeField] private List<MenuState> menuOptionTransitions = new();

    [Header("UI Options")]
    [SerializeField] private TextMeshProUGUI[] options;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;

    [Header("Menu Transition")]
    [SerializeField] private Material transitionMaterial;
    [SerializeField] private string transitionPropertyName = "_TransitionValue";
    [SerializeField] private float targetValue = 0;
    [SerializeField] private float transitionStartValue = 1;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float inputDelay = 0.1f;


    [Header("Dynamic")]
    [SerializeField] private int _selectedIndex = 0;



    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            OnSelectionChanged?.Invoke(_selectedIndex);
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (options == null || menuOptionTransitions == null)
            return;

        int optionCount = options.Length;
        int transitionCount = menuOptionTransitions.Count;

        // If transitions list is too small → expand it
        if (transitionCount < optionCount)
        {
            int toAdd = optionCount - transitionCount;
            for (int i = 0; i < toAdd; i++)
            {
                menuOptionTransitions.Add(MenuState.None);
            }

            Debug.LogWarning($"{name}: Expanded menuOptionTransitions to match options length.");
        }
        // If transitions list is too big → trim it
        else if (transitionCount > optionCount)
        {
            menuOptionTransitions.RemoveRange(optionCount, transitionCount - optionCount);

            Debug.LogWarning($"{name}: Trimmed menuOptionTransitions to match options length.");
        }
    }
    #endif

    private void OnEnable()
    {
        MainMenuOption.OnMainMenuOptionClicked += HandleOptionClicked;
        StartOptionsNavigator.OnStartSelected += HandleStartSelected;
        InputModeManager.OnInputModeChanged += HandleInputModeChanged;
    }

    private void OnDisable()
    {
        MainMenuOption.OnMainMenuOptionClicked -= HandleOptionClicked;
        StartOptionsNavigator.OnStartSelected -= HandleStartSelected;
        InputModeManager.OnInputModeChanged -= HandleInputModeChanged;
    }

    private void HandleInputModeChanged(InputModeManager.InputMode inputMode)
    {
        ResetVisuals();

        if(inputMode == InputModeManager.InputMode.Keyboard)
        {
            UpdateVisuals();
        }
    }

    private void HandleOptionClicked(int index)
    {
        SelectedIndex = index;
        ActivateOption();
    }

    private void Start()
    {
        SelectedIndex = 0;

        InputFocusManager.Clear();
        
        switch (SceneEntryContext.EntryType)
        {
            case SceneEntryType.ReturnFromTutorial:
                StartCoroutine(ReturnFromTutorialSequence());
                break;
            case SceneEntryType.ReturnFromPractice:
                MenuManager.Instance.OpenMenu(MenuState.Practice);
                break;
            default:
                EnableInput();
                break;
        }

        SceneEntryContext.Clear();
    }

    private IEnumerator ReturnFromTutorialSequence()
    {
        yield return new WaitForSeconds(0.5f);

        // Make sure we're actually starting at the covered state.
        transitionMaterial.SetFloat(transitionPropertyName,transitionStartValue);

        Tween tween = transitionMaterial
            .DOFloat(targetValue,transitionPropertyName,duration)
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        yield return tween.WaitForCompletion();

        yield return new WaitForSeconds(inputDelay);

        OnOpen();
    }

    private void EnableInput()
    {
        
    }


    private void Update()
    {
        if(!CanProcessInput)
            return;

        if(InputFocusManager.CurrentOwner != null)
            return;

        HandleInput();

        if (InputModeManager.Instance.CurrentMode ==
            InputModeManager.InputMode.Keyboard)
        {
            AnimateSelection();
        }
    }



    public override void OnOpen()
    {
        SelectedIndex = 0;

        UpdateVisuals();

        base.OnOpen();

      

        //Debug.LogError("MAIN MENU OPENING");
    }

    public override void OnClose()
    {
        options[SelectedIndex].transform.localScale = Vector3.one;
        SelectedIndex = -1;

        base.OnClose();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            SelectedIndex = (SelectedIndex + 1) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            SelectedIndex = (SelectedIndex - 1 + options.Length) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);
            UpdateVisuals();
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            ActivateOption();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].color = (i == SelectedIndex) ? selectedColor : defaultColor;
        }
    }

    void AnimateSelection()
    {
        for (int i = 0; i < options.Length; i++)
        {
            float targetScale = (i == SelectedIndex) ? selectedScale : 1f;
            options[i].transform.localScale = Vector3.Lerp(
                options[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    private void ResetVisuals()
    {
        for (int i = 0; i < options.Length; i++)
        {
            Transform optionTransform = options[i].transform;

            optionTransform.DOKill();

            options[i].color = defaultColor;
            optionTransform.localScale = Vector3.one;
        }
    }



    private MenuState GetTargetState(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= menuOptionTransitions.Count)
            return MenuState.None;

        return menuOptionTransitions[targetIndex];
    }

    private void HandleStartSelected()
    {
        if(isActive)
        {
            GameSceneLoader.PendingConfig = new GameSceneConfig(GameMode.Tutorial,0,null);
            SceneManager.LoadScene(SceneNames.TutorialScene);
        }
    }

    void ActivateOption()
    {
        MenuState targetState = GetTargetState(SelectedIndex);

        if(targetState != MenuState.Tutorial)
            isActive = false;

        switch (targetState)
        {
            case MenuState.None:
                break;
            case MenuState.Play:
                MenuManager.Instance.RequestMenuTransition(MenuState.Play);
                break;
            case MenuState.Tutorial:
                startOptions.Open(GameMode.Tutorial, SceneNames.TutorialScene);
                break;
            case MenuState.Settings:
                MenuManager.Instance.RequestMenuTransition(MenuState.Settings);
                break;
            case MenuState.LeaderBoard:
                MenuManager.Instance.RequestMenuTransition(MenuState.LeaderBoard);
                break;
            case MenuState.Practice:
                MenuManager.Instance.RequestMenuTransition(MenuState.Practice);
                break;
            case MenuState.Exit:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;

            default:
                Debug.Log("No action assigned to " + options[SelectedIndex].text.ToLower());
                break;
        }
    }
}
