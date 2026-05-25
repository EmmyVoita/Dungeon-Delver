

using UnityEngine;

public class ArrowWithVFX : ArrowBase
{
    [Header("VFX")]
    [SerializeField] private GameObject destroyEffect;

    protected override void Die(Goal.GoalType goalType = Goal.GoalType.Normal, bool invokeDeathEvent = true, Vector2 hitDirection = default)
    {
       
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        if(destroyEffect)
            Instantiate(destroyEffect,transform.position,rotation);


        base.Die(goalType,invokeDeathEvent,hitDirection);
    }
}

