using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem.Controls;

public class InputBindingOption : BaseSettingOption
{
    [SerializeField] private InputActionType actionType;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI keyDisplay;

    private bool waitingForInput = false;

    private void OnEnable()
    {
        InputBindingManager.OnResetKeybinds += UpdateKeyDisplay;
    }

    private void OnDisable()
    {
        InputBindingManager.OnResetKeybinds -= UpdateKeyDisplay;
    }

    override public void AdjustValue(int direction)
    {
        // No adjustment for key bindings
    }

    public override void OnActivate()
    {
        BeginRebind();
    }

    void Start()
    {
        label.text = actionType.ToString();
        UpdateKeyDisplay();
    }

    void UpdateKeyDisplay()
    {
        var key = InputBindingManager.Instance.GetKey(actionType);
        keyDisplay.text = key.ToString();
    }

    public void BeginRebind()
    {
        if (!waitingForInput)
            StartCoroutine(WaitForKeyPress());
    }

    IEnumerator WaitForKeyPress()
    {
        LockNavigation = true;
        waitingForInput = true;
        keyDisplay.text = "_";
        keyDisplay.color = Color.yellow;

        yield return null;


        bool keyAssigned = false;
        while (!keyAssigned)
        {
            foreach (KeyControl keyControl in Keyboard.current.allKeys)
            {
                if (keyControl.wasPressedThisFrame)
                {
                    bool success = InputBindingManager.Instance.TrySetKey(actionType, keyControl.keyCode);
                    if(success)
                    {
                        keyAssigned = true;
                        keyDisplay.color = Color.white;
                        break;
                    }
                    else
                    {
                        // Key is invalid → show feedback
                        keyDisplay.text = "X";
                        keyDisplay.color = Color.red;
                        yield return new WaitForSeconds(0.2f);

                        keyDisplay.text = "_";
                        keyDisplay.color = Color.yellow;
                    }    
                }
            }
            yield return null;
        }

        LockNavigation = false;

        waitingForInput = false;
        UpdateKeyDisplay();
    }
}
