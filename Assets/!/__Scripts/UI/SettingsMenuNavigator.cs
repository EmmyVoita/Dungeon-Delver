using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;


public class SettingsMenuNavigator : BaseMenu
{


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

    [Header("TransitionSettings")]
    [SerializeField] private MenuState targetExitState = MenuState.Main;


    private List<BaseSettingOption> _currentOptions = new List<BaseSettingOption>();
    private int _selectedIndex = -1;
    private bool _inTabNavigation = false;


    private void OnEnable()
    {
        BaseSettingOption.OnSettingOptionEnter += HandlePointerEnter;
    }

    private void OnDisable()
    {
        BaseSettingOption.OnSettingOptionEnter -= HandlePointerEnter;
    }

    void Start()
    {
        LoadPanelOptions();
    }

    public override void OnOpen()
    {
        base.OnOpen();

        ScreenDimmerManager.Instance.AddDimSource("SettingsMenu");

        InputFocusManager.Claim(this);

        // Start on the tabs
        barImage.DOKill();
        barImage.color = new Color(barImage.color.r, barImage.color.g, barImage.color.b, 0f);
        _inTabNavigation = true;
        tabManager.SetTabFocus(true);

        DeselectAllOptions();
    }

    public override void OnClose()
    {
        base.OnClose();
        ScreenDimmerManager.Instance.RemoveDimSource("SettingsMenu");
        _selectedIndex = 0;

        InputFocusManager.Release(this);
    }

    void Update()
    {
        if (!CanProcessInput) 
            return;

        if (_currentOptions.Count == 0)
            return;

        BaseSettingOption current = _currentOptions[_selectedIndex];

        // Animation
        AnimateHorizontalBar(current);
        AnimateSelection();

        // Handle trying to exit out of the settings menu
        if(HandleBackInput(current))
            return;
      
        //  block all navigation if current option locked
        if(HandleLockCurrentOption(current))
            return;

        // Tab navigation
        if(HandleTabNaviagation())
            return;

        // Moving the selection Up + Down
        if(HandleMoveSelectionVerticalInput())
            return;

        // Adjust current setting (left/right)
        HandleCurrentSettingInput(current);
    }

    // Handling Input
    // -----------------------------------------------------------------------------

    private bool HandleBackInput(BaseSettingOption current)
    {
        if(current.IsNavigationLocked)
            return false;

        if(!InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
            return false;
        
        // Audio Feedback
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);

        // Menu Manager transition to exit menu. Main menu by default.
        MenuManager.Instance.RequestMenuTransition(targetExitState);
        return true;
    }

    private bool HandleLockCurrentOption(BaseSettingOption current)
    {
        if(!current.IsNavigationLocked)
            return false;

        // Still allow confirmation for in-progress interactions
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            current.OnActivate();

        // block all movement
        return true; 
    }

    private bool HandleTabNaviagation()
    {
        if (!_inTabNavigation)
            return false;
    
        tabManager.SetTabFocus(true);

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
            tabManager.MoveTab(-1);

        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
            tabManager.MoveTab(1);

        // when we move down from the tab we load into the pages options
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            _inTabNavigation = false;
            barImage.DOKill();
            barImage.DOFade(barAlpha,  0.15f).SetUpdate(true);
            tabManager.SetTabFocus(false);
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            HighlightOption(_selectedIndex, instant: true);
        }

        return true;
    }

    private void HandleCurrentSettingInput(BaseSettingOption current)
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
            current.AdjustValue(-1);

        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
            current.AdjustValue(1);

        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            current.OnActivate();
    }

    private bool HandleMoveSelectionVerticalInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            if (_selectedIndex == 0)
            {
                // Hide the bar image
                barImage.DOKill();
                barImage.DOFade(0.0f, 0.15f).SetUpdate(true);
                
                // Move back to tab mode
                _inTabNavigation = true;
                tabManager.SetTabFocus(true);

                // Audio feedback
                AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);

                DeselectAllOptions();
                return true;
            }
            else
            {
                MoveSelection(-1);
                return true;
            }
        }

        // Move selection DOWN
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            MoveSelection(1);
            return true;
        }
            
        return false;
    }


 

    void LoadPanelOptions()
    {
        _currentOptions.Clear();
        _currentOptions.AddRange(currentPanel.GetComponentsInChildren<BaseSettingOption>(true));
        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _currentOptions.Count - 1);
    }

    public void ChangeActivePanel(Transform newPanel, bool fromTabSwitch = false)
    {
        currentPanel = newPanel;
        LoadPanelOptions();

        if (fromTabSwitch)
        {
            barImage.DOKill();
            barImage.DOFade(0.0f, 0.1f).SetUpdate(true);
            _inTabNavigation = true;
            tabManager.SetTabFocus(true);
            DeselectAllOptions();
        }
        else
        {
            Debug.LogError("In navigation false");
            barImage.DOKill();
            barImage.DOFade(barAlpha, 0.1f).SetUpdate(true);
            _inTabNavigation = false;
        }
    }

   

    void MoveSelection(int direction)
    {
        _selectedIndex = Mathf.Clamp(_selectedIndex + direction, 0, _currentOptions.Count - 1);
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        HighlightOption(_selectedIndex);
    }

 

    // Animation / Visuals
    // ------------------------------------------------------------------------------------------

    void AnimateSelection()
    {
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            float targetScale = (i == _selectedIndex) && !_inTabNavigation ? selectedScale : 1f;

            _currentOptions[i].transform.localScale = Vector3.Lerp(
                _currentOptions[i].transform.localScale,
                Vector3.one * targetScale,
                Time.unscaledDeltaTime * transitionSpeed
            );

            var text = _currentOptions[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                Color targetColor = (i == _selectedIndex) && !_inTabNavigation ? selectedColor : defaultColor;
                text.color = Color.Lerp(
                    text.color,
                    targetColor,
                    Time.unscaledDeltaTime * transitionSpeed
                );
            }
        }
    }

    private void AnimateHorizontalBar(BaseSettingOption current)
    {
        if(!_inTabNavigation)
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
    }

    void HighlightOption(int index, bool instant = false)
    {
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            bool active = (i == index);

            var settingOption = _currentOptions[i] as AudioSettingOption;
            if (settingOption != null)
                settingOption.SetSelected(active);

            var text = _currentOptions[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.color = active ? selectedColor : defaultColor;

            float targetScale = active ? selectedScale : 1f;
            if (instant)
                _currentOptions[i].transform.localScale = Vector3.one * targetScale;
        }
    }

    void DeselectAllOptions()
    {
        foreach (var option in _currentOptions)
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


    // 

    private void HandlePointerEnter(BaseSettingOption option)
    {
        int index = _currentOptions.IndexOf(option);

        if (index == -1)
            return;

        _inTabNavigation = false;
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
        _selectedIndex = index;

        barImage.DOKill();
        barImage.DOFade(barAlpha,  0.15f).SetUpdate(true);
        tabManager.SetTabFocus(false);

        HighlightOption(_selectedIndex, true);
    }
}
