using UnityEngine;

public class FullscreenmodeMenuOption : BaseSettingOption
{
    public FullscreenManager fullscreenManager;
    public override void AdjustValue(int direction)
    {
        // No adjustment for this option
    }

    public override void OnActivate()
    {
        fullscreenManager.ToggleFullscreen(!Screen.fullScreen);
    }
}
