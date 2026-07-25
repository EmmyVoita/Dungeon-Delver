using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ContinuePromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIFadeGroup continuePrompt;
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("State Management")]
    [SerializeField] private GameState showState = GameState.WorldMapView;
    [SerializeField] private List<GameState> hideStates;


    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }


    void Start()
    {
        continuePrompt?.Hide(true);
    }

    void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == showState)
        {
            textComponent.text = $"[<color=#FFD700>{InputBindingManager.Instance.GetBoundKey(InputActionType.Confirm)}</color>] to continue";
            continuePrompt?.Show();
        }
        else if(hideStates.Contains(newState))
        {
            continuePrompt?.Hide();
            return;
        }
    }
}
