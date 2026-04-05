using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIImageAnimator : MonoBehaviour
{
    [Header("Animation Frames")]
    public Image targetImage;
    public Sprite[] frames;
    public float[] frameTimes; // custom duration per frame (same length as frames)
    public bool loop = false;

    [Header("Audio Settings")]
    public AudioClip startClip;          // plays once at animation start
    public AudioClip[] frameClips;       // optional: one per frame
    public bool playFrameSounds = false; // if true, plays sound each frame

    private int currentFrame = 0;

    void OnEnable()
    {
        UIManager.OnGameOverUI += OnGameOver;
    }

    void OnDisable()
    {
        UIManager.OnGameOverUI -= OnGameOver;
    }

    private void OnGameOver()
    {
        Debug.Log("GAME OVER animation triggered");
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        if (frames.Length == 0 || frameTimes.Length != frames.Length)
        {
            Debug.LogError("Frames and frameTimes must be the same length!");
            yield break;
        }

        // 🔹 Play intro clip (optional)
        if (startClip != null)
            PlaySound(startClip);

        do
        {
            for (int i = 0; i < frames.Length; i++)
            {
                targetImage.sprite = frames[i];

                // 🔹 Optional per-frame sound
                if (playFrameSounds && frameClips != null && i < frameClips.Length && frameClips[i] != null)
                    PlaySound(frameClips[i]);

                yield return new WaitForSecondsRealtime(frameTimes[i]);
            }
        }
        while (loop);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

 
            // fallback: 2D spatial UI sound
            AudioHelpers.PlayMyClipAtPoint(clip, AudioChannel.UI, Camera.main.transform.position);
        
    }
}
