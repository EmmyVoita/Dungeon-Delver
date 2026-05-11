using UnityEngine;

public class SlowOnCritListener
{
    [SerializeField] private TimeSlowImpulseData impulseData;
    private float slowMult;
    private float inTime;
    private float holdTime;
    private float outTime;

    public SlowOnCritListener(
        float slowMult,
        float inTime,
        float holdTime,
        float outTime)
    {
        this.slowMult = slowMult;
        this.inTime = inTime;
        this.holdTime = holdTime;
        this.outTime = outTime;

        ArrowBase.OnArrowResolved += HandleArrowResolved;
    }

    private void HandleArrowResolved(ArrowResolvedData data)
    {
        if (data.goalType != Goal.GoalType.Critical)
            return;
        
        /*
        TimeManager.Instance.PlayImpulseSlow(
            impulseData
        );
        */
    }

    public void Cleanup()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
    }
}
