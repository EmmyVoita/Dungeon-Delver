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
    [SerializeField] private bool openMenuOnStart = true;


    [Header("Menu References")]
    [SerializeField] private List<BaseMenu> menuList = new ();

    
    [Header("Dynamic")]
    [SerializeField] private BaseMenu _activeMenu;
    [SerializeField] private MenuState _previousMenu = MenuState.None;
    [SerializeField] private bool _isTransitioning = false;


    private Dictionary<MenuState, BaseMenu> _lookup = new();


    public MenuState CurrentState => _activeMenu != null ? _activeMenu.menuType : MenuState.None;
    public BaseMenu ActiveMenu => _activeMenu;
    public MenuState PreviousMenuState => _previousMenu;
    public bool ActiveMenuLocked => !_activeMenu.CanProcessInput;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

         // Convert List to dictionary
        _lookup = new Dictionary<MenuState, BaseMenu>();

        foreach (BaseMenu entry in menuList)
        {
             if (!_lookup.ContainsKey(entry.menuType))
            {
                _lookup.Add(entry.menuType, entry);
            }
        }
    }


    void Start()
    {   
        if(!openMenuOnStart)
            return;

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


    public bool RequestMenuTransition(MenuState newState)
    {
        if (!MenuTransitionManager.Instance.CanStartTransition)
            return false;

        return MenuTransitionManager.Instance.PlayTransition(
            CurrentState,
            newState
        );
    }


    public void OpenMenu(MenuState type)
    {
        if (_activeMenu != null && _activeMenu.menuType == type)
            return;

        BaseMenu oldMenu = _activeMenu;

        if (oldMenu != null)
            oldMenu.OnClose();

        if (type == MenuState.None)
        {
            SetActiveMenu(null);
            OnMenuOpened?.Invoke(MenuState.None);
            return;
        }

        if (_lookup.TryGetValue(type, out BaseMenu menu))
        {
            SetActiveMenu(menu);

            Debug.Log($"Trying to open menu {type}");

            _activeMenu.OnOpen();
            OnMenuOpened?.Invoke(type);
        }
        else
        {
            Debug.LogWarning(
                $"MenuManager: No menu registered for {type}"
            );
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
            SetActiveMenu(newMenu);
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

    private void SetActiveMenu(BaseMenu newMenu)
    {
        if (_activeMenu == newMenu)
            return;

        _previousMenu =_activeMenu != null? _activeMenu.menuType: MenuState.None;

        _activeMenu = newMenu;
    }


    //  Helper Utilities
    public void CloseCurrentMenu()
    {
        if (_activeMenu == null)
        {
            _previousMenu = MenuState.None;
            return;
        }

        _previousMenu = _activeMenu.menuType;

        _activeMenu.OnClose();
        _activeMenu = null;
    }
}
