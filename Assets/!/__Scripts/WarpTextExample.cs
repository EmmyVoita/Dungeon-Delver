using UnityEngine;
using TMPro;

[ExecuteAlways]
public class WarpTextStatic : MonoBehaviour
{
    public AnimationCurve VertexCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.25f, 2.0f),
        new Keyframe(0.5f, 0),
        new Keyframe(0.75f, 2.0f),
        new Keyframe(1, 0f));

    public float CurveScale = 1.0f;

    private TMP_Text m_TextComponent;

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void Start()
    {
        ApplyCurve();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
            ApplyCurve();
    }
#endif

    void LateUpdate()
    {
        if (m_TextComponent == null) return;
        ApplyCurve();
    }


    void ApplyCurve()
    {
        if (m_TextComponent == null) return;

        m_TextComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        if (textInfo.characterCount == 0) return;

        float boundsMinX = m_TextComponent.bounds.min.x;
        float boundsMaxX = m_TextComponent.bounds.max.x;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;


            // Normalized position (0-1)
            float x0 = (vertices[vertexIndex].x - boundsMinX) / (boundsMaxX - boundsMinX);
            float y0 = VertexCurve.Evaluate(x0) * CurveScale;

            // Offset
            Vector3 offset = new Vector3(0, y0, 0);

            // Sample curve at slightly offset points
            float delta = 0.01f; // one-hundredth of the width
            float yBefore = VertexCurve.Evaluate(Mathf.Clamp01(x0 - delta)) * CurveScale;
            float yAfter  = VertexCurve.Evaluate(Mathf.Clamp01(x0 + delta)) * CurveScale;

            // Tangent = slope of curve at this point
            Vector2 tangent = new Vector2(2 * delta * (boundsMaxX - boundsMinX), yAfter - yBefore).normalized;
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;


            // Center of the character before rotation
            Vector3 charMid = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            // Apply offset + rotation
            Matrix4x4 matrix = Matrix4x4.TRS(offset, Quaternion.Euler(0, 0, angle), Vector3.one);

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] -= charMid; // move to origin
                vertices[vertexIndex + j] = matrix.MultiplyPoint3x4(vertices[vertexIndex + j]);
                vertices[vertexIndex + j] += charMid; // move back
            }
        }

        m_TextComponent.UpdateVertexData();
    }
}
