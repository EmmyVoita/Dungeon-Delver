using UnityEngine;

[CreateAssetMenu(menuName = "Database/Game State Setting")]
public class GameStateSettingData : ScriptableObject
{
    public GameState state;
    public bool dimScreen;
    public bool allowPlayerInput;
    public bool enableDamage;
    public bool allowPause;
    public bool showScoreUI;
    public bool allowPlayerDeath;
    public bool showPlayer;
    public bool allowMouseInput;

    public override string ToString()
    {
        return
            $"State: {state}\n" +
            $"Dim Screen: {dimScreen}\n" +
            $"Allow Player Input: {allowPlayerInput}\n" +
            $"Enable Damage: {enableDamage}\n" +
            $"Allow Pause: {allowPause}\n" +
            $"Show Score UI: {showScoreUI}\n" +
            $"Allow Player Death: {allowPlayerDeath}\n" +
            $"Show Player: {showPlayer}\n" +
            $"Allow Mouse Input: {allowMouseInput}";
    }
}