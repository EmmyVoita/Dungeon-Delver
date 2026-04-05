using System.Collections.Generic;
using UnityEngine;

public class LaneVisualizer : MonoBehaviour
{
    [SerializeField] private GameObject laneLinePrefab;

    private List<GameObject> activeLines = new List<GameObject>();

    public void ShowLanes(int maxLanes, float spacing)
    {
        Clear();

        float centerOffset = (maxLanes - 1) * 0.5f;

        for (int i = 0; i < maxLanes; i++)
        {
            float y = (i - centerOffset) * spacing;

            GameObject line = Instantiate(laneLinePrefab, transform);
            line.transform.localPosition = new Vector3(0, y, 0);

            activeLines.Add(line);
        }
    }

    public void Clear()
    {
        foreach (var line in activeLines)
        {
            if (line != null)
                Destroy(line);
        }

        activeLines.Clear();
    }
}