using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UpgradeIconUI : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public Image fillImage;
    public Color fillBackgroundColor = Color.white;
    public IdleHover idleHover;

    [Header("Settings")]
    public float activeScale = 1.1f;

    private string upgradeId;
    private Tween glowTween;
    private IconFeedbackStyle feedbackStyle;
    private UpgradeBase upgradeData;


    // ----------------------------------------------------
    // INITIALIZATION
    // ----------------------------------------------------

    public void Initialize(UpgradeBase upgrade)
    {
        upgradeId = upgrade.upgradeId;
        upgradeData = upgrade;
        feedbackStyle = upgrade.feedbackStyle;

        iconImage.sprite = upgrade.baseIcon;

        // Idle hover defaults OFF
        idleHover.enableHover = false;
        idleHover.enableSway = false;
        idleHover.enableScoreJump = false;
        idleHover.ApplyState();

        // Recharge fill defaults
        fillImage.fillAmount = 0f;
        fillImage.sprite = upgrade.baseIcon;
        fillImage.color = Color.white;

        UpgradeManager.OnUpgradeStateChanged += HandleStateChanged;
        UpgradeManager.OnUpgradeRechargeProgress += HandleRechargeProgress;
    }

    private void OnDestroy()
    {
        UpgradeManager.OnUpgradeStateChanged -= HandleStateChanged;
        UpgradeManager.OnUpgradeRechargeProgress -= HandleRechargeProgress;
    }

    // ----------------------------------------------------
    // EVENT HANDLERS
    // ----------------------------------------------------

    private void HandleStateChanged(string id, bool active)
    {
        if (id != upgradeId)
            return;

        if (active)
            ActivateVisuals();
        else
            DeactivateVisuals();
    }

    private void HandleRechargeProgress(string id, float deltaAmount)
    {
        if (id != upgradeId)
            return;

        

        fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount + deltaAmount);

        Debug.Log($"UpgradeIconUI: Recharge progress for {id} is {deltaAmount}, new fillAmount={fillImage.fillAmount}, fill image color={fillImage.color}   ");

        // Fully recharged → jump impulse
        if (fillImage.fillAmount >= 1f)
        {
            fillImage.color = Color.clear;
            PlayJumpImpulse();
        }

        // Just used → shake
        if (fillImage.fillAmount == 0f)
        {
            idleHover.ShakeJumpTarget();
        }
    }

    // ----------------------------------------------------
    // PERSISTENT STATE VISUALS
    // ----------------------------------------------------

    private void ActivateVisuals()
    {
        switch (feedbackStyle)
        {
            case IconFeedbackStyle.None:
                break;

            case IconFeedbackStyle.SwayAndHover:
                idleHover.enableHover = true;
                idleHover.enableSway = true;
                idleHover.UpdateState();

                glowTween?.Kill();
                glowTween = iconImage.transform
                    .DOScale(activeScale, 0.6f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);

                iconImage.sprite = upgradeData.activeIcon;
                break;

            case IconFeedbackStyle.Jump:
                // Jump-only upgrades do NOT stay active
                PlayJumpImpulse();
                break;

            case IconFeedbackStyle.ActiveInactiveColor:
                iconImage.color = fillBackgroundColor;
                break;
        }
    }

    private void DeactivateVisuals()
    {
        Destroy(gameObject);
        switch (feedbackStyle)
        {
            case IconFeedbackStyle.None:
                break;

            case IconFeedbackStyle.SwayAndHover:
                idleHover.enableHover = false;
                idleHover.enableSway = false;
                idleHover.ApplyState();

                glowTween?.Kill();
                iconImage.transform.localScale = Vector3.one;
                iconImage.sprite = upgradeData.baseIcon;
                break;

            case IconFeedbackStyle.Jump:
                // Jump visuals reset themselves — nothing to do
                iconImage.sprite = upgradeData.baseIcon;
                break;

            case IconFeedbackStyle.ActiveInactiveColor:
                iconImage.color = Color.white;
                break;
        }
    }

    // ----------------------------------------------------
    // ONE-SHOT JUMP IMPULSE (IMPORTANT)
    // ----------------------------------------------------

    private void PlayJumpImpulse()
    {
        idleHover.enableScoreJump = true;

        idleHover.JumpImmediate(() =>
        {
            // Restore base state AFTER jump finishes
            idleHover.enableScoreJump = false;
            iconImage.sprite = upgradeData.baseIcon;
            fillImage.color = Color.white;
        });
    }
}
