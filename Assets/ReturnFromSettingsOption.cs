using System;
using System.Collections;
using UnityEngine;

public class ReturnFromSettingsOption : BaseSettingOption
{
    
    public SettingsMenuNavigator settingsMenuNavigator;
    public override void AdjustValue(int direction)
    {
        // No adjustment for this option
    }

    public override void OnActivate()
    {
        //SettingsMenuClosed?.Invoke();
        //settingsMenuNavigator.HideSettingsMenu();
        // Start the coroutine to close the settings menu
        //StartCoroutine(close());
    }
}
