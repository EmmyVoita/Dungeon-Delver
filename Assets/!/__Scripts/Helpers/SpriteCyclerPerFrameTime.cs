using UnityEngine;

public class SpriteCyclerPerFrameTime : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;

    [Tooltip("Seconds each frame stays on screen. Must match frames length.")]
    [SerializeField] private float[] frameDurations;

    [SerializeField] private bool stopAtLastFrame = false;
    [SerializeField] private bool playOnAwake = true;

    private int _currentFrame = 0;
    private float _timer = 0f;
    private bool _isPlaying = true;

    private void Awake()
    {
        if(playOnAwake)
            Play();
        else
            Stop();
    }

    public void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _currentFrame = 0;
        _timer = 0f;

        if (frames != null && frames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = frames[0];
    }

    private void OnValidate()
    {
        // Auto-resize durations array if frames array changes
        if (frames != null)
        {
            if (frameDurations == null || frameDurations.Length != frames.Length)
            {
                float defaultTime = 0.1f;

                var newDurations = new float[frames.Length];
                for (int i = 0; i < newDurations.Length; i++)
                    newDurations[i] = defaultTime;

                frameDurations = newDurations;
            }
        }
    }

    private void Update()
    {
        if (!_isPlaying || frames == null || frames.Length == 0 || spriteRenderer == null)
            return;

        if (frameDurations == null || frameDurations.Length != frames.Length)
            return;

        _timer += Time.deltaTime;

        float currentDuration = Mathf.Max(0.001f, frameDurations[_currentFrame]);

        if (_timer >= currentDuration)
        {
            _timer = 0f;
            _currentFrame++;

            if (_currentFrame >= frames.Length)
            {
                if (stopAtLastFrame)
                {
                    _currentFrame = frames.Length - 1;
                    _isPlaying = false;
                }
                else
                {
                    _currentFrame = 0;
                }
            }

            spriteRenderer.sprite = frames[_currentFrame];
        }
    }

    // Optional helpers
    public void Play()
    {
        
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Restart()
    {
        Reset();
        _isPlaying = true;
    }
}
