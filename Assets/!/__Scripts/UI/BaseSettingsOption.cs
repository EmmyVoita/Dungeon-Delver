using UnityEngine;

public abstract class BaseSettingOption : MonoBehaviour
{
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

    // Called when pressing left/right
    public abstract void AdjustValue(int direction);

    // Called when pressing Enter or Space (for toggles, keybinds, etc.)
    public virtual void OnActivate() { }
}
