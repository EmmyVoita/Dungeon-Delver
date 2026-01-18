using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class SettingsMenuNavigator : BaseMenu
{
    public static Action SettingsMenuClosed;

    [Header("References")]
    [SerializeField] private GameObject holderObject;
    [SerializeField] private Transform currentPanel;
    [SerializeField] private SettingsTabManager tabManager;

    [Header("Visuals")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;

    private List<BaseSettingOption> currentOptions = new List<BaseSettingOption>();
    private int selectedIndex = -1;
    private bool enableInput = false;
    [SerializeField] private bool inTabNavigation = false;

    void Awake()
    {
        MenuManager.Instance.RegisterMenu(this);
        lockInput = true;
    }

    void Start()
    {
        holderObject.SetActive(false);
        LoadPanelOptions();
    }

    public override void OnOpen()
    {
        base.OnOpen();

        holderObject.SetActive(true);

        // Start on the tabs
        inTabNavigation = true;
        tabManager.SetTabFocus(true);

        HighlightAllOptionsDefault();
    }

    public override void OnClose()
    {
        base.OnClose();

        holderObject.SetActive(false);
        selectedIndex = 0;
    }

    void LoadPanelOptions()
    {
        currentOptions.Clear();
        currentOptions.AddRange(currentPanel.GetComponentsInChildren<BaseSettingOption>(true));
        selectedIndex = Mathf.Clamp(selectedIndex, 0, currentOptions.Count - 1);
    }

    public void ChangeActivePanel(Transform newPanel, bool fromTabSwitch = false)
    {
        currentPanel = newPanel;
        LoadPanelOptions();

        if (fromTabSwitch)
        {
            inTabNavigation = true;
            tabManager.SetTabFocus(true);
            HighlightAllOptionsDefault();
        }
        else
        {
            inTabNavigation = false;
        }
    }

    void Update()
    {
        if (lockInput) return;

        if (currentOptions.Count == 0)
            return;

        BaseSettingOption current = currentOptions[selectedIndex];

        // -----------------------------
        // Exit settings
        // -----------------------------
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back) && !current.IsNavigationLocked)
        {
            AudioSettingsManager.PlayBackSound();
            SettingsMenuClosed?.Invoke();
            MenuManager.Instance.TransitionToMenu(StartMenuWindows.MainMenu, 0.2f);
            return;
        }

        // ======================================================================
        // TAB NAVIGATION MODE
        // ======================================================================
        if (inTabNavigation)
        {
            tabManager.SetTabFocus(true);

            if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
                tabManager.MoveTab(-1);

            if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
                tabManager.MoveTab(1);

            if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
            {
                inTabNavigation = false;
                tabManager.SetTabFocus(false);
                AudioSettingsManager.PlayNavigateSound();
                HighlightOption(selectedIndex, instant: true);
            }

            return;
        }

        // ======================================================================
        // OPTION NAVIGATION MODE
        // ======================================================================
        tabManager.SetTabFocus(false);



        // ----------------------------------------------------------------------
        //  🔒 BLOCK ALL NAVIGATION IF LOCKED
        // ----------------------------------------------------------------------
        if (current.IsNavigationLocked)
        {
            // Still allow confirmation for in-progress interactions
            if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
                current.OnActivate();

            return; // block all movement
        }

        // ----------------------------------------------------------------------
        // Move selection UP
        // ----------------------------------------------------------------------
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            if (selectedIndex == 0)
            {
                // Move back to tab mode
                inTabNavigation = true;
                tabManager.SetTabFocus(true);
                AudioSettingsManager.PlayNavigateSound();
                HighlightAllOptionsDefault();
                return;
            }
            else
            {
                MoveSelection(-1);
            }
        }

        // ----------------------------------------------------------------------
        // Move selection DOWN
        // ----------------------------------------------------------------------
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
            MoveSelection(1);

        // ----------------------------------------------------------------------
        // Adjust current setting (left/right)
        // ----------------------------------------------------------------------
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
            current.AdjustValue(-1);

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
            current.AdjustValue(1);

        // ----------------------------------------------------------------------
        // Activate setting (Confirm)
        // ----------------------------------------------------------------------
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            current.OnActivate();

        AnimateSelection();
    }

    void MoveSelection(int direction)
    {
        selectedIndex = Mathf.Clamp(selectedIndex + direction, 0, currentOptions.Count - 1);
        AudioSettingsManager.PlayNavigateSound();
        HighlightOption(selectedIndex);
    }

    void HighlightOption(int index, bool instant = false)
    {
        for (int i = 0; i < currentOptions.Count; i++)
        {
            bool active = (i == index);

            var settingOption = currentOptions[i] as SettingOption;
            if (settingOption != null)
                settingOption.SetSelected(active);

            var text = currentOptions[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.color = active ? selectedColor : defaultColor;

            float targetScale = active ? selectedScale : 1f;
            if (instant)
                currentOptions[i].transform.localScale = Vector3.one * targetScale;
        }
    }

    void HighlightAllOptionsDefault()
    {
        foreach (var option in currentOptions)
        {
            var settingOption = option as SettingOption;
            if (settingOption != null)
                settingOption.SetSelected(false);

            var text = option.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.color = defaultColor;

            option.transform.localScale = Vector3.one;
        }
    }

    void AnimateSelection()
    {
        for (int i = 0; i < currentOptions.Count; i++)
        {
            float targetScale = (i == selectedIndex) ? selectedScale : 1f;

            currentOptions[i].transform.localScale = Vector3.Lerp(
                currentOptions[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * transitionSpeed
            );

            var text = currentOptions[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                Color targetColor = (i == selectedIndex) ? selectedColor : defaultColor;
                text.color = Color.Lerp(
                    text.color,
                    targetColor,
                    Time.deltaTime * transitionSpeed
                );
            }
        }
    }
}
