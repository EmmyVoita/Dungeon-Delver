using UnityEngine;

public class IgnoreMissAbility : AbilityBase
{
    [Header("Ability Settings")]
    public GameObject visualObject;
    public float duration = 2.0f;


    public override void Activate(Quaternion rotation)
    {
        ComboManager.Instance.PreventNextComboBreak(duration);
        AudioHelpers.PlaySoundEffect(activateSound,Player.Instance.transform.position);
    }
}
