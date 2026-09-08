using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundOption : BaseSettingOption
{
   public TextMeshProUGUI keyText;

    private void Start()
    {
        keyText.text = BackgroundPreference.UseSimpleBackground ? "On" : "Off";
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
        bool newState = !BackgroundPreference.UseSimpleBackground;

        BackgroundPreference.UseSimpleBackground = newState;

        StartCoroutine(UpdateTextNextFrame(newState));
    }

    IEnumerator UpdateTextNextFrame(bool state)
    {
        yield return null; 
        keyText.text = state ? "On" : "Off";
    }
}

