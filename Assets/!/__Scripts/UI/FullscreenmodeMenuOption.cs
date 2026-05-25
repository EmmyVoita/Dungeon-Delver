using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullscreenmodeMenuOption : BaseSettingOption
{
    public FullscreenManager fullscreenManager;
    public TextMeshProUGUI keyText;

    private void Start()
    {
        keyText.text = Screen.fullScreen ? "Fullscreen" : "Windowed";
    }

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
        bool newState = !fullscreenManager.IsFullscreen;

        fullscreenManager.ToggleFullscreen(newState);

        StartCoroutine(UpdateTextNextFrame(newState));
    }

    IEnumerator UpdateTextNextFrame(bool state)
    {
        yield return null; // wait 1 frame

        keyText.text = state ? "Fullscreen" : "Windowed";
    }
}
