using UnityEngine;

public abstract class BaseMenu : MonoBehaviour
{
    public StartMenuWindows menuType;
    protected bool isActive;
    protected bool lockInput;

    private int lockDepth = 0;

    public virtual void OnOpen()
    {
        lockDepth = 0;
        lockInput = false;
        isActive = true;
        //gameObject.SetActive(true);
        Debug.Log($"[Menu] Opened {menuType}");
    }

    public virtual void OnClose()
    {
        lockInput = true;
        isActive = false;
        //gameObject.SetActive(false);
        lockDepth = 0;
        Debug.Log($"[Menu] Closed {menuType}");
    }

    // 🔹 Optional hooks for animation or fades
    public virtual void OnTransitionIn() { }
    public virtual void OnTransitionOut() { }

    // --------------------------------------------------------
    // 🔒 INPUT LOCK SYSTEM (supports nested locks)
    // --------------------------------------------------------
    public void SetInputLocked(bool locked)
    {
        if (locked)
            lockDepth++;
        else
            lockDepth = Mathf.Max(lockDepth - 1, 0);

        lockInput = lockDepth > 0;

        Debug.Log($"[Menu] {menuType} input locked = {lockInput} (depth: {lockDepth})");
    }

    public bool IsActive => isActive;
    public bool IsInputLocked => lockInput;
}
