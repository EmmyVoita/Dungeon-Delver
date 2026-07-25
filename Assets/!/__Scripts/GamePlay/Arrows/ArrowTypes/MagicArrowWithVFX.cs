

using UnityEngine;

public class MagicArrowWithVFX : ArrowBase
{
    [Header("VFX")]
    [SerializeField] private GameObject destroyEffect;

    [Header("Additional Flag Colors")]
    [SerializeField] protected string colorCpropertyName = "_ColorC";
    [SerializeField] protected string colorDpropertyName = "_ColorD";
    [ColorUsage(true, true)][SerializeField] protected Color goldenColorC;
    [ColorUsage(true, true)][SerializeField] protected Color goldenColorD;


    protected override void Die(Goal.GoalType goalType = Goal.GoalType.Normal, bool invokeDeathEvent = true, Vector2 hitDirection = default)
    {
       
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        if(destroyEffect)
            Instantiate(destroyEffect,transform.position,rotation);


        base.Die(goalType,invokeDeathEvent,hitDirection);
    }

    public override void SetGolden()
    {
        AddStatus(ArrowStatus.Golden);
        
        if(!runTimeMaterial) 
            return;

        runTimeMaterial.SetColor(colorApropertyName, goldenColorA);
        runTimeMaterial.SetColor(colorBpropertyName, goldenColorB);
        runTimeMaterial.SetColor(colorCpropertyName, goldenColorC);
        runTimeMaterial.SetColor(colorDpropertyName, goldenColorD);
    }
}

