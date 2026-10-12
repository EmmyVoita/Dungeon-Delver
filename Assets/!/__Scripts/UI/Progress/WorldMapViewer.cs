using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapViewer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private WorldNodeView nodePrefab;

    

    
    [Header("Route Marker")]
    [SerializeField] private float routeMarkerYOffset;
    [SerializeField] private WorldMapRouteMarker routeMarkerPrefab;
    [SerializeField] private RouteOptionDatabase routeOptionDatabase;
    [SerializeField] private float routeMarkerDelay = 0.08f;

    [Header("Layout")]
    [SerializeField] private RectTransform scrollRoot;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform pathRect;

    [SerializeField] private float spacing = 180f;
    [SerializeField] private float startX = 0f;
    [SerializeField] private float nodeY = 0f;

    [Header("Scrolling")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private float focusX = 0f;
    [SerializeField] private float scrollDuration = 0.45f;
    [SerializeField] private Ease scrollEase = Ease.OutCubic;

    [Header("Animation")]
    [SerializeField] private float firstNodeDelay = 0.15f;
    [SerializeField] private float nodeDisplayDelay = 0.1f;

    [Header("Audio")]
    [SerializeField] private SoundEffect buildSound;

    private readonly List<WorldNodeView> nodeViews = new();

    private Coroutine buildRoutine;
    private Tween scrollTween;
     private WorldMapRouteMarker _currentRouteMarker;

    public Vector2 CurrentNodePosition => (nodeViews[CurrentIndex].transform as RectTransform).anchoredPosition;
    private int CurrentIndex => RoundManager.Instance != null ? RoundManager.Instance.TotalCurrentLevelIndex: 0;

    private int GetRouteMarkerIndex(int levelIndex)
    {
        if (BetweenRoundFlowManager.Instance.RewardTiming == RouteRewardTiming.BeforeNextLevel)
        {
            levelIndex--;
        }

        return levelIndex;
    }

    private StageLevelIndex GetRouteHistoryIndexForNode(int nodeIndex)
    {
        int levelIndex = nodeIndex;

        if (BetweenRoundFlowManager.Instance.RewardTiming ==
            RouteRewardTiming.BeforeNextLevel)
        {
            levelIndex++;
        }

        return new StageLevelIndex
        {
            stageIndex = RoundManager.Instance.CurrentStageIndex,
            levelIndex = levelIndex
        };
    }



    public void Build(Action onComplete)
    {
        if (buildRoutine != null)
            StopCoroutine(buildRoutine);

        Clear();

        buildRoutine = StartCoroutine(
            BuildSequence(onComplete)
        );
    }

    private IEnumerator BuildSequence(Action onComplete)
    {
        if (RoundManager.Instance.StageCount == 0)
        {
            buildRoutine = null;
            yield break;
        }

        SetupPath();

        int index = 0;

        for (int i = 0; i < RoundManager.Instance.StageCount; i++)
        {
            StageDataObject stageDataObject = RoundManager.Instance.StageObjects[i];
            
            for(int j = 0; j < stageDataObject.levelsToPlay; j++)
            {
                yield return CreateNodeSequence(index);
                index++;
            }
            
            yield return CreateNodeSequence(index, true);
            index++;
        }

        // Start with the current part of the map visible.
        SnapToNode(Mathf.Max(CurrentIndex - 1, 0));

        onComplete?.Invoke();

        buildRoutine = null;
    }

    private IEnumerator CreateNodeSequence(int index, bool isBoss = false)
    {
        Vector2 position =GetNodePosition(index);

        WorldNodeView node =SpawnNode(position, index, index < CurrentIndex, isBoss);

        if (index == 0)
            yield return new WaitForSecondsRealtime(firstNodeDelay);
        else
            yield return new WaitForSecondsRealtime(nodeDisplayDelay);

        AudioHelpers.PlaySoundEffect(buildSound,transform.position);

        StageLevelIndex historyIndex = GetRouteHistoryIndexForNode(index);

        if (RunStateManager.Instance.TryGetRouteHistory(historyIndex,out WorldMapRouteType route))
        {
            yield return new WaitForSecondsRealtime(routeMarkerDelay);    
            SpawnRouteMarker(node,route);
        }
    }

    private Vector2 GetNodePosition(int index)
    {
        return new Vector2(startX + index * spacing,nodeY);
    }

    private void SetupPath()
    {
        if (pathRect == null || RoundManager.Instance.StageCount == 0)
            return;

        float firstX = GetNodePosition(0).x;
        float lastX = GetNodePosition(RoundManager.Instance.StageCount - 1).x;

        float width = lastX - firstX;

        pathRect.anchorMin = new Vector2(0.5f, 0.5f);
        pathRect.anchorMax = new Vector2(0.5f, 0.5f);
        pathRect.pivot = new Vector2(0f, 0.5f);

        pathRect.anchoredPosition = new Vector2(firstX, nodeY);

        pathRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            width
        );
    }

    private WorldNodeView SpawnNode(Vector2 anchoredPosition, int index, bool completed, bool isBoss)
    {
        WorldNodeView view = Instantiate(nodePrefab, nodeContainer, false);

        RectTransform nodeRect = view.transform as RectTransform;

        if (nodeRect != null)
        {
            nodeRect.anchoredPosition = anchoredPosition;
            nodeRect.localRotation = Quaternion.identity;
            nodeRect.localScale = Vector3.one;
        }

        view.Initialize(index, isBoss,completed);

        nodeViews.Add(view);

        return view;
    }

    private WorldMapRouteOption GetRouteOption(WorldMapRouteType type)
    {
        return routeOptionDatabase.routeOptions.Find(option => option.type == type);
    }

    private WorldMapRouteMarker SpawnRouteMarker(WorldNodeView node,WorldMapRouteType routeType)
    {
        if (node == null || routeMarkerPrefab == null)
            return null;

        WorldMapRouteOption option = GetRouteOption(routeType);

        if (option == null)
            return null;

        RectTransform nodeRect = node.transform as RectTransform;

        WorldMapRouteMarker marker = Instantiate(routeMarkerPrefab,nodeRect,false);

        RectTransform markerRect =marker.transform as RectTransform;

        markerRect.anchorMin =new Vector2(0.5f, 0.5f);

        markerRect.anchorMax =new Vector2(0.5f, 0.5f);

        markerRect.pivot =new Vector2(0.5f, 0.5f);

        markerRect.anchoredPosition =new Vector2(0f,routeMarkerYOffset);

        marker.SetIcon(option.icon);

        return marker;
    }

    public Tween ScrollToNode(int index)
    {
        if (index < 0 || index >= nodeViews.Count)
            return null;

        RectTransform nodeRect = nodeViews[index].transform as RectTransform;

        if (nodeRect == null)
            return null;

        // Node's local X inside the scrolling content.
        float nodeX = nodeRect.anchoredPosition.x;

        // Move the entire map so this node ends up at focusX.
        float targetRootX = focusX - nodeX;

        scrollTween?.Kill();

        scrollTween = scrollRoot
            .DOAnchorPosX(targetRootX, scrollDuration)
            .SetEase(scrollEase)
            .SetUpdate(true);

        return scrollTween;
    }

    public void SnapToNode(int index)
    {
        if (index < 0 || index >= nodeViews.Count)
            return;

        RectTransform nodeRect = nodeViews[index].transform as RectTransform;

        if (nodeRect == null)
            return;

        float targetRootX = focusX - nodeRect.anchoredPosition.x;

        Vector2 pos = scrollRoot.anchoredPosition;
        pos.x = targetRootX;

        scrollRoot.anchoredPosition = pos;
    }

    public void Clear()
    {
        if (buildRoutine != null)
        {
            StopCoroutine(buildRoutine);
            buildRoutine = null;
        }

        scrollTween?.Kill();

        if (nodeContainer != null)
            DestroyChildren(nodeContainer);

        nodeViews.Clear();
    }

    private static void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public WorldNodeView GetNode(int index)
    {
        if (index < 0 || index >= nodeViews.Count)
            return null;

        return nodeViews[index];
    }

    // Current Marker Creation
    // ----------------------------------------------------------------------------------

    public void CreateCurrentRoutePreview()
    {
        int markerIndex = GetRouteMarkerIndex(CurrentIndex);

        WorldNodeView node = GetNode(markerIndex);

        if (node == null)
            return;

        if (_currentRouteMarker != null)
            Destroy(_currentRouteMarker.gameObject);

        _currentRouteMarker =
            Instantiate(routeMarkerPrefab, node.transform, false);

        RectTransform rect = _currentRouteMarker.transform as RectTransform;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = new Vector2(0f, routeMarkerYOffset);
    }

    public void SetCurrentRoutePreview(Sprite icon)
    {
        if (_currentRouteMarker == null)
            return;

        _currentRouteMarker.SetIcon(icon);
    }

    public void RemoveCurrentRoutePreview()
    {
        if (_currentRouteMarker == null)
            return;

        Destroy(_currentRouteMarker.gameObject);
        _currentRouteMarker = null;
    }
    
}