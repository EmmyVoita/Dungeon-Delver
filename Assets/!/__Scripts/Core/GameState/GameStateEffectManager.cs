using System.Collections.Generic;
using UnityEngine;


public class GameStateEffectManager : MonoBehaviour
{
    public static bool PlayerInputEnabled { get; private set; }
    public static bool PlayerDamageAllowed { get; private set; }
    public static bool PauseAllowed { get; private set; }
    public static bool ShowScoreUI { get; private set; }
    public static bool PlayerDeathAllowed { get; private set; }
    public static bool ShowPlayer { get; private set; }
    public static bool AllowMouseInput { get; private set; }


    [SerializeField] private GameStateDatabase database;


    [Tooltip("What should happen to player input if the current GameState is not defined in the GameStateSettings list.")]
    [SerializeField] private GameStateSettingData defaultSettingData;

    private Dictionary<GameState, GameStateSettingData> stateLookup;

    void Awake()
    {
        stateLookup = new Dictionary<GameState, GameStateSettingData>();

        foreach (var setting in database.data)
        {
            if(setting == null)
            {
                Debug.LogError("Null setting in database");
                continue;
            }

            stateLookup.Add(setting.state, setting);

            Debug.Log($"Added: {setting.state}");
        }

        Debug.Log($"Dictionary Count: {stateLookup.Count}");
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

        GameStateSettingData setting =
            stateLookup.TryGetValue(newState, out var s)
            ? s
            : defaultSettingData;

        PlayerInputEnabled = setting.allowPlayerInput;
        PlayerDamageAllowed = setting.enableDamage;
        PauseAllowed = setting.allowPause;
        ShowScoreUI = setting.showScoreUI;
        PlayerDeathAllowed = setting.allowPlayerDeath;
        ShowPlayer = setting.showPlayer;
        AllowMouseInput = setting.allowMouseInput;

   

        // Remove previous dim first
        GameStateSettingData prevSetting =
            stateLookup.TryGetValue(previousState, out var prev)
            ? prev
            : defaultSettingData;

        Debug.Log(
            $"Previous Setting => \n{prevSetting}\n\n" +
            $"New Setting => \n{setting}\n"
        );

        if(prevSetting.dimScreen)
            ScreenDimmerManager.Instance.RemoveDimSource(previousState.ToString());

        // Add new dim
        if(setting.dimScreen)
            ScreenDimmerManager.Instance.AddDimSource(newState.ToString());
    }
}
