using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Perfect Score Reward")]
public class PerfectScoreReward : UpgradeBase, IActivatableUpgrade
{
    public int bonusScore = 10000;
    public AudioClip bonusSound;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{BONUS_SCORE}", bonusScore.ToString());
    }

    public void Activate()
    {
        GameStateManager.OnStateChanged += HandleGameState;
    }

    public void Deactivate()
    {
        GameStateManager.OnStateChanged -= HandleGameState;
    }


    private void HandleGameState(GameState previous, GameState current)
    {
        if (current == GameState.RoundResultsTally && previous != GameState.RoundResultsTally && RoundManager.Instance.stats.PerfectRound)
        {
            AudioHelpers.PlayMyClipAtPoint(bonusSound, AudioChannel.SFX, Camera.main.transform.position);
            ScoreManager.Instance.AddScore(bonusScore, ScoreSource.Bonus);
        }
    }
}