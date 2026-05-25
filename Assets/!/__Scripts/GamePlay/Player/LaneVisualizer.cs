using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LaneVisualizer : MonoBehaviour
{
    public static event Action<int> OnRequestHighlight;
    public static event Action OnClearHighlight;
    public static event Action<int> OnCollapseLane;
    public static event Action<int> OnRestoreLane;




    [SerializeField] private GameObject laneLinePrefab;
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color collapseColor = Color.white;

    [SerializeField] private float collapseWarningDuration = 1.0f;


    [Header("Audio")]
    [SerializeField] private SoundEffect collapseSound;

    private List<GameObject> activeLines = new List<GameObject>();


    private void OnEnable()
    {
        OnRequestHighlight += HighlightLane;
        OnClearHighlight += ClearHighlights;
        OnCollapseLane += CollapseLane;
        OnRestoreLane += RestoreLane;
    }

    private void OnDisable()
    {
        OnRequestHighlight -= HighlightLane;
        OnClearHighlight -= ClearHighlights;
        OnCollapseLane -= CollapseLane;
        OnRestoreLane -= RestoreLane;
    }

    public static void RequestHighlightLane(int lane)
    {
        OnRequestHighlight?.Invoke(lane);
    }

    public static void RequestClearLaneHighlights()
    {
        OnClearHighlight?.Invoke();
    }


    public static void RequestCollapseLane(int lane)
    {
        OnCollapseLane?.Invoke(lane);
    }

    public static void RequestRestoreLane(int lane)
    {
        OnRestoreLane?.Invoke(lane);
    }

    private void HighlightLane(int lane)
    {
        activeLines[lane].GetComponentInChildren<SpriteRenderer>().DOColor(highlightColor,0.5f);
    }

    private void ClearHighlights()
    {
        foreach(GameObject lane in activeLines)
        {
            lane.GetComponentInChildren<SpriteRenderer>().DOColor(normalColor,0.5f);
        }
    }

    public void ShowLanes(int maxLanes, float spacing, float widthScale = 1)
    {
        Clear();

        LaneState.Set(maxLanes,spacing);

        float centerOffset = (maxLanes - 1) * 0.5f;

        for (int i = 0; i < maxLanes; i++)
        {
            float y = (i - centerOffset) * spacing;

            GameObject line = Instantiate(laneLinePrefab, transform);
            line.transform.localPosition = new Vector3(0, y, 0);
            line.transform.localScale = new Vector3(widthScale,1,1);

            activeLines.Add(line);
        }
    }

    private void CollapseLane(int lane)
    {
        SpriteRenderer sr =
            activeLines[lane]
            .GetComponentInChildren<SpriteRenderer>();

        sr.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            sr.DOColor(
                collapseColor,
                collapseWarningDuration
            )
        );

        seq.AppendCallback(() =>
        {
            LaneState.CollapseLane(lane);
            AudioHelpers.PlaySoundEffect(collapseSound, activeLines[lane].transform.position);
        });

        seq.Append(
            sr.DOFade(
                .2f,
                .5f
            )
        );

       
    }

    private void RestoreLane(int lane)
    {
        SpriteRenderer sr =
            activeLines[lane]
            .GetComponentInChildren<SpriteRenderer>();

        sr.DOKill();

        sr.DOFade(1f,.5f);

        sr.DOColor(normalColor, 0.5f);

        LaneState.RestoreLane(lane);
    }

    public void Clear()
    {
        foreach (var line in activeLines)
        {
            if (line != null)
                Destroy(line);
        }

        LaneState.Clear();

        activeLines.Clear();
    }
}