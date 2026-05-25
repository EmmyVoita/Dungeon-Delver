using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReturnFromSettingsOption : BaseSettingOption
{
    
    public SettingsMenuNavigator settingsMenuNavigator;
    public override void AdjustValue(int direction)
    {
        // No adjustment for this option
    }

    override public void OnPointerClick(PointerEventData eventData)
    {
        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;
            
        OnActivate();
    }

    public override void OnActivate()
    {
        //SettingsMenuClosed?.Invoke();
        //settingsMenuNavigator.HideSettingsMenu();
        // Start the coroutine to close the settings menu
        //StartCoroutine(close());
    }
}
