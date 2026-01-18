using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class CurveText : MonoBehaviour
{
    public AnimationCurve curve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.5f, 0.5f),
        new Keyframe(1, 0)
    ); // shape of curve

    public float curveScale = 10f; // how strong the bend is

    private TMP_Text textComponent;
    private Mesh mesh;
    private Vector3[] vertices;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        mesh = textComponent.mesh;
        vertices = mesh.vertices;

        int characterCount = textComponent.textInfo.characterCount;

        if (characterCount == 0) return;

        for (int i = 0; i < characterCount; i++)
        {
            var charInfo = textComponent.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;

            Vector3[] srcVertices = textComponent.textInfo.meshInfo[materialIndex].vertices;

            // Middle of the character in local space
            Vector3 offsetToMidBaseline = (srcVertices[vertexIndex + 0] +
                                           srcVertices[vertexIndex + 2]) / 2;

            // Normalize to 0–1 across the text width
            float x0 = Mathf.InverseLerp(charInfo.origin, charInfo.xAdvance, offsetToMidBaseline.x);

            // Evaluate curve at this point
            float yOffset = curve.Evaluate(x0) * curveScale;

            Vector3 offset = new Vector3(0, yOffset, 0);

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] = srcVertices[vertexIndex + j] + offset;
            }
        }

        mesh.vertices = vertices;
        textComponent.canvasRenderer.SetMesh(mesh);
    }
}
