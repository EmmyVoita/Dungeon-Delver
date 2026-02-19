using System.Collections;
using UnityEngine;

public class SpriteCycler : MonoBehaviour
{
    private enum ExitMode
    {
        Manual,
        Automatic
    }
    [Header("Exit Settings")]
    [SerializeField] private ExitMode exitMode = ExitMode.Manual;
    [SerializeField] private float automaticExitDelay = 2f;

    [Header("Main Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 0.1f;
    [SerializeField] private bool loopMain = true;

    [Header("Exit Animation")]
    [SerializeField] private Sprite[] exitFrames;
    [SerializeField] private float exitFrameRate = 0.1f;
    [SerializeField] private bool disableOnExitComplete = false;

    private int currentFrame = 0;
    private float timer = 0f;

    private bool isPlaying = true;
    private bool isExiting = false;

    private Sprite[] activeFrames;
    private float activeFrameRate;
    private bool loopActive;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        // start on main animation
        SetAnimation(frames, frameRate, loopMain);
        if(exitMode == ExitMode.Automatic)
        {
            StartCoroutine(AutomaticExitCoroutine());
        }
    }

    private void Update()
    {
        if (activeFrames == null || activeFrames.Length == 0 || spriteRenderer == null || !isPlaying)
            return;

        timer += Time.deltaTime;

        if (timer >= activeFrameRate)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame >= activeFrames.Length)
            {
                if (loopActive)
                {
                    currentFrame = 0;
                }
                else
                {
                    // one-shot animation finished
                    currentFrame = activeFrames.Length - 1;
                    isPlaying = false;

                    if (disableOnExitComplete)
                        gameObject.SetActive(false);

                    return;
                }
            }

            spriteRenderer.sprite = activeFrames[currentFrame];
        }
    }

    private IEnumerator AutomaticExitCoroutine()
    {
        yield return new WaitForSeconds(automaticExitDelay);
        PlayExitAnimation();
    }

    // 🔹 Switch to exit animation (one-shot)
    public void PlayExitAnimation()
    {
        if (exitFrames == null || exitFrames.Length == 0)
            return;

        isExiting = true;
        SetAnimation(exitFrames, exitFrameRate, false);
    }

    // 🔹 Restart main animation (optional)
    public void PlayMainAnimation()
    {
        isExiting = false;
        SetAnimation(frames, frameRate, loopMain);
    }

    private void SetAnimation(Sprite[] newFrames, float newRate, bool loop)
    {
        activeFrames = newFrames;
        activeFrameRate = newRate;
        loopActive = loop;

        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        if (activeFrames != null && activeFrames.Length > 0)
            spriteRenderer.sprite = activeFrames[0];
    }
}
