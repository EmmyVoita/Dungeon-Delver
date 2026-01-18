using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/BPM Up Challenge")]
public class BPMUpChallenge : UpgradeBase, IActivatableUpgrade, IArrowScoreModifier
{
    public int shopRefreshesGranted = 1;
    public float bpmBonus = .15f;
    public float bonusArrowScoreModifier = 1.05f; 
    public AudioClip bonusSound;
    private bool isActive = true;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{BPM_BONUS}", bpmBonus.ToString("P0"))
            .Replace("{REFRESH_COUNT}", shopRefreshesGranted.ToString())
            .Replace("{BONUS_SCORE_MODIFIER}", (bonusArrowScoreModifier-1).ToString("P0"));
    }

    public void Activate()
    {
        GameStateManager.OnStateChanged += HandleGameState;
        isActive = true;
    }

    public void Deactivate()
    {
        GameStateManager.OnStateChanged -= HandleGameState;
    }


    private void HandleGameState(GameState previous, GameState current)
    {
        if (current == GameState.RoundActive && previous != GameState.RoundActive && isActive)
        {
            RoundManager.Instance.ApplyTempBPMBonus(bpmBonus);
            RunStateManager.Instance.GrantShopReroll(shopRefreshesGranted);
            isActive = false;
        }
    }

    public float ModifyArrowScore(float baseScore)
    {
        return baseScore *= bonusArrowScoreModifier;
    }
}