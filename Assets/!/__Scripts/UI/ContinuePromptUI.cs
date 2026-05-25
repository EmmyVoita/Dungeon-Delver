using TMPro;
using UnityEngine;


public class ContinuePromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIFadeGroup continuePrompt;
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("State Management")]
    public GameState showState = GameState.WorldMapView;
    public GameState hideState = GameState.UpgradeSelection;


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
            textComponent.text = $"[<color=#FFD700>{InputBindingManager.Instance.GetKeyName(InputActionType.Confirm)}</color>] to continue";
            continuePrompt?.Show();
        }
        else if(newState == hideState)
        {
            continuePrompt?.Hide();
            return;
        }
    }
}
