using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;



public class KeyPromptViewGameState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private List<PromptGameObject> promptObjects;
    [SerializeField] private List<KeyPromptVisibleState> visibleStates;

    private readonly Dictionary<KeyPromptType, GameObject> keyPromptLookup = new();
    private readonly Dictionary<GameState, List<KeyPromptType>> promptVisibilityLookup = new();


    private void Awake()
    {
        BuildLookups();
        SetAllPromptsActive(false);
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Update()
    {
        if(InputFocusManager.CurrentOwner != null)
        {
            promptCanvasGroup.alpha = 0;
        }
        else
        {
            promptCanvasGroup.alpha = 1;
        }
    }

    private void BuildLookups()
    {
        keyPromptLookup.Clear();
        promptVisibilityLookup.Clear();

        foreach (var promptObj in promptObjects)
        {
            if (promptObj.obj == null)
                continue;

            keyPromptLookup[promptObj.keyPrompt] = promptObj.obj;
        }

        foreach (var state in visibleStates)
        {
            if (state.keyPrompts == null)
                continue;

            promptVisibilityLookup[state.targetState] = state.keyPrompts;
        }
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        SetAllPromptsActive(false);

        bool hasVisiblePrompts = promptVisibilityLookup.TryGetValue(
            newState,
            out List<KeyPromptType> promptsToShow
        );

        if(!hasVisiblePrompts) return;


        foreach (KeyPromptType keyPromptType in promptsToShow)
        {
            if (keyPromptLookup.TryGetValue(keyPromptType, out GameObject promptObj))
            {
                promptObj.SetActive(true);
            }
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