using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;




public class MenuManager : MonoBehaviour
{
    public static event Action<MenuState> OnMenuOpened;

    public static Action OnMainMenuOpened;
    public static Action OnPlayMenuOpened;
    public static Action OnSettingsMenuOpened;

    public static MenuManager Instance { get; private set; }

    private readonly Dictionary<MenuState, BaseMenu> menus = new();
    private BaseMenu activeMenu;
    private bool isTransitioning = false;

    [Header("Transition Settings")]
    public float defaultTransitionDelay = 0.25f; 


    public MenuState CurrentState => activeMenu != null ? activeMenu.menuType : MenuState.None;
    public BaseMenu GetActiveMenu => activeMenu;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void RegisterMenu(BaseMenu menu)
    {
        if (!menus.ContainsKey(menu.menuType))
        {
            menus.Add(menu.menuType, menu);
        }
    }

    public void RequestMenuTransition(MenuState newState)
    {
        MenuTransitionManager.Instance.PlayTransition(CurrentState, newState);
    }


    public void OpenMenu(MenuState type)
    {
        if (activeMenu != null && activeMenu.menuType == type)
            return;

        OnMenuOpened?.Invoke(type);
        Debug.Log($"Trying to open menu {type}");

        if (activeMenu != null)
            activeMenu.OnClose();

        if (menus.TryGetValue(type, out var menu))
        {
            activeMenu = menu;
            activeMenu.OnOpen();
        }
        else
        {
            Debug.LogWarning($"⚠️ MenuManager: No menu registered for {type}");
        }
    }


    public void TransitionToMenu(MenuState nextMenu, float delay = -1f)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(nextMenu, delay < 0 ? defaultTransitionDelay : delay));
    }

    private IEnumerator TransitionRoutine(MenuState nextMenu, float delay)
    {
        isTransitioning = true;

        // 🔸 Step 1: Close current menu
        if (activeMenu != null)
        {
            activeMenu.OnTransitionOut();
            yield return new WaitForSecondsRealtime(delay);
            activeMenu.OnClose();
        }

        // 🔸 Step 2: Open the next menu
        if (menus.TryGetValue(nextMenu, out var newMenu))
        {
            activeMenu = newMenu;
            activeMenu.OnOpen();
            activeMenu.OnTransitionIn();
            OnMenuOpened?.Invoke(nextMenu);
        }
        else
        {
            Debug.LogWarning($"⚠️ MenuManager: No menu registered for {nextMenu}");
        }

        yield return null;
        isTransitioning = false;
    }

    // --------------------------------------------------------
    // 🔹 Input Lock System (delegated to active menu)
    // --------------------------------------------------------
    public void LockActiveMenuInput(bool locked, float delay = 0f)
    {
        StartCoroutine(DelayedInputLock(locked, delay));
    }

    private IEnumerator DelayedInputLock(bool locked, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (activeMenu != null)
            activeMenu.SetInputLocked(locked);
    }

    // --------------------------------------------------------
    // 🔹 Helper Utilities
    // --------------------------------------------------------
    public void CloseCurrentMenu()
    {
        if (activeMenu != null)
        {
            activeMenu.OnClose();
            activeMenu = null;
        }
    }


}
