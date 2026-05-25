using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

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
    }

    private void OnDisable()
    {
        MainMenuOption.OnMainMenuOptionClicked -= HandleOptionClicked;
    }

    private void HandleOptionClicked(int index)
    {
        SelectedIndex = index;
        ActivateOption();
    }

    private void Start()
    {
        SelectedIndex = 0;
    }

    private void Update()
    {
        if (lockInput || !isActive) return;

        HandleInput();
        AnimateSelection();
    }



    public override void OnOpen()
    {
        base.OnOpen();

        lockInput = false;
        SelectedIndex = 0;

        if (options == null || options.Length == 0)
        {
            Debug.LogError("MainMenuNavigator: 'options' is empty or not assigned.");
            return;
        }

        UpdateVisuals();
    }

    public override void OnClose()
    {

        SelectedIndex = -1;
        base.OnClose();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            SelectedIndex = (SelectedIndex + 1) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            _selectedIndex = (_selectedIndex - 1 + options.Length) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
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



    private MenuState GetTargetState(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= menuOptionTransitions.Count)
            return MenuState.None;

        return menuOptionTransitions[targetIndex];
    }

    void ActivateOption()
    {
        MenuState targetState = GetTargetState(SelectedIndex);

        lockInput = true;

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
