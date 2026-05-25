using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;


public class SettingsMenuNavigator : BaseMenu
{
    public static Action SettingsMenuClosed;

    [Header("References")]
    [SerializeField] private GameObject holderObject;
    [SerializeField] private Transform currentPanel;
    [SerializeField] private SettingsTabManager tabManager;
    [SerializeField] private RectTransform horzontalBar;
    [SerializeField] private Image barImage;

    [Header("Visuals")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;
    [SerializeField] private float barAlpha = 0.2f;

    private List<BaseSettingOption> currentOptions = new List<BaseSettingOption>();
    private int selectedIndex = -1;
    private bool enableInput = false;
    [SerializeField] private bool inTabNavigation = false;

    void Awake()
    {
        lockInput = true;
    }

    void Start()
    {
        //holderObject.SetActive(false);
        LoadPanelOptions();
    }

    private void OnEnable()
    {
        BaseSettingOption.OnSettingOptionEnter += HandlePointerEnter;
    }

    private void OnDisable()
    {
        BaseSettingOption.OnSettingOptionEnter -= HandlePointerEnter;
    }

    private void HandlePointerEnter(BaseSettingOption option)
    {
        int index = currentOptions.IndexOf(option);

        if (index == -1)
            return;

        inTabNavigation = false;
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        selectedIndex = index;

        barImage.DOKill();
        barImage.DOFade(barAlpha,  0.15f);
        tabManager.SetTabFocus(false);

        HighlightOption(selectedIndex, true);
    }

    public override void OnOpen()
    {
        lockInput = false;
        base.OnOpen();

        ScreenDimmerManager.Instance.AddDimSource("SettingsMenu");

        //holderObject.SetActive(true);

        // Start on the tabs
        barImage.DOKill();
        barImage.color = new Color(barImage.color.r, barImage.color.g, barImage.color.b, 0f);
        inTabNavigation = true;
        tabManager.SetTabFocus(true);

        HighlightAllOptionsDefault();
    }

    public override void OnClose()
    {
        base.OnClose();

        ScreenDimmerManager.Instance.RemoveDimSource("SettingsMenu");

        //holderObject.SetActive(false);
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
            barImage.DOKill();
            barImage.DOFade(0.0f, 0.1f);
            inTabNavigation = true;
            tabManager.SetTabFocus(true);
            HighlightAllOptionsDefault();
        }
        else
        {
            Debug.LogError("In navigation false");
            barImage.DOKill();
            barImage.DOFade(barAlpha, 0.1f);
            inTabNavigation = false;
        }
    }

    void Update()
    {
        if (lockInput || !isActive) return;

        if (currentOptions.Count == 0)
            return;

        BaseSettingOption current = currentOptions[selectedIndex];

        if(!inTabNavigation)
        {
            RectTransform currentRect = current.GetComponent<RectTransform>();

            Vector3 worldPos = currentRect.position;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                horzontalBar.parent as RectTransform,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out localPos
            );

            horzontalBar.anchoredPosition = new Vector2(
                horzontalBar.anchoredPosition.x,
                localPos.y
            );
        }

        
 
        // -----------------------------
        // Exit settings
        // -----------------------------
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back) && !current.IsNavigationLocked)
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
            SettingsMenuClosed?.Invoke();
            MenuManager.Instance.RequestMenuTransition(MenuState.Main);
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
                barImage.DOKill();
                barImage.DOFade(barAlpha,  0.15f);
                tabManager.SetTabFocus(false);
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
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
                barImage.DOKill();
                barImage.DOFade(0.0f, 0.15f);
                // Move back to tab mode
                inTabNavigation = true;
                tabManager.SetTabFocus(true);
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
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
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        HighlightOption(selectedIndex);
    }

    void HighlightOption(int index, bool instant = false)
    {
        for (int i = 0; i < currentOptions.Count; i++)
        {
            bool active = (i == index);

            var settingOption = currentOptions[i] as AudioSettingOption;
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
            var settingOption = option as AudioSettingOption;
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
