using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningZapEffect : MonoBehaviour
{
    [SerializeField] private int points = 8;
    [SerializeField] private float jaggedness = 0.25f;

    [Header("Timing")]
    [SerializeField] private float buildStepDelay = 0.015f;
    [SerializeField] private float holdTime = 0.04f;
    [SerializeField] private float destroyStepDelay = 0.01f;

    private LineRenderer _line;
    private Vector3[] _points;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
    }

    public void Play(Vector3 start, Vector3 end)
    {
        if (_line == null)
            _line = GetComponent<LineRenderer>();

        _points = GeneratePoints(start, end);

        StopAllCoroutines();
        StartCoroutine(PlaySequence());
    }

    private Vector3[] GeneratePoints(Vector3 start, Vector3 end)
    {
        Vector3[] result = new Vector3[points];

        Vector3 direction = end - start;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.forward);

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            if (i != 0 && i != points - 1)
            {
                float offset = Random.Range(-jaggedness, jaggedness);
                point += perpendicular * offset;
            }

            result[i] = point;
        }

        return result;
    }

    private IEnumerator PlaySequence()
    {
        float initialWidth = _line.widthMultiplier;
        _line.positionCount = 0;

        // Build outward
        for (int i = 0; i < _points.Length; i++)
        {
            float t = (i + 1) / (float)_points.Length;

            _line.positionCount = i + 1;
            _line.SetPosition(i, _points[i]);
            _line.widthMultiplier = initialWidth * t;

            yield return new WaitForSeconds(buildStepDelay);
        }

        yield return new WaitForSeconds(holdTime);

        // Destroy one segment at a time while shrinking
        for (int i = _points.Length; i >= 0; i--)
        {
            float t = i / (float)_points.Length;

            _line.positionCount = i;
            _line.widthMultiplier = initialWidth * t;

            yield return new WaitForSeconds(destroyStepDelay);
        }

        _line.widthMultiplier = 0f;

        Destroy(gameObject);
    }
}