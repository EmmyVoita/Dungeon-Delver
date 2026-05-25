using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseInputMenuOption : BaseSettingOption
{
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private string onState = "On";
    [SerializeField] private string offState = "Off";

    private void Start()
    {
        keyText.text = InputModeManager.Instance.AllowMouseInput ? onState : offState;
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
        bool newState = !InputModeManager.Instance.AllowMouseInput;

        InputModeManager.Instance.AllowMouseInput = newState;

        StartCoroutine(UpdateTextNextFrame(newState));
    }

    IEnumerator UpdateTextNextFrame(bool state)
    {
        yield return null; // wait 1 frame

        keyText.text = state ? onState : offState;
    }
}
