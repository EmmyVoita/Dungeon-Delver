using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class InputControlsKeyPromptView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionType key = InputActionType.ViewUpgrades;
    [SerializeField] private List<PromptGameObject> promptObjects;

    private readonly Dictionary<KeyPromptType, GameObject> keyPromptLookup = new();


    private void Awake()
    {
        BuildLookups();
        SetAllPromptsActive(false);
    }

    private void Update()
    {
        if(InputBindingManager.Instance.GetKeyDown(key))
        {
            SetAllPromptsActive(true);
        }
    }



    private void BuildLookups()
    {
        keyPromptLookup.Clear();

        foreach (var promptObj in promptObjects)
        {
            if (promptObj.obj == null)
                continue;

            keyPromptLookup[promptObj.keyPrompt] = promptObj.obj;
        }
    }


    private void SetAllPromptsActive(bool active)
    {
        foreach (var pair in keyPromptLookup)
        {
            pair.Value.SetActive(active);
        }
    }
}