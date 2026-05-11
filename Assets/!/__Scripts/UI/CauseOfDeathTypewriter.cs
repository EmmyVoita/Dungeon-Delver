using System.Collections;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CauseOfDeathTypewriter : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private string prefix = "Killed by";
    [SerializeField] private float typeSpeed = 0.05f; // same as NPCDialogue
    public AudioClip typeSound; // optional typing sound

    [Header("Wiggle Settings")]
    [SerializeField] private bool enableWiggle = true;
    [SerializeField] private float wiggleAmplitude = 5f;
    [SerializeField] private float wiggleSpeed = 4f;

    private bool isTyping = false;
    private bool isWiggling = false;
    private Coroutine typingCoroutine;

    private TMP_TextInfo textInfo;

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        //OnGameOver();
    }

    private void OnEnable() => GameStateManager.OnStateChanged += HandleStateChanged;
    private void OnDisable() => GameStateManager.OnStateChanged -= HandleStateChanged;

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.GameOverTally && previousState != newState)
        {
            OnGameOver();
        }
    }

    private void OnGameOver()
    {
        StopAllCoroutines();
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        textComponent.text = "";
        isTyping = true;
        isWiggling = false;

        string currentText = "";
        string fullText = prefix + Player.Instance.LastDamageSource;

        foreach (char c in fullText)
        {
            AudioHelpers.PlayClipWithVariation(typeSound, AudioChannel.UI, Camera.main.transform.position, 1.0f, 0.05f);
            currentText += c;
            textComponent.text = currentText;
            textComponent.ForceMeshUpdate(); // ensure TMP rebuilds
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false;

        if (enableWiggle)
        {
            isWiggling = true;
            StartCoroutine(WiggleCharacters());
        }
    }

    private IEnumerator WiggleCharacters()
    {
        while (isWiggling)
        {
            textComponent.ForceMeshUpdate();
            textInfo = textComponent.textInfo;

            float time = Time.unscaledTime * wiggleSpeed; // ✅ Unscaled time!

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

            // Apply updated vertices back
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }

            yield return null; // still fine, since it's frame-based
        }
    }

}
