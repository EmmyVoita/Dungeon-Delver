using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class SettingsTabManager : MonoBehaviour
{
    [System.Serializable]
    public class SettingsTab
    {
        public string name;
        public GameObject panel;
        public TextMeshProUGUI tabLabel;
    }

    [Header("References")]
    [SerializeField] private SettingsMenuNavigator settingsMenu;
    [SerializeField] private List<SettingsTab> tabs = new();

    [Header("Visuals")]
    [SerializeField] private Color activeTabColor = Color.white;      // color for current active tab (not focused)
    [SerializeField] private Color focusedTabColor = Color.yellow;    // color when navigating tabs
    [SerializeField] private Color inactiveTabColor = new Color(1f, 1f, 1f, 0.5f); // dimmed inactive tabs

    [SerializeField] private float highlightScale = 1.1f;  // slightly larger when active tab (not focused)
    [SerializeField] private float tabFocusScale = 1.3f;   // larger when player is on tab row
    [SerializeField] private float transitionDuration = 0.25f;
    [SerializeField] private AudioClip tabSwitchSound;

    private int currentTab = 0;
    private bool inTabFocus = false;

    void Start()
    {
        ActivateTab(currentTab, false);
    }

    /// <summary>
    /// Called by SettingsMenuNavigator when user moves up/down into tab mode
    /// </summary>
    public void SetTabFocus(bool active)
    {
        inTabFocus = active;
        UpdateTabVisuals();
    }

    /// <summary>
    /// Move horizontally between tabs
    /// </summary>
    public void MoveTab(int dir)
    {
        AudioSettingsManager.PlayNavigateSound();
        
        int newTab = Mathf.Clamp(currentTab + dir, 0, tabs.Count - 1);
        if (newTab != currentTab)
        {
            currentTab = newTab;
            ActivateTab(currentTab);
        }
    }

    /// <summary>
    /// Activates a specific tab and its panel, without automatically jumping into its options.
    /// </summary>
    private void ActivateTab(int index, bool playSound = true)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isActive = (i == index);

            if (tabs[i].panel != null)
                tabs[i].panel.SetActive(isActive);
        }

        /*
        // Play sound when switching tabs
        if (playSound && UIManager.IsPaused)
        {
            AudioSettingsManager.PlayNavigateSound();
        }
        */

        // Tell settings menu which panel to show
        if (settingsMenu != null && tabs[index].panel != null)
        {
            settingsMenu.ChangeActivePanel(tabs[index].panel.transform, fromTabSwitch: true);
        }

        UpdateTabVisuals();
    }

    /// <summary>
    /// Updates the appearance of all tab labels (color and scale)
    /// depending on which is active and whether we're focused on the tab row.
    /// </summary>
    private void UpdateTabVisuals()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isSelected = (i == currentTab);
            float targetScale = isSelected
                ? (inTabFocus ? tabFocusScale : highlightScale)
                : 1f;

            if (tabs[i].tabLabel != null)
            {
                // Set colors depending on focus state
                if (isSelected)
                    tabs[i].tabLabel.color = inTabFocus ? focusedTabColor : activeTabColor;
                else
                    tabs[i].tabLabel.color = inactiveTabColor;

                // Smooth scale transition
                tabs[i].tabLabel.rectTransform
                    .DOScale(targetScale, transitionDuration)
                    .SetEase(Ease.OutQuad);
            }
        }
    }
}
