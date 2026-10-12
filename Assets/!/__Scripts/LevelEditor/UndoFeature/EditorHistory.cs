using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EditorHistory : MonoBehaviour
{
    public static event Action OnUndo;
    public static EditorHistory Instance { get; private set; }

    private readonly Stack<IEditorCommand> undoStack = new();
    private readonly Stack<IEditorCommand> redoStack = new();

    private void Awake()
    {
        Instance = this;
    }

    public void Execute(IEditorCommand command)
    {
        command.Execute();

        undoStack.Push(command);

        // New action invalidates redo history.
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count == 0)
            return;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, Camera.main.transform.position);

        IEditorCommand command = undoStack.Pop();

        command.Undo();

        redoStack.Push(command);

        OnUndo?.Invoke();
    }

    public void Redo()
    {
        if (redoStack.Count == 0)
            return;

        IEditorCommand command = redoStack.Pop();

        command.Execute();

        undoStack.Push(command);
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    private void Update()
    {
        bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed ||
                        Keyboard.current.rightCtrlKey.isPressed;

        if (ctrlHeld && Keyboard.current.zKey.wasPressedThisFrame)
        {
            Undo();
            
        }
    }
}