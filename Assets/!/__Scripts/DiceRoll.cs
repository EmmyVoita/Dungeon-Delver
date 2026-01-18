using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DiceRoll : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer diceRenderer;

    [Header("Dice Sprites")]
    public Sprite[] rollSprites;
    public Sprite[] faceSprites;

    [Header("Animation Settings")]
    public float rollDuration = 1.5f;
    public float frameRate = 20f;
    public float moveToCenterDuration = 0.6f;
    public float fadeInDuration = 0.3f;
    public float fadeOutDelay = 0.6f;
    public float fadeOutDuration = 0.3f;
    public float arcHeight = 1.5f;
    public float spinAmount = 360f;
    public AudioClip rollSound;

    [Header("Spawn Motion")]
    public bool randomizeStartPosition = true;
    public Vector2 spawnOffsetRange = new Vector2(2f, 1f); // world units

    private SpriteRenderer sr;
    private System.Action<int> onDiceResult;
    private System.Action<int> onDiceAnimationComplete;
    private Vector3 startPos;
    private Vector3 targetPos;

    void Awake()
    {
        sr = diceRenderer != null ? diceRenderer : GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    public void Roll(System.Action<int> onResult, System.Action<int> onAnimationComplete = null)
    {
        onDiceResult = onResult;
        onDiceAnimationComplete = onAnimationComplete;
        gameObject.SetActive(true);

        // Randomized starting position
        startPos = Vector3.zero;
        if (randomizeStartPosition)
        {
            startPos = new Vector3(
                Random.Range(-spawnOffsetRange.x, spawnOffsetRange.x),
                Random.Range(-spawnOffsetRange.y, spawnOffsetRange.y),
                0f
            );
        }

        transform.position = startPos;
        targetPos = Vector3.zero;

        // Fade in
        sr.DOFade(1f, fadeInDuration).SetUpdate(true);

        // Start fancy motion
        StartCoroutine(ArcMoveAndRoll());
    }

    private IEnumerator ArcMoveAndRoll()
    {
        if (rollSound != null)
            AudioHelpers.PlayMyClipAtPoint(rollSound, AudioChannel.SFX, Camera.main.transform.position);

        float elapsed = 0f;
        float frameTimer = 0f;
        int frameIndex = 0;

        Vector3 startScale = transform.localScale;

        // Animate over time
        while (elapsed < rollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            frameTimer += Time.unscaledDeltaTime;

            // Frame change
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer -= 1f / frameRate;
                frameIndex = (frameIndex + 1) % rollSprites.Length;
                sr.sprite = rollSprites[frameIndex];
            }

            // Progress 0→1 with smooth ease
            float t = elapsed / rollDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // Arc motion (like a jump)
            float heightOffset = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, smoothT);
            newPos.y += heightOffset;
            transform.position = newPos;

            // Spin for flavor
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2f) * spinAmount * (1f - t));

            yield return null;
        }

        // Land result
        int finalIndex = Random.Range(0, faceSprites.Length);
        sr.sprite = faceSprites[finalIndex];

        onDiceAnimationComplete?.Invoke(finalIndex);

        // Bounce and settle
        Sequence settle = DOTween.Sequence();
        settle.Append(transform.DOScale(startScale * 1.2f, 0.15f).SetEase(Ease.OutQuad));
        settle.Append(transform.DOScale(startScale, 0.25f).SetEase(Ease.OutBack));
        settle.Join(transform.DOMoveY(targetPos.y - 0.1f, 0.15f).SetRelative().SetEase(Ease.InSine));
        settle.Append(transform.DOMoveY(targetPos.y, 0.25f).SetEase(Ease.OutBack));
        settle.SetUpdate(true);

        yield return new WaitForSecondsRealtime(fadeOutDelay);
        sr.DOFade(0f, fadeOutDuration).SetUpdate(true)
            .OnComplete(() =>
            {
                onDiceResult?.Invoke(finalIndex);
                Destroy(gameObject);
            });
    }
}
