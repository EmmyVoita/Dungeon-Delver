using System.Collections.Generic;
using UnityEngine;

public static class InputFocusManager
{
    private static readonly Stack<object> focusStack = new();

    public static object CurrentOwner
    {
        get
        {
            if (focusStack.Count == 0)
                return null;

            return focusStack.Peek();
        }
    }

    public static bool HasFocus(object owner)
    {
        return CurrentOwner == null || CurrentOwner == owner;
    }

    public static void Claim(object owner)
    {
        if (owner == null)
            return;

        // Don't add the same owner twice if it's already on top.
        if (CurrentOwner == owner)
            return;
        
        Debug.Log($"InputFocusManager - {owner} tries to claim ownership. Pushing to stack");

        focusStack.Push(owner);
    }

    public static void Release(object owner)
    {
        if (owner == null)
            return;

        // Normally an object should only release focus
        // if it currently owns it.
        if (CurrentOwner == owner)
        {
            Debug.Log($"InputFocusManager - {owner} tries to release ownership. Popping stack");
            focusStack.Pop();
        }
    }

    public static void Clear()
    {
        focusStack.Clear();
    }
}