using System.Collections;
using TMPro;
using UnityEngine;

public class RewardStampText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private bool toUpper = false;

    [Header("Timing")]
    [SerializeField] private float letterInterval = 0.04f;
    [SerializeField] private float letterDuration = 0.22f;

    [Header("Scale")]
    [SerializeField] private float overshootScale = 1.35f;

    [Header("Rotation")]
    [SerializeField] private float startRotation = -8f;
    [SerializeField] private float rotationVariation = 3f;

    [Header("Curve")]
    [SerializeField] private AnimationCurve scaleCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.65f, 1.2f),
            new Keyframe(1f, 1f)
        );

    private Coroutine _animationRoutine;

    public void Play(string message)
    {
        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        if(toUpper)
            message = message.ToUpper();

        _animationRoutine = StartCoroutine(AnimateText(message));
    }
    public void Hide()
    {
        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        textComponent.text = "";
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            Play("PERFECT");
        }
    }

    private IEnumerator AnimateText(string message)
    {
        textComponent.text = message;
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;

        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] =
                textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }

        float[] letterStartTimes =
            new float[textInfo.characterCount];

        float[] letterRotations =
            new float[textInfo.characterCount];

        int visibleLetterIndex = 0;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo =
                textInfo.characterInfo[i];

            if (!characterInfo.isVisible)
                continue;

            letterStartTimes[i] =
                visibleLetterIndex * letterInterval;

            letterRotations[i] =
                startRotation +
                Random.Range(-rotationVariation, rotationVariation);

            visibleLetterIndex++;
        }

        float totalDuration =
            Mathf.Max(0, visibleLetterIndex - 1) * letterInterval
            + letterDuration;

        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Restore the original vertices before applying
            // the current frame's transformations.
            for (int meshIndex = 0;
                 meshIndex < textInfo.meshInfo.Length;
                 meshIndex++)
            {
                originalVertices[meshIndex].CopyTo(
                    textInfo.meshInfo[meshIndex].vertices,
                    0
                );
            }

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo characterInfo =
                    textInfo.characterInfo[i];

                if (!characterInfo.isVisible)
                    continue;

                float normalizedTime = Mathf.Clamp01(
                    (elapsed - letterStartTimes[i]) /
                    letterDuration
                );

                float scale =
                    scaleCurve.Evaluate(normalizedTime);

                float rotation =
                    Mathf.Lerp(
                        letterRotations[i],
                        0f,
                        normalizedTime
                    );

                ApplyCharacterTransform(
                    textInfo,
                    i,
                    scale,
                    rotation
                );
            }

            UpdateMesh(textInfo);

            yield return null;
        }

        // Ensure every letter finishes exactly at its
        // original scale and rotation.
        for (int meshIndex = 0;
             meshIndex < textInfo.meshInfo.Length;
             meshIndex++)
        {
            originalVertices[meshIndex].CopyTo(
                textInfo.meshInfo[meshIndex].vertices,
                0
            );
        }

        UpdateMesh(textInfo);

        _animationRoutine = null;
    }

    private void ApplyCharacterTransform(
        TMP_TextInfo textInfo,
        int characterIndex,
        float scale,
        float rotationDegrees
    )
    {
        TMP_CharacterInfo characterInfo =
            textInfo.characterInfo[characterIndex];

        int materialIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;

        Vector3[] vertices =
            textInfo.meshInfo[materialIndex].vertices;

        Vector3 center =
            (vertices[vertexIndex] +
             vertices[vertexIndex + 2]) * 0.5f;

        Quaternion rotation =
            Quaternion.Euler(0f, 0f, rotationDegrees);

        for (int i = 0; i < 4; i++)
        {
            int currentVertex = vertexIndex + i;

            Vector3 offset =
                vertices[currentVertex] - center;

            offset *= scale;
            offset = rotation * offset;

            vertices[currentVertex] =
                center + offset;
        }
    }

    private void UpdateMesh(TMP_TextInfo textInfo)
    {
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo =
                textInfo.meshInfo[i];

            meshInfo.mesh.vertices =
                meshInfo.vertices;

            textComponent.UpdateGeometry(
                meshInfo.mesh,
                i
            );
        }
    }

    private void OnDisable()
    {
        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }
    }
}