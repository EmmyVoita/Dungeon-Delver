using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public enum KeyPromptValueType
{
    None,
    Rerolls
}

[System.Serializable]
public struct KeyPrompt
{
    public string name;
    public InputActionType input;
    public KeyPromptValueType valueType;
}

public class KeyPromptController : MonoBehaviour
{
    [SerializeField] private RectTransform promptContainer;
    [SerializeField] private PromptLayoutController promptPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    private readonly List<PromptLayoutController> prompts = new();
    private readonly Stack<List<KeyPrompt>> promptStack = new();

    public void HideUI()
    {
        canvasGroup.alpha = 0;
    }

    public void ShowUI()
    {
        canvasGroup.alpha = 1;
    }

    public void PushPrompts(List<KeyPrompt> keyPrompts)
    {
        //Debug.LogError($"PUSHING KEY PROMPTS OF SIZE =>{keyPrompts.Count}");
        promptStack.Push(keyPrompts);
        Refresh();
    }

    public void PopPrompts()
    {
        //Debug.LogError($"POPING PROMPTS");
        if (promptStack.Count == 0)
            return;

        promptStack.Pop();
        Refresh();
    }

    private void Refresh()
    {
        Clear();

        if (promptStack.Count == 0)
            return;

        CreateKeyPrompts(promptStack.Peek());
    }

    private void CreateKeyPrompts(List<KeyPrompt> keyPrompts)
    {
        //Debug.LogError($"CREATING KEY PROMPTS KEYPTOMPTS SIZE => {keyPrompts.Count}");
        foreach (KeyPrompt keyPrompt in keyPrompts)
        {
            PromptLayoutController prompt = Instantiate(promptPrefab, promptContainer);

            prompt.Initialize(keyPrompt);
            prompts.Add(prompt);
        }
    }

    public void Clear()
    {
        foreach (PromptLayoutController prompt in prompts)
        {
            if (prompt != null)
                Destroy(prompt.gameObject);
        }

        prompts.Clear();
    }
}