using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem.Controls;
using UnityEngine.EventSystems;

public class InputBindingOption : BaseSettingOption
{
    private static InputBindingOption activeRebind;

    [SerializeField] private InputActionType actionType;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI keyDisplay;

    private bool _waitingForInput = false;

    private void OnEnable()
    {
        InputBindingManager.OnResetKeybinds += UpdateKeyDisplay;
    }

    private void OnDisable()
    {
        InputBindingManager.OnResetKeybinds -= UpdateKeyDisplay;
    }

    override public void OnPointerClick(PointerEventData eventData)
    {
        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        if(_waitingForInput)
            return;

        BeginRebind();
    }

    override public void AdjustValue(int direction)
    {
        // No adjustment for key bindings
    }

    public override void OnActivate()
    {
        SoundEffect adjustSound = AudioLibrary.Instance.Database.navigate;
        AudioHelpers.PlaySoundEffect(adjustSound,Camera.main.transform.position);
        BeginRebind();
    }

    void Start()
    {
        label.text = actionType.ToString();
        UpdateKeyDisplay();
    }

    void UpdateKeyDisplay()
    {
        var key = InputBindingManager.Instance.GetKeyName(actionType);
        keyDisplay.text = key.ToString();
    }

    public void BeginRebind()
    {
        // Cancel previous active binding
        if (activeRebind != null &&
            activeRebind != this)
        {
            activeRebind.CancelRebind();
        }

        activeRebind = this;
        
        if (!_waitingForInput)
            StartCoroutine(WaitForKeyPress());
    }

    public void CancelRebind()
    {
        StopAllCoroutines();

        LockNavigation = false;
        _waitingForInput = false;

        UpdateKeyDisplay();
        keyDisplay.color = Color.white;
    }

    IEnumerator WaitForKeyPress()
    {
        LockNavigation = true;
        _waitingForInput = true;
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
                        SoundEffect confrimSound = AudioLibrary.Instance.Database.select;
                        AudioHelpers.PlaySoundEffect(confrimSound,Camera.main.transform.position);


                        keyAssigned = true;
                        keyDisplay.color = Color.white;
                        break;
                    }
                    else
                    {
                        SoundEffect negativeSound = AudioLibrary.Instance.Database.negative;
                        AudioHelpers.PlaySoundEffect(negativeSound,Camera.main.transform.position);

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

        _waitingForInput = false;
        UpdateKeyDisplay();
    }
}
