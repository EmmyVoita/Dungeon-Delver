using System.Collections;
using UnityEngine;

public class BigGoalAbility : AbilityBase, ICritWindowModifier, IGoalSizeModifier
{
    [Header("Ability Settings")]
    public float goalScaleModifier = 1.5f;
    public float critWindowMultiplier;
    private GoalFormController _formController;


    public override void Activate(Quaternion rotation)
    {
        _formController = Player.Instance.goal.GetComponent<GoalFormController>();

        _formController.Activate();

        StartCoroutine(DurationRoutine());
    }

    private IEnumerator DurationRoutine()
    {
        AudioHelpers.PlaySoundEffect(Data.activationSound, Player.Instance.transform.position);

        yield return new WaitForSeconds(GetModifiedDuration());

        AudioHelpers.PlaySoundEffect(Data.deactivateSound, Player.Instance.transform.position);

        _formController.Deactivate();
    }

    public float ModifyCritWindow(float current)
        => current * critWindowMultiplier;

    public float ModifyGoalSize(float current)
        => current * goalScaleModifier;
}
