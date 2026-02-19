using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TextTypewriter : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private float beginDelay = 0.2f;
    [SerializeField] public TMP_Text textComponent;
    [SerializeField] private string fullText = "GAME OVER";
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private bool castToUpperCase = false;
    public AudioClip typeSound;

    [Header("Wiggle Settings")]
    [SerializeField] private bool enableWiggle = false;
    [SerializeField] private float wiggleAmplitude = 5f;
    [SerializeField] private float wiggleSpeed = 4f;
    [SerializeField] private float minSoundDelay = 0.08f;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float basePitch = 1.0f;

    private bool isTyping = false;
    private bool isWiggling = false;
    private Coroutine typingCoroutine;

    private TMP_TextInfo textInfo;

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    // ------------------------------------------------------------
    // Start typing new text
    // ------------------------------------------------------------
    public void StartTyping(string description, Action onComplete = null)
    {
        fullText = castToUpperCase ? description.ToUpper() : description;
        StopAllCoroutines();
        typingCoroutine = StartCoroutine(TypeText(onComplete));
    }

    // ------------------------------------------------------------
    // Instantly append text (no typing)
    // ------------------------------------------------------------
    public void AppendInstant(string extra)
    {
        StopAllCoroutines();

        textComponent.text += castToUpperCase ? extra.ToUpper() : extra;
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;

        textComponent.maxVisibleCharacters = textInfo.characterCount;
    }

    // ------------------------------------------------------------
    // Instantly replace text (no typing)
    // ------------------------------------------------------------
    public void SetInstant(string newText)
    {
        StopAllCoroutines();

        textComponent.text = castToUpperCase ? newText.ToUpper() : newText;
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;

        textComponent.maxVisibleCharacters = textInfo.characterCount;
    }

    // ------------------------------------------------------------
    // Core typing coroutine
    // ------------------------------------------------------------
    private IEnumerator TypeText(Action onComplete)
    {
        StopWiggle();

        textComponent.text = castToUpperCase ? fullText.ToUpper() : fullText;
        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;

        yield return new WaitForSecondsRealtime(beginDelay);

        isTyping = true;
        float lastSoundTime = -999f;
        int totalVisibleChars = textInfo.characterCount;

        for (int i = 0; i <= totalVisibleChars; i++)
        {
            textComponent.maxVisibleCharacters = i;

            if (typeSound != null && Time.unscaledTime - lastSoundTime >= minSoundDelay)
            {
                AudioHelpers.PlayClipWithVariation(
                    typeSound,
                    AudioChannel.UI,
                    Camera.main.transform.position,
                    basePitch,
                    0.05f,
                    volume
                );
                lastSoundTime = Time.unscaledTime;
            }

            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false;
        onComplete?.Invoke();

        if (enableWiggle)
            StartWiggle();
    }

    // ------------------------------------------------------------
    // Wiggle logic
    // ------------------------------------------------------------
    private void StartWiggle()
    {
        StopWiggle();
        isWiggling = true;
        StartCoroutine(WiggleCharacters());
    }

    private void StopWiggle()
    {
        isWiggling = false;
    }

    private IEnumerator WiggleCharacters()
    {
        while (isWiggling)
        {
            textComponent.ForceMeshUpdate();
            textInfo = textComponent.textInfo;

            float time = Time.unscaledTime * wiggleSpeed;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;

                float wave = Mathf.Sin(time + i * 0.4f) * wiggleAmplitude;
                Vector3 offset = new Vector3(0, wave, 0);

                verts[vertexIndex + 0] += offset;
                verts[vertexIndex + 1] += offset;
                verts[vertexIndex + 2] += offset;
                verts[vertexIndex + 3] += offset;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }

            yield return null;
        }
    }
}
