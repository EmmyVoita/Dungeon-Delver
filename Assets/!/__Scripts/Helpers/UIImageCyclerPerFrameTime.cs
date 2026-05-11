using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class UIImageCyclerPerFrameTime : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] frames;

    [Tooltip("Seconds each frame stays on screen. Must match frames length.")]
    [SerializeField] private float[] frameDurations;

    [SerializeField] private bool stopAtLastFrame = false;

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = true;

    private void Reset()
    {
        image = GetComponent<Image>();
    }

    public void Initalize(Image targetImage, List<Sprite> frames, float frameDuration = 0.1f, bool loop = true)
    {
        image = targetImage;
        this.frames = frames.ToArray();
        OnValidate();

        stopAtLastFrame = loop ? false : true;

        for(int i = 0; i < frameDurations.Length; i++)
        {
            frameDurations[i] = frameDuration;
        }
    }

    private void OnValidate()
    {
        // Auto-resize durations array if frames array changes
        if (frames != null)
        {
            if (frameDurations == null || frameDurations.Length != frames.Length)
            {
                float defaultTime = 0.1f;
                frameDurations = new float[frames.Length];

                for (int i = 0; i < frameDurations.Length; i++)
                    frameDurations[i] = defaultTime;
            }
        }
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0 || image == null)
            return;

        if (frameDurations == null || frameDurations.Length != frames.Length)
            return;

        timer += Time.unscaledDeltaTime; // ✅ better for UI

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

            image.sprite = frames[currentFrame];
        }
    }

    // ───────────────── Helpers ─────────────────

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

        if (frames != null && frames.Length > 0 && image != null)
            image.sprite = frames[0];
    }
}
