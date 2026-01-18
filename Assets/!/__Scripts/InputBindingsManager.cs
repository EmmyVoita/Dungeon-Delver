using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindingManager : MonoBehaviour
{
    public static InputBindingManager Instance { get; private set; }

    public static event Action OnConfirmPressed;
    public static event Action OnResetKeybinds;

    private const string FileName = " myInputBindings.json";
    private Dictionary<InputActionType, Key> bindings = new();

    // Define which actions are exclusive with each other
    private static readonly HashSet<InputActionType> MovementGroup = new()
    {
        InputActionType.MoveUp,
        InputActionType.MoveDown,
        InputActionType.MoveLeft,
        InputActionType.MoveRight
    };

    // Confirm & Back cannot share a key
    private static readonly HashSet<InputActionType> NavigationGroup = new()
    {
        InputActionType.Confirm,
        InputActionType.Back
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

    void Update()
    {
        if (GetKeyDown(InputActionType.Confirm))
        {
            OnConfirmPressed?.Invoke();
        }
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
        bindings[InputActionType.UseAbility] = Key.Enter;
        bindings[InputActionType.Jump] = Key.Space;
        bindings[InputActionType.Confirm] = Key.Enter;
        bindings[InputActionType.Back] = Key.Escape;
        bindings[InputActionType.Interact] = Key.R;
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
    public Key GetKey(InputActionType action)
    {
        return bindings.ContainsKey(action) ? bindings[action] : Key.None;
    }

    public bool TrySetKey(InputActionType action, Key newKey)
    {
        // ------------------------------------------------------------
        // 0. Forbidden keys
        // ------------------------------------------------------------
        if (ForbiddenKeys.Contains(newKey))
            return false;

        // ------------------------------------------------------------
        // 1. Movement group — all actions here must have unique keys
        // ------------------------------------------------------------
        if (MovementGroup.Contains(action))
        {
            foreach (var pair in bindings)
            {
                if (MovementGroup.Contains(pair.Key) &&
                    pair.Key != action &&
                    pair.Value == newKey)
                {
                    return false; // conflict in WASD
                }
            }
        }

        // ------------------------------------------------------------
        // 2. Navigation group — Confirm & Back cannot share keys
        // ------------------------------------------------------------
        if (NavigationGroup.Contains(action))
        {
            foreach (var pair in bindings)
            {
                if (NavigationGroup.Contains(pair.Key) &&
                    pair.Key != action &&
                    pair.Value == newKey)
                {
                    return false; // Confirm clash with Back OR Back clash with Confirm
                }
            }
        }

        // ------------------------------------------------------------
        // 3. All rules passed → apply binding
        // ------------------------------------------------------------
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
            
        return Keyboard.current[key].wasPressedThisFrame;
    }

    public bool GetKeyInput(InputActionType action)
    {
         if (!bindings.TryGetValue(action, out var key))
        return false;

        if (key == Key.None || Keyboard.current == null)
            return false;
            
        return Keyboard.current[key].isPressed;
    }
}
