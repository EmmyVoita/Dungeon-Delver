using UnityEngine;
using System.Collections;

public class BossLaneCollapseChallenge : ChallengeBase
{
    [SerializeField] private float collapseInterval = 3f;
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float collapseDuration = 2f;

    private Coroutine routine;

    void Start()
    {
        Begin();
    }

    public override void Begin(object config = null)
    {
        routine = StartCoroutine(CollapseRoutine());
    }

    private IEnumerator CollapseRoutine()
    {
        yield return null;
        
        while (true)
        {

            StartCoroutine(
                CollapseLaneSequence()
            );

            yield return new WaitForSeconds(
                collapseInterval
            );
        }
    }

    private IEnumerator CollapseLaneSequence()
    {
        int lane;

        // Avoid already-collapsed lanes
        do
        {
            lane = Random.Range(
                0,
                LaneState.MaxLanes
            );

        } while (
            LaneState.IsLaneCollapsed(lane)
        );


        // collapse
        LaneVisualizer.RequestCollapseLane(
            lane
        );

        yield return new WaitForSeconds(
            collapseDuration
        );

        // restore
        LaneVisualizer.RequestRestoreLane(
            lane
        );
    }

    protected override void CleanUp()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        StopAllCoroutines();
    }

    public override void End()
    {
        CleanUp();
        Destroy(gameObject);
    }
}