using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class InputModeManager : MonoBehaviour
{
    public static InputModeManager Instance { get; private set; }

    public enum InputMode
    {
        Mouse, 
        Keyboard
    }

    public static event Action<InputMode> OnInputModeChanged;

    private const string PREF_KEY = "mouseInput_mode"; // 1 = on, 0 = off
    private const bool DEFAULT_MOUSE_INPUT = false;


    public InputMode CurrentMode {get; private set;} = InputMode.Keyboard;

    
    [SerializeField] private List<InputActionType> keyboardInputs; 


    [Header("Dynamic")]
    [SerializeField] private Vector3 _lastMousePosition; 
    [SerializeField] private bool _allowMouseInput = false;

    public bool AllowMouseInput
    {
        get => _allowMouseInput;
        set
        {
            _allowMouseInput = value;

            PlayerPrefs.SetInt(
                PREF_KEY,
                value ? 1 : 0
            );

            PlayerPrefs.Save();

            if (!value)
                SetInputMode(InputMode.Keyboard);
        }
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(GameStateEffectManager.AllowMouseInput && AllowMouseInput)
        {
            SetInputMode(InputMode.Mouse);
        }
        else
        {
            SetInputMode(InputMode.Keyboard);
        }
    }
    

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        _lastMousePosition = Input.mousePosition;

        //Cursor.visible = false;
    }   


    private void Start()
    {
        _allowMouseInput =
            PlayerPrefs.GetInt(
                PREF_KEY,
                DEFAULT_MOUSE_INPUT ? 1 : 0
            ) == 1;

        SetInputMode(
            _allowMouseInput
                ? InputMode.Mouse
                : InputMode.Keyboard
        );
    }


    private void Update()
    {
        if(GameStateEffectManager.AllowMouseInput && AllowMouseInput)
        {
            DetectKeyboardInput();
            DetectMouseInput();
        }
    }

    private void DetectKeyboardInput()
    {
        bool usingKeyboard = false;

        foreach(InputActionType inputAction in keyboardInputs)
        {
            if(InputBindingManager.Instance.GetKeyDown(inputAction))
            {
                usingKeyboard = true;
                break;
            }
        }

        if (usingKeyboard && CurrentMode != InputMode.Keyboard)
        {
            SetInputMode(InputMode.Keyboard);
        }
    }


    private void DetectMouseInput()
    {
        // Check if the mouse has moved
        bool mouseMoved = (Input.mousePosition - _lastMousePosition).sqrMagnitude > 4f;

        bool mouseClicked = Input.GetMouseButtonDown(0);

        _lastMousePosition = Input.mousePosition;

        if((mouseMoved || mouseClicked) && CurrentMode != InputMode.Mouse)
        {
            SetInputMode(InputMode.Mouse);
        }
    }

    private void SetInputMode(InputMode newMode)
    {
        CurrentMode = newMode;

        //Cursor.visible = false;

        CursorController.Instance?.ShowCursor(
            newMode == InputMode.Mouse
        );

        OnInputModeChanged?.Invoke(newMode);
    }
}