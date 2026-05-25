using UnityEngine;
using UnityEngine.EventSystems;

public class ResetScoreMenuOption : BaseSettingOption
{
    public AudioClip resetSound;
    public AbilityUnlockManager unlockManager;
    public override void AdjustValue(int direction)
    {
        // No adjustment for this option
    }

    override public void OnPointerClick(PointerEventData eventData)
    {
        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;
            
        OnActivate();
    }

    public override void OnActivate()
    {
        AudioHelpers.PlayMyClipAtPoint(resetSound, AudioChannel.SFX, Camera.main.transform.position);
        ScoreManager.Instance.ResetHighScore();
        unlockManager.DeleteSaveFile();
    }
}
