using UnityEngine;

public class UnlockAllMenuOption : BaseSettingOption
{
    public AudioClip resetSound;
    public override void AdjustValue(int direction)
    {
        // No adjustment for this option
    }

    public override void OnActivate()
    {
        AudioHelpers.PlayMyClipAtPoint(resetSound, AudioChannel.SFX, Camera.main.transform.position);
        ScoreManager.Instance.SetHighScore(9999999);
    }
}
