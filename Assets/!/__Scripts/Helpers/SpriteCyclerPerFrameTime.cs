using UnityEngine;

public class SpriteCyclerPerFrameTime : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;

    [Tooltip("Seconds each frame stays on screen. Must match frames length.")]
    [SerializeField] private float[] frameDurations;

    [SerializeField] private bool stopAtLastFrame = false;

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = true;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        if (!isPlaying || frames == null || frames.Length == 0 || spriteRenderer == null)
            return;

        if (frameDurations == null || frameDurations.Length != frames.Length)
            return;

        timer += Time.deltaTime;

        float currentDuration = Mathf.Max(0.001f, frameDurations[currentFrame]);

        if (timer >= currentDuration)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (stopAtLastFrame)
                {
                    currentFrame = frames.Length - 1;
                    isPlaying = false;
                }
                else
                {
                    currentFrame = 0;
                }
            }

            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    // Optional helpers
    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void Restart()
    {
        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        if (frames != null && frames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = frames[0];
    }
}
