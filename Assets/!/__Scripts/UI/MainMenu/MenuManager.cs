using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;




public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    public static event Action<MenuState> OnMenuOpened;


    [Header("Transition Settings")]
    public float defaultTransitionDelay = 0.25f; 


    [SerializeField] private List<BaseMenu> menuList = new ();

    private Dictionary<MenuState, BaseMenu> _lookup = new();
    [SerializeField] private BaseMenu _activeMenu;
    [SerializeField] private bool _isTransitioning = false;


    public MenuState CurrentState => _activeMenu != null ? _activeMenu.menuType : MenuState.None;
    public BaseMenu ActiveMenu => _activeMenu;
    public bool ActiveMenuLocked => _activeMenu.IsInputLocked;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    void Start()
    {
        // Convert List to dictionary
        _lookup = new Dictionary<MenuState, BaseMenu>();

        foreach (BaseMenu entry in menuList)
        {
             if (!_lookup.ContainsKey(entry.menuType))
            {
                _lookup.Add(entry.menuType, entry);
            }
        }

        // Load menu from game scene config if it exists.
        GameSceneConfig config = GameSessionBootstrap.Config;

        if(config != null)
        {
            MenuState target = config.ReturnTarget;
            OpenMenu(target);
        }
        else
        {
           OpenMenu(MenuState.Main); 
        }
    }


    public void RequestMenuTransition(MenuState newState)
    {
        MenuTransitionManager.Instance.PlayTransition(CurrentState, newState);
    }


    public void OpenMenu(MenuState type)
    {
        if (_activeMenu != null && _activeMenu.menuType == type)
            return;

        OnMenuOpened?.Invoke(type);
        Debug.Log($"Trying to open menu {type}");

        if (_activeMenu != null)
            _activeMenu.OnClose();

        if (_lookup.TryGetValue(type, out var menu))
        {
            _activeMenu = menu;
            _activeMenu.OnOpen();
        }
        else
        {
            Debug.LogWarning($"⚠️ MenuManager: No menu registered for {type}");
        }
    }


    public void TransitionToMenu(MenuState nextMenu, float delay = -1f)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionRoutine(nextMenu, delay < 0 ? defaultTransitionDelay : delay));
    }

    private IEnumerator TransitionRoutine(MenuState nextMenu, float delay)
    {
        _isTransitioning = true;

        // 🔸 Step 1: Close current menu
        if (_activeMenu != null)
        {
            _activeMenu.OnTransitionOut();
            yield return new WaitForSecondsRealtime(delay);
            _activeMenu.OnClose();
        }

        // 🔸 Step 2: Open the next menu
        if (_lookup.TryGetValue(nextMenu, out var newMenu))
        {
            _activeMenu = newMenu;
            _activeMenu.OnOpen();
            _activeMenu.OnTransitionIn();
            OnMenuOpened?.Invoke(nextMenu);
        }
        else
        {
            Debug.LogWarning($"⚠️ MenuManager: No menu registered for {nextMenu}");
        }

        yield return null;
        _isTransitioning = false;
    }

    // Input Lock System (delegated to active menu)
    public void LockActiveMenuInput(bool locked, float delay = 0f)
    {
        StartCoroutine(DelayedInputLock(locked, delay));
    }

    private IEnumerator DelayedInputLock(bool locked, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (_activeMenu != null)
            _activeMenu.SetInputLocked(locked);
    }

    //  Helper Utilities
    public void CloseCurrentMenu()
    {
        if (_activeMenu != null)
        {
            _activeMenu.OnClose();
            _activeMenu = null;
        }
    }


}
