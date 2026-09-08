using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class BaseMenu : MonoBehaviour
{
    public MenuState menuType;

    [SerializeField] private GameObject panelRoot;

    protected bool isActive = false;

    protected virtual void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        isActive = false;
    }

    public virtual void OnOpen()
    {
        if (panelRoot != null)
            PanelRoot.SetActive(true);
        
        isActive = true;
    }

    public virtual void OnClose()
    {
        if (panelRoot != null)
            PanelRoot.SetActive(false);
            
        isActive = false;
    }

    public virtual void OnTransitionIn() { }
    public virtual void OnTransitionOut() { }

    public bool IsActive => isActive;
    public GameObject PanelRoot => panelRoot;

    public bool CanProcessInput
    {
        get
        {
            //Debug.Log($"isActive => {isActive}, is menu transitionmanager playing? => {MenuTransitionManager.Instance.IsPlayingTransition}");
            if (!isActive)
                return false;

            //Debug.LogError($"MENU TRANSITION PLAYING => {MenuTransitionManager.Instance.IsPlayingTransition}");

            if (MenuTransitionManager.Instance != null &&
                MenuTransitionManager.Instance.IsPlayingTransition)
                return false;

            return true;
        }
    }
}