using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CollapsingRing : MonoBehaviour
{
    [Header("Ring Generation")]
    public GameObject segmentPrefab;
    public int segmentCount = 24;
    public float startRadius = 3f;
    public float endRadius = 0.4f;
    public float collapseDuration = 2f;
    public AnimationCurve shrinkCurve;


    [Header("Damage Settings")]
    public float checkInterval = 0.05f;
    public float hitRadiusMultiplier = 1.0f;

    [Header("Fade Settings")]
    public float fadeOutTime = 0.3f;
    

    private List<GameObject> segments = new List<GameObject>();
    private float currentRadius;
    private Action onCompleteCallback;

    public void Init(Transform center, Action onComplete = null)
    {
        onCompleteCallback = onComplete;
        
        SpawnRing();
        currentRadius = startRadius;

        StartCoroutine(CollapseRoutine());
    }

    private void SpawnRing()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (float)i / segmentCount * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * startRadius;

            GameObject segment = Instantiate(segmentPrefab, transform);
            segment.transform.localPosition = pos;
            segment.transform.up = pos.normalized;
            segments.Add(segment);
        }
    }

    IEnumerator CollapseRoutine()
    {
        float t = 0f;

        while (t < collapseDuration)
        {
            t += Time.deltaTime;
            float pct = shrinkCurve.Evaluate(t / collapseDuration);

            currentRadius = Mathf.Lerp(startRadius, endRadius, pct);

            foreach (var s in segments)
            {
                if (s == null) continue;

                Vector3 dir = s.transform.localPosition.normalized;
                s.transform.localPosition = dir * currentRadius;
            }

            yield return null;
        }

        onCompleteCallback?.Invoke();

        Finish();
    }

    void Finish()
    {
        foreach (var s in segments)
        {
            if (s == null) continue;

            s.GetComponent<BasicProjectile>().DestroyProjectile();
        }

        Destroy(gameObject, fadeOutTime + 0.1f);
    }

    IEnumerator FadeOut(SpriteRenderer sr)
    {
        float t = 0f;
        Color startColor = sr.color;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1 - (t / fadeOutTime));
            yield return null;
        }

        sr.color = new Color(startColor.r, startColor.g, startColor.b, 0);
    }
}
