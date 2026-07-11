
using System;
using UnityEngine;
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameState currentState;  
    [SerializeField] private GameState previousState;
    public GameState CurrentState => currentState;
    public GameState PreviousState => previousState;

    public static GameState LevelStartState => GameState.RoundActive;
    public static GameState LevelEndState => GameState.RoundResultsTally;

    public static event Action<GameState, GameState> OnStateChanged;

    private void Awake()
    {
        // If an instance already exists and it's not us → destroy
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Assign instance
        Instance = this;

        // Persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initialize to a default state if needed
        //SetState(GameState.None);
    }

    public void SetState(GameState newState)
    {
        if (newState == CurrentState)
            return;

        previousState = CurrentState;
        currentState = newState;

        Debug.Log($"Set state from {previousState} => {newState}!");

        OnStateChanged?.Invoke(previousState, newState);
    }

    public void SetStateForceUpdate(GameState newState)
    {
        previousState = CurrentState;
        currentState = newState;

        OnStateChanged?.Invoke(previousState, newState);
    }

    public void RequestStateChange(GameState newState)
    {
        TransitionManager.Instance.PlayTransition(CurrentState, newState);
    }

    public bool Is(GameState state) => CurrentState == state;
}
