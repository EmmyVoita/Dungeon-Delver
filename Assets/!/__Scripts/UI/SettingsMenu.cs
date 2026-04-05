using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsTabManager tabManager; // 👈 link to tab manager in inspector
    [SerializeField] private Transform currentPanel;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private AudioClip naviagateSwitchSound;

    private List<BaseSettingOption> currentOptions = new List<BaseSettingOption>();
    private int selectedIndex = 0;

    void Start()
    {
        LoadPanelOptions();
        HighlightOption(selectedIndex);
    }

    void LoadPanelOptions()
    {
        currentOptions.Clear();
        currentOptions.AddRange(currentPanel.GetComponentsInChildren<BaseSettingOption>(true)); // include inactive
        selectedIndex = 0;
        HighlightOption(selectedIndex);
    }

    public void ChangeActivePanel(Transform newPanel)
    {
        currentPanel = newPanel;
        LoadPanelOptions();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if(!UIManager.IsPaused) return;

        // 🔹 Handle tab switching first
        if (kb.qKey.wasPressedThisFrame)
            tabManager.MoveTab(-1);
        else if (kb.eKey.wasPressedThisFrame)
            tabManager.MoveTab(1);

        // 🔹 If no options (empty tab), skip navigation
        if (currentOptions.Count == 0)
            return;

        // 🔹 Navigate inside the current tab
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
            MoveSelection(-1);
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
            MoveSelection(1);

        // 🔹 Adjust selected value
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
            currentOptions[selectedIndex].AdjustValue(-1);
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
            currentOptions[selectedIndex].AdjustValue(1);

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            currentOptions[selectedIndex].OnActivate();
    }

    void MoveSelection(int direction)
    {
        selectedIndex = Mathf.Clamp(selectedIndex + direction, 0, currentOptions.Count - 1);
        if (naviagateSwitchSound != null && UIManager.IsPaused)
            AudioHelpers.PlayMyClipAtPoint(naviagateSwitchSound, AudioChannel.UI, Camera.main.transform.position, 1.0f);
        HighlightOption(selectedIndex);
    }

    void HighlightOption(int index)
    {
        for (int i = 0; i < currentOptions.Count; i++)
        {
            float scale = (i == index) ? selectedScale : 1f;
            currentOptions[i].transform.localScale = Vector3.one * scale;
        }
    }
}
