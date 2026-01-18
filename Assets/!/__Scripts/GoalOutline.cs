using DG.Tweening;
using UnityEngine;

public class GoalOutline : MonoBehaviour
{
    [Header("Outlines")]
    public Sprite squareOutline;
    public Sprite octagonOutline;

    [Header("Settings")]
    public float fadeOutDuration = 0.25f;

    private SpriteRenderer sRend;
    private Tween fadeTween;
    private Sprite currentSprite; // tracks which sprite is currently shown

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        sRend.sprite = squareOutline;
        currentSprite = sRend.sprite; // initialize
        
    }

    void Update()
    {
        Sprite targetSprite;
        Vector3 targetScale;

        if (Player.Instance.UseEightDirections)
        {
            targetSprite = octagonOutline;
            targetScale = new Vector3(0.175f, 0.175f, 1);
        }
        else
        {
            targetSprite = squareOutline;
            targetScale = Vector3.one;
        }

        // ✅ Only fade if sprite actually needs to change
        if (targetSprite != currentSprite)
        {
            FadeState(targetSprite, targetScale);
            currentSprite = targetSprite;
        }
    }

    private void FadeState(Sprite newSprite, Vector3 newScale)
    {
        fadeTween?.Kill();

        Sequence seq = DOTween.Sequence();

        // Step 1: Fade out
        seq.Append(sRend.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine));

        // Step 2: Switch sprite & scale
        seq.AppendCallback(() =>
        {
            sRend.sprite = newSprite;
            transform.localScale = newScale;
        });

        // Step 3: Fade in new sprite
        seq.Append(sRend.DOFade(1f, fadeOutDuration).SetEase(Ease.OutSine));

        fadeTween = seq;
    }
}
