using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldMapData mapData;
    [SerializeField] private UIPanelNudge panelNudge;

    [Header("Prefabs")]
    [SerializeField] private WorldNodeView nodePrefab;
    [SerializeField] private WorldPathView pathPrefab;

    [Header("Containers")]
    [SerializeField] private RectTransform pathContainer;
    [SerializeField] private RectTransform nodeContainer;

    [Header("Layout")]
    [SerializeField] private float spacing = 120f;
    [SerializeField] private float verticalOffset = 0f;

    [Header("Animation")]
    [SerializeField] private float firstNodeDelay = 0.15f;
    [SerializeField] private float lineBuildDuration = 0.25f;
    [SerializeField] private float nodeDisplayDelay = 0.1f;

    [Header("Audio")]
    [SerializeField] private SoundEffect buildSound;

    private readonly List<WorldNodeView>
        nodeViews = new();

    private Coroutine buildRoutine;

    private int CurrentIndex =>
        RoundManager.Instance != null
            ? RoundManager.Instance.TotalCurrentLevelIndex
            : 0;

    public void Build(Action onComplete)
    {
        if (buildRoutine != null)
            StopCoroutine(buildRoutine);

        Clear();

        buildRoutine =
            StartCoroutine(
                BuildSequence(onComplete)
            );
    }

    private IEnumerator BuildSequence(
        Action onComplete)
    {
        if (mapData == null ||
            mapData.nodes == null ||
            mapData.nodes.Count == 0)
        {
            buildRoutine = null;
            yield break;
        }

        WorldNodeData firstNode = mapData.nodes[0];

        Vector2 firstPosition = WorldToUIPosition(firstNode.gridPosition, nodeContainer) + new Vector2(0,verticalOffset);

        SpawnNode(firstNode,firstPosition ,CurrentIndex > 0);

        yield return new WaitForSecondsRealtime(firstNodeDelay);

        for (int i = 1; i < mapData.nodes.Count; i++)
        {
            WorldNodeData previousNode = mapData.nodes[i - 1];
            WorldNodeData currentNode = mapData.nodes[i];

            Vector3 fromWorldPosition = previousNode.gridPosition;
            Vector3 toWorldPosition = currentNode.gridPosition;

            Vector2 fromPosition = WorldToUIPosition(fromWorldPosition,pathContainer) + new Vector2(0,verticalOffset);
            Vector2 toPosition = WorldToUIPosition(toWorldPosition,pathContainer) + new Vector2(0,verticalOffset);

            RectTransform pathParent = pathContainer != null ? pathContainer : transform as RectTransform;

            WorldPathView path = Instantiate(pathPrefab,pathParent,false);

            Vector2 direction = toPosition - fromPosition;


            bool shouldNudge = !Mathf.Approximately(direction.x,0f);

            if (shouldNudge && panelNudge != null)
            {
                if (direction.x > 0f)
                    panelNudge.NudgeRight();
                else
                    panelNudge.NudgeLeft();
            }


            path.Setup(fromPosition,toPosition);

            yield return path.Build(lineBuildDuration);

            SpawnNode(currentNode,toPosition, i < CurrentIndex);

            AudioHelpers.PlaySoundEffect(buildSound,transform.position);

            yield return new WaitForSecondsRealtime(nodeDisplayDelay);
        }

        onComplete?.Invoke();

        buildRoutine = null;
    }

    private Vector2 WorldToUIPosition(Vector3 worldPosition, RectTransform targetContainer)
    {
        if (targetContainer == null)
            return Vector2.zero;

        Camera worldCamera = Camera.main;

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                worldCamera,
                worldPosition
            );

        Canvas canvas = targetContainer.GetComponentInParent<Canvas>();

        Camera uiCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetContainer,
            screenPosition,
            uiCamera,
            out Vector2 localPosition
        );

        return localPosition;
    }

    private WorldNodeView SpawnNode(WorldNodeData nodeData,Vector2 anchoredPosition,bool completed)
    {
        RectTransform parent =
            nodeContainer != null
                ? nodeContainer
                : transform as RectTransform;

        WorldNodeView view = Instantiate(nodePrefab,parent,false);

        RectTransform nodeRect = view.transform as RectTransform;

        if (nodeRect != null)
        {
            nodeRect.anchoredPosition = anchoredPosition;

            nodeRect.localRotation = Quaternion.identity;

            nodeRect.localScale = Vector3.one;
        }

        view.Initialize(nodeData.index,nodeData.isBoss,completed);

        nodeViews.Add(view);

        return view;
    }

    private Vector2 GridToAnchoredPosition(
        Vector2 gridPosition)
    {
        return new Vector2(
            gridPosition.x * spacing,
            gridPosition.y * spacing
        );
    }

    public void Clear()
    {
        if (buildRoutine != null)
        {
            StopCoroutine(buildRoutine);
            buildRoutine = null;
        }

        if (pathContainer != null)
            DestroyChildren(pathContainer);

        if (nodeContainer != null)
            DestroyChildren(nodeContainer);

        if (pathContainer == null &&
            nodeContainer == null)
        {
            DestroyChildren(transform);
        }

        nodeViews.Clear();
    }

    private static void DestroyChildren(
        Transform parent)
    {
        for (int i = parent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                parent.GetChild(i).gameObject
            );
        }
    }

    public WorldNodeView GetNode(int index)
    {
        if (index < 0 ||
            index >= nodeViews.Count)
        {
            return null;
        }

        return nodeViews[index];
    }
}