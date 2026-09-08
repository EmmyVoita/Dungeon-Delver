using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindingManager : MonoBehaviour
{
    public static InputBindingManager Instance { get; private set; }
    
    public static event Action OnJumpPressed;
    public static event Action OnConfirmPressed;
    public static event Action OnResetKeybinds;

    private const string FileName = " myInputBindings.json";
    private Dictionary<InputActionType, Key> bindings = new();

    private static readonly Dictionary<Key, string> DisplayNames = new()
    {
        { Key.Space, "SPC" },
        { Key.Enter, "ENT" },
        { Key.Escape, "ESC" },

        { Key.LeftArrow, "←" },
        { Key.RightArrow, "→" },
        { Key.UpArrow, "↑" },
        { Key.DownArrow, "↓" }
    };


    private static readonly HashSet<Key> ForbiddenKeys = new()
    {
        Key.PrintScreen,
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5,
        Key.LeftWindows, Key.RightWindows
    };



    [Serializable]
    private class BindingEntry
    {
        public string action;
        public string key;
    }

    [Serializable]
    private class BindingList
    {
        public List<BindingEntry> bindings = new();
    }

    private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private readonly HashSet<InputActionType> blockedUntilRelease = new();

    public void BlockUntilRelease(InputActionType action)
    {
        blockedUntilRelease.Add(action);
    }

    private bool IsBlocked(InputActionType action, Key key)
    {
        if (!blockedUntilRelease.Contains(action))
            return false;

        // Once the physical key has been released,
        // allow this action again.
        if (!Keyboard.current[key].isPressed)
            blockedUntilRelease.Remove(action);

        return true;
    }

    // ------------------------------------------------------------
    // Initialization
    // ------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBindings();
    }

    private void UpdateBlockedInputs()
    {
        if (Keyboard.current == null)
            return;

        blockedUntilRelease.RemoveWhere(action =>
        {
            if (!bindings.TryGetValue(action, out Key key))
                return true;

            return !Keyboard.current[key].isPressed;
        });
    }

    void Update()
    {
        UpdateBlockedInputs();

        if (GetKeyDown(InputActionType.Confirm))
            OnConfirmPressed?.Invoke();

        if (GetKeyDown(InputActionType.Jump))
            OnJumpPressed?.Invoke();
    }

    // ------------------------------------------------------------
    // Default bindings fallback
    // ------------------------------------------------------------

    public void ResetKeybinds()
    {
        LoadDefaultBindings();
        SaveBindings();
        OnResetKeybinds?.Invoke();
    }

    private void LoadDefaultBindings()
    {
        bindings.Clear();
        bindings[InputActionType.MoveUp] = Key.W;
        bindings[InputActionType.MoveDown] = Key.S;
        bindings[InputActionType.MoveLeft] = Key.A;
        bindings[InputActionType.MoveRight] = Key.D;
        bindings[InputActionType.UseAbility] = Key.E;
        bindings[InputActionType.Jump] = Key.Space;
        bindings[InputActionType.Confirm] = Key.Enter;
        bindings[InputActionType.Back] = Key.Escape;
        bindings[InputActionType.Reroll] = Key.R;
        bindings[InputActionType.ViewUpgrades] = Key.Tab;
    }

    // ------------------------------------------------------------
    // JSON Save / Load
    // ------------------------------------------------------------
    public void SaveBindings()
    {
        var list = new BindingList();

        foreach (var pair in bindings)
        {
            list.bindings.Add(new BindingEntry
            {
                action = pair.Key.ToString(),
                key = pair.Value.ToString()
            });
        }

        string json = JsonUtility.ToJson(list, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"💾 Saved input bindings to: {FilePath}");
    }

    public void LoadBindings()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log("⚠️ No binding file found — using defaults.");
            LoadDefaultBindings();
            SaveBindings();
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            var list = JsonUtility.FromJson<BindingList>(json);

            bindings.Clear();
            foreach (var entry in list.bindings)
            {
                if (Enum.TryParse(entry.action, out InputActionType action) &&
                    Enum.TryParse(entry.key, out Key key))
                {
                    bindings[action] = key;
                }
            }

            Debug.Log("✅ Loaded input bindings from JSON.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"⚠️ Failed to load bindings: {e.Message}. Loading defaults.");
            LoadDefaultBindings();
        }
    }

    // ------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------
    public Key GetBoundKey(InputActionType action)
    {
        return bindings.ContainsKey(action) ? bindings[action] : Key.None;
    }

    public string GetKeyDisplayName(Key key)
    {
        if (DisplayNames.TryGetValue(key, out string name))
            return name;

        return key.ToString().ToUpper();
    }

    public bool TrySetKey(InputActionType action, Key newKey)
    {
        // Forbidden keys
        if (ForbiddenKeys.Contains(newKey))
            return false;

        // No two actions can share the same key
        foreach (var pair in bindings)
        {
            if (pair.Key != action && pair.Value == newKey)
            {
                return false;
            }
        }

        bindings[action] = newKey;
        SaveBindings();

        return true;
    }

    
    public bool GetKeyDown(InputActionType action)
    {
        if (!bindings.TryGetValue(action, out var key))
            return false;

        if (key == Key.None || Keyboard.current == null)
            return false;

        if (blockedUntilRelease.Contains(action))
            return false;

        bool fallBackKeyDown = false;

        switch (action)
        {
            case InputActionType.MoveUp:
                fallBackKeyDown = Keyboard.current[Key.UpArrow].wasPressedThisFrame;
                break;
            case InputActionType.MoveDown:
                fallBackKeyDown = Keyboard.current[Key.DownArrow].wasPressedThisFrame;
                break;
            case InputActionType.MoveLeft:
                fallBackKeyDown = Keyboard.current[Key.LeftArrow].wasPressedThisFrame;
                break;
            case InputActionType.MoveRight:
                fallBackKeyDown = Keyboard.current[Key.RightArrow].wasPressedThisFrame;
                break;
        }

        return Keyboard.current[key].wasPressedThisFrame || fallBackKeyDown;
    }


    public bool GetKeyUp(InputActionType action)
    {
        if (!bindings.TryGetValue(action, out var key))
            return false;

        if (key == Key.None || Keyboard.current == null)
            return false;

        if (blockedUntilRelease.Contains(action))
            return false;

        bool fallBackKeyUp = false;

        switch (action)
        {
            case InputActionType.MoveUp:
                fallBackKeyUp = Keyboard.current[Key.UpArrow].wasReleasedThisFrame;
                break;
            case InputActionType.MoveDown:
                fallBackKeyUp = Keyboard.current[Key.DownArrow].wasReleasedThisFrame;
                break;
            case InputActionType.MoveLeft:
                fallBackKeyUp = Keyboard.current[Key.LeftArrow].wasReleasedThisFrame;
                break;
            case InputActionType.MoveRight:
                fallBackKeyUp = Keyboard.current[Key.RightArrow].wasReleasedThisFrame;
                break;
        }

        return Keyboard.current[key].wasReleasedThisFrame || fallBackKeyUp;
    }

    public bool GetKeyHeld(InputActionType action)
    {
         if (!bindings.TryGetValue(action, out var key))
        return false;

        if (key == Key.None || Keyboard.current == null)
            return false;
            
        return Keyboard.current[key].isPressed;
    }


}
