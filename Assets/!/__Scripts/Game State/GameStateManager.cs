
using System;
using UnityEngine;
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameState currentState;  
    public GameState CurrentState => currentState;

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

    public void SetState(GameState newState)
    {
        if (newState == CurrentState)
            return;

        GameState previous = CurrentState;
        currentState = newState;

        OnStateChanged?.Invoke(previous, newState);
    }

    public bool Is(GameState state) => CurrentState == state;
}
