using System.Collections.Generic;
using UnityEngine;

public class WorldMapView : MonoBehaviour
{
    [SerializeField] private WorldMapLine lineRenderer;
    [Header("Data")]
    [SerializeField] private WorldMapData mapData;

    [Header("Prefabs")]
    [SerializeField] private WorldNodeView nodePrefab;
    [SerializeField] private WorldPathView pathPrefab;

    [Header("Layout")]
    [SerializeField] private float spacing = 120f;

    private List<WorldNodeView> nodeViews = new();


    /*
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            Build();
        }
    }
    */

    public void Build()
    {
        Clear();

        // 1. Spawn nodes (IN ORDER)
        foreach (var node in mapData.nodes)
        {
            Vector2 pos = GridToWorld(node.gridPosition);

            var view = Instantiate(nodePrefab, transform);
            view.transform.localPosition = pos;

            view.Initialize(node.index, node.isBoss);

            nodeViews.Add(view);
        }
        
        // 2. Connect sequentially (no branching)
        for (int i = 0; i < nodeViews.Count - 1; i++)
        {
            var from = nodeViews[i];
            var to = nodeViews[i + 1];

            var path = Instantiate(pathPrefab, transform);
            path.Setup(from.transform.position, to.transform.position);
        }
        
        /*
        List<Vector3> pathPoints = new();

        foreach (var node in nodeViews)
        {
            pathPoints.Add(node.transform.position);
        }

        lineRenderer.BuildPath(pathPoints);
        */
    }

    private Vector2 GridToWorld(Vector2 gridPos)
    {
        return new Vector2(
            gridPos.x * spacing,
            gridPos.y * spacing
        );
    }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        nodeViews.Clear();
    }

    public WorldNodeView GetNode(int index)
    {
        if (index < 0 || index >= nodeViews.Count)
            return null;

        return nodeViews[index];
    }
}