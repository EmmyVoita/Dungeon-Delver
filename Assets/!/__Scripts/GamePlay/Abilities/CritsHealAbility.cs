using System.Collections;
using UnityEngine;

public class CritsHealAbility : AbilityBase
{
    [Header("Ability Settings")]
    public int healAmount = 1;
    public int startRequirement = 3;
    public int requirementStep = 1;
    public float duration = 5f;
    private int _critsCounter = 0;
    private int _currentRequiredCrits;
    public Material fillBarMaterial;
    public SpriteRenderer backSprite;


    private void Awake()
    {
        fillBarMaterial.SetFloat("_FillAmount", 0f);
        backSprite.color = Color.clear;
    }

    private void HandleArrowResolved(ArrowResolvedData data)
    {
        switch (data.goalType)
        {
            case Goal.GoalType.Critical:
                _critsCounter++;
                CheckForHeal();
                break;

            case Goal.GoalType.Normal:
                break;

            case Goal.GoalType.Miss:
                break;
        }
    }

    private void CheckForHeal()
    {
        if(_critsCounter > _currentRequiredCrits)
        {
            Player.Instance.HealPlayer(healAmount);
            _critsCounter = 0;
            _currentRequiredCrits += requirementStep;
        }

        fillBarMaterial.SetFloat("_FillAmount", (float)_critsCounter / (float) _currentRequiredCrits);
    }

    public override void Activate(Quaternion rotation)
    {
        //duration *= durationModifier;
        
        _critsCounter = 0;
        _currentRequiredCrits = startRequirement;
        backSprite.color = Color.white;
        fillBarMaterial.SetFloat("_FillAmount", 0f);
        StartCoroutine(DurationRoutine());
    }

    private IEnumerator DurationRoutine()
    {
        ArrowBase.OnArrowResolved += HandleArrowResolved;

        AudioHelpers.PlaySoundEffect(Data.activationSound, Player.Instance.transform.position);

        yield return new WaitForSeconds(duration);

        AudioHelpers.PlaySoundEffect(Data.deactivateSound, Player.Instance.transform.position);
        
        ArrowBase.OnArrowResolved -= HandleArrowResolved;

        fillBarMaterial.SetFloat("_FillAmount", 0f);
        backSprite.color = Color.clear;
    }
}
