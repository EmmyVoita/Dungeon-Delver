using System.Collections;
using UnityEngine;

public class BigGoalAbility : AbilityBase, ICritWindowModifier, IGoalSizeModifier
{
    [Header("Ability Settings")]
    public float goalScaleModifier = 1.5f;
    public float duration = 5f;
    public SoundEffect deactivateSound;
    public float critWindowMultiplier;
    private GoalFormController _formController;


    public override void Activate(Quaternion rotation)
    {
        //UpgradeManager.Instance.AddTemporaryModifier(this);

        /*
        Player.Instance.goal
            .GetComponentInChildren<Goal>()
            .ModifyScale(goalScaleModifier, duration);
        */
        _formController = Player.Instance.goal.GetComponent<GoalFormController>();

        _formController.Activate();

        StartCoroutine(DurationRoutine());
    }

    private IEnumerator DurationRoutine()
    {
        AudioHelpers.PlaySoundEffect(activateSound, Player.Instance.transform.position);

        yield return new WaitForSeconds(duration);

        AudioHelpers.PlaySoundEffect(deactivateSound, Player.Instance.transform.position);

        _formController.Deactivate();

        //UpgradeManager.Instance.RemoveTemporaryModifier(this);
    }

    public float ModifyCritWindow(float current)
        => current * critWindowMultiplier;

    public float ModifyGoalSize(float current)
        => current * goalScaleModifier;
}
