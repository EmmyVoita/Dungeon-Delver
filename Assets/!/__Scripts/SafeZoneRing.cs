using UnityEngine;
using System;
using System.Collections;

public class SafeZoneRing : MonoBehaviour
{
    public Vector2 direction; // Up, Down, Left, Right
    public RingDistance distanceTier; // Near/Mid/Far

    public float collapseDuration = 2f;
    public AnimationCurve collapseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Transform centerTarget; // Usually player center
    private Vector3 startPos;
    private Vector3 endPos;

    public event Action<SafeZoneRing> OnRingCollapsed;

    public void Init(Transform center, Vector2 dir, RingDistance dist, float baseRadius)
    {
        centerTarget = center;
        direction = dir;
        distanceTier = dist;

        float distMultiplier = dist == RingDistance.Near ? 1f :
                               dist == RingDistance.Mid ? 1.8f : 2.8f;

        startPos = center.position + (Vector3)dir.normalized * (baseRadius * distMultiplier);
        endPos = center.position;

        transform.position = startPos;

        StartCoroutine(CollapseRoutine());
    }

    private IEnumerator CollapseRoutine()
    {
        float t = 0;
        while (t < collapseDuration)
        {
            t += Time.deltaTime;
            float lerpT = collapseCurve.Evaluate(t / collapseDuration);
            transform.position = Vector3.Lerp(startPos, endPos, lerpT);
            yield return null;
        }

        OnRingCollapsed?.Invoke(this);
        Destroy(gameObject);
    }
}

public enum RingDistance { Near, Mid, Far }
