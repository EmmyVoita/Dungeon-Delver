using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputModeManager : MonoBehaviour
{
    public static InputModeManager Instance { get; private set; }

    public enum InputMode
    {
        Mouse,
        Keyboard
    }

    public static event Action<InputMode> OnInputModeChanged;

    public InputMode CurrentMode { get; private set; } = InputMode.Keyboard;

    [SerializeField]
    private List<InputActionType> keyboardInputs;

    [Header("Mouse Detection")]
    [SerializeField]
    private float mouseMoveThreshold = 2f;

    [Header("Dynamic")]
    [SerializeField] private Vector3 _lastMousePosition;
    [SerializeField] private bool _paused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _lastMousePosition = Input.mousePosition;

        // Start in keyboard mode.
        CurrentMode = InputMode.Keyboard;
        CursorController.Instance?.ShowCursor(false);

        Cursor.visible = false;
        _paused = false;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PauseMenuNavigator.OnPaused += HandlePaused;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PauseMenuNavigator.OnPaused -= HandlePaused;
    }

    private void HandlePaused(bool value)
    {
        _paused = value;

        if (CurrentMode != InputMode.Mouse && _paused)
            SetInputMode(InputMode.Mouse);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == SceneNames.LevelEditor)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }

        if(scene.name == SceneNames.ArrowGameScene)
        {
            SetInputMode(InputMode.Keyboard);
        }

        _paused = false;
    }

    private void Update()
    {
        // Mouse isn't allowed in this state at all.
        if (!GameStateEffectManager.AllowMouseInput && !_paused)
        {
            if (CurrentMode != InputMode.Keyboard)
                SetInputMode(InputMode.Keyboard);

            // Still update this so entering a mouse-enabled state
            // doesn't immediately count old movement as new movement.
            _lastMousePosition = Input.mousePosition;

            return;
        }

        DetectKeyboardInput();
        DetectMouseInput();
    }

    private void HandleStateChanged(GameState previousState,GameState newState)
    {
        // If the new state doesn't support mouse input,
        // immediately return to keyboard mode.
        if (!GameStateEffectManager.AllowMouseInput)
        {
            SetInputMode(InputMode.Keyboard);
        }

        // Reset mouse position so changing state doesn't
        // accidentally trigger mouse mode.
        _lastMousePosition = Input.mousePosition;
    }

    private void DetectKeyboardInput()
    {
        if (InputBindingManager.Instance == null)
            return;

        foreach (InputActionType inputAction in keyboardInputs)
        {
            if (InputBindingManager.Instance.GetKeyDown(inputAction))
            {
                SetInputMode(InputMode.Keyboard);
                return;
            }
        }
    }

    private void DetectMouseInput()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        float thresholdSqr =
            mouseMoveThreshold * mouseMoveThreshold;

        bool mouseMoved =
            (currentMousePosition - _lastMousePosition)
            .sqrMagnitude > thresholdSqr;

        bool mouseClicked =
            Input.GetMouseButtonDown(0);

        bool mouseScrolled =
            Mathf.Abs(Input.mouseScrollDelta.y) > 0.01f;

        _lastMousePosition = currentMousePosition;

        if (mouseMoved || mouseClicked || mouseScrolled)
        {
            SetInputMode(InputMode.Mouse);
        }
    }

    private void SetInputMode(InputMode newMode)
    {
        bool modeChanged = CurrentMode != newMode;

        CurrentMode = newMode;

        CursorController.Instance?.ShowCursor(
            newMode == InputMode.Mouse
        );

        if (modeChanged)
            OnInputModeChanged?.Invoke(newMode);
    }
}