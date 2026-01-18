using UnityEngine;

public class ResetScoreMenuOption : BaseSettingOption
{
    public AudioClip resetSound;
    public override void AdjustValue(int direction)
    {
        // No adjustment for this option
    }

    public override void OnActivate()
    {
        AudioHelpers.PlayMyClipAtPoint(resetSound, AudioChannel.SFX, Camera.main.transform.position);
        ScoreManager.Instance.ResetHighScore();
    }
}
