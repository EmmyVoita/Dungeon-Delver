using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public struct GameStateSetting
{
    public GameState state;
    public bool dimScreen;
    public bool allowPlayerInput;
    public bool enableDamage;
    public bool allowPause;
    public bool showScoreUI;
    public bool allowPlayerDeath;
    public bool showPlayer;
}
    

public class GameStateEffectManager : MonoBehaviour
{
    public static bool PlayerInputEnabled { get; private set; }
    public static bool PlayerDamageAllowed { get; private set; }
    public static bool PauseAllowed { get; private set; }
    public static bool ShowScoreUI { get; private set; }
    public static bool PlayerDeathAllowed { get; private set; }
    public static bool ShowPlayer { get; private set; }

    [SerializeField] private List<GameStateSetting> gameStateSettings;


    [Tooltip("What should happen to player input if the current GameState is not defined in the GameStateSettings list.")]
    [SerializeField] private GameStateSetting defaultSetting;

    private Dictionary<GameState, GameStateSetting> stateLookup;

    void Awake()
    {
        stateLookup = new Dictionary<GameState, GameStateSetting>(gameStateSettings.Count);

        foreach (var setting in gameStateSettings)
        {
            stateLookup[setting.state] = setting;
        }
    }

    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(previousState == newState) return;

        GameStateSetting setting =
            stateLookup.TryGetValue(newState, out var s)
            ? s
            : defaultSetting;

         // Decided whether we should enable or disable the player input
         // and whether they can take damage,etc.
        PlayerInputEnabled = setting.allowPlayerInput;
        PlayerDamageAllowed = setting.enableDamage;
        PauseAllowed = setting.allowPause;
        ShowScoreUI = setting.showScoreUI;
        PlayerDeathAllowed = setting.allowPlayerDeath;
        ShowPlayer = setting.showPlayer;

        // Add the new states screen dim source if we should dim the screen
        if (setting.dimScreen)
            ScreenDimmerManager.Instance.AddDimSource(newState.ToString());
            

        if(stateLookup.TryGetValue(previousState, out GameStateSetting prevSetting))
        {
            // Try and remove the previousState's dim screen source if it exists.
            if(prevSetting.dimScreen)
                ScreenDimmerManager.Instance.RemoveDimSource(previousState.ToString());
        }           
    }
}
