using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseSettingOption : MonoBehaviour, 
                                        IPointerClickHandler, 
                                        IPointerEnterHandler, 
                                        IPointerExitHandler
{
    public static event Action<BaseSettingOption> OnSettingOptionEnter;

    protected bool LockNavigation { get; set; }
    public bool IsSelected { get; private set; }
    public bool IsNavigationLocked => LockNavigation;


    public virtual void OnSelected()
    {
        IsSelected = true;
        // Example visual feedback (e.g. highlight)
        transform.localScale = Vector3.one * 1.1f;
    }

    public virtual void OnDeselected()
    {
        IsSelected = false;
        transform.localScale = Vector3.one;
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        OnSettingOptionEnter?.Invoke(this);
    }


    public virtual void OnPointerExit(PointerEventData eventData)
    {
        
    }

    // Called when pressing left/right
    public abstract void AdjustValue(int direction);

    // Called when pressing Enter or Space (for toggles, keybinds, etc.)
    public virtual void OnActivate() { }
}
