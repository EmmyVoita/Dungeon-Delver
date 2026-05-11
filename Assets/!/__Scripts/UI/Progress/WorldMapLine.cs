using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WorldMapLine : MonoBehaviour
{
    [SerializeField] private float cornerRadius = 40f;
    [SerializeField] private int curveResolution = 8;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void BuildPath(List<Vector3> nodes)
    {
        List<Vector3> points = new();

        for (int i = 0; i < nodes.Count; i++)
        {
            // First & last points
            if (i == 0 || i == nodes.Count - 1)
            {
                points.Add(nodes[i]);
                continue;
            }

            Vector3 prev = nodes[i - 1];
            Vector3 current = nodes[i];
            Vector3 next = nodes[i + 1];

            Vector3 dirA = (current - prev).normalized;
            Vector3 dirB = (next - current).normalized;

            // If straight, just add point
            if (Vector3.Dot(dirA, dirB) > 0.99f)
            {
                points.Add(current);
                continue;
            }

            // --- CORNER ---
            Vector3 cornerStart = current - dirA * cornerRadius;
            Vector3 cornerEnd = current + dirB * cornerRadius;

            points.Add(cornerStart);

            // Generate curve
            for (int j = 0; j <= curveResolution; j++)
            {
                float t = j / (float)curveResolution;

                // Quadratic Bezier
                Vector3 p = Mathf.Pow(1 - t, 2) * cornerStart +
                            2 * (1 - t) * t * current +
                            Mathf.Pow(t, 2) * cornerEnd;

                points.Add(p);
            }

            points.Add(cornerEnd);
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}