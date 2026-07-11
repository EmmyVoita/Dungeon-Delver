using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TextTypewriter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public TMP_Text textComponent;


    [Header("Text Settings")]   
    [SerializeField] private float beginDelay = 0.2f;
    [Range(1,300)] [SerializeField] private int charactersPerSecond = 20;
    [SerializeField] private bool castToUpperCase = false;
    

    [Header("Wiggle Settings")]
    [SerializeField] private bool enableWiggle = false;
    [SerializeField] private float wiggleAmplitude = 5f;
    [SerializeField] private float wiggleSpeed = 4f;
    [SerializeField] private float minSoundDelay = 0.08f;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float basePitch = 1.0f;


    [Header("Behaviour")]
    [SerializeField] private bool stopOnPause = false;


    
    private Coroutine _typingCoroutine;

    private TMP_TextInfo _textInfo;
    private bool _paused;
    private bool _isWiggling = false;

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }


    

    private void OnEnable()
    {
        OverlayManager.OnOverlayChanged += HandleOverlayChanged;
    }

    private void OnDisable()
    {
        OverlayManager.OnOverlayChanged -= HandleOverlayChanged;
    }

    private void HandleOverlayChanged(OverlayState previousState, OverlayState newState)
    {
        if(!stopOnPause) return;

        if(newState == OverlayState.Pause)
        {
            _paused = true;
            return;
        }
           
        if(newState != OverlayState.Pause && previousState == OverlayState.Pause)
            _paused = false;
    }


    // ------------------------------------------------------------
    // Start typing new text
    // ------------------------------------------------------------

    public void StartTyping(string newText, Action onComplete = null)
    {
        StopAllCoroutines();
        textComponent.text = castToUpperCase ? newText.ToUpper() : newText;
        _typingCoroutine = StartCoroutine(TypeText(onComplete));
        _paused = false;
    }

    // ------------------------------------------------------------
    // Instantly append text (no typing)
    // ------------------------------------------------------------
    public void AppendInstant(string extra)
    {
        StopAllCoroutines();

        textComponent.text += castToUpperCase ? extra.ToUpper() : extra;
        textComponent.ForceMeshUpdate();
        _textInfo = textComponent.textInfo;

        textComponent.maxVisibleCharacters = _textInfo.characterCount;
    }

    // ------------------------------------------------------------
    // Instantly replace text (no typing)
    // ------------------------------------------------------------
    public void SetInstant(string newText)
    {
        StopAllCoroutines();

        textComponent.text = castToUpperCase ? newText.ToUpper() : newText;
        textComponent.ForceMeshUpdate();
        _textInfo = textComponent.textInfo;

        textComponent.maxVisibleCharacters = _textInfo.characterCount;
    }

    // ------------------------------------------------------------
    // Core typing coroutine
    // ------------------------------------------------------------
    private IEnumerator TypeText(Action onComplete)
    {
        StopWiggle();

        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate();
        _textInfo = textComponent.textInfo;

        yield return new WaitForSecondsRealtime(beginDelay);

        float charsVisible = 0f;
        float lastSoundTime = -999f;
        int totalVisibleChars = _textInfo.characterCount;

        int i = 0;

        while(textComponent.maxVisibleCharacters < totalVisibleChars)
        {
            
            while (_paused)
            {
                yield return null;
            };
            
            charsVisible += charactersPerSecond * Time.unscaledDeltaTime;

            int visibleCount = Mathf.FloorToInt(charsVisible);
            visibleCount = Mathf.Clamp(visibleCount,0,totalVisibleChars);

            if(visibleCount > textComponent.maxVisibleCharacters)
            {
                textComponent.maxVisibleCharacters = visibleCount;

                if (Time.unscaledTime - lastSoundTime >= minSoundDelay)
                {
                    AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.typewriterBlip, transform.position);
                    lastSoundTime = Time.unscaledTime;
                }
            }
    

            yield return null;//new WaitForSecondsRealtime(1f/(float)charactersPerSecond);
        }

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
        _isWiggling = true;
        StartCoroutine(WiggleCharacters());
    }

    private void StopWiggle()
    {
        _isWiggling = false;
    }

    private IEnumerator WiggleCharacters()
    {
        while (_isWiggling)
        {
            textComponent.ForceMeshUpdate();
            _textInfo = textComponent.textInfo;

            float time = Time.unscaledTime * wiggleSpeed;

            for (int i = 0; i < _textInfo.characterCount; i++)
            {
                if (!_textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = _textInfo.characterInfo[i].vertexIndex;
                int materialIndex = _textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] verts = _textInfo.meshInfo[materialIndex].vertices;

                float wave = Mathf.Sin(time + i * 0.4f) * wiggleAmplitude;
                Vector3 offset = new Vector3(0, wave, 0);

                verts[vertexIndex + 0] += offset;
                verts[vertexIndex + 1] += offset;
                verts[vertexIndex + 2] += offset;
                verts[vertexIndex + 3] += offset;
            }

            for (int i = 0; i < _textInfo.meshInfo.Length; i++)
            {
                var meshInfo = _textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }

            yield return null;
        }
    }
}
