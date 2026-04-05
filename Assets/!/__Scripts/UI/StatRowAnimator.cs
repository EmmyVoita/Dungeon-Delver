using UnityEngine;
using DG.Tweening;

public class StatRowAnimator : MonoBehaviour
{
    [Header("Slide-In Settings")]
    public AudioClip slideInSound;
    public bool slideFromLeft = false;         // default = slide from right
    public float slideOffset = 300f;           // how far offscreen it begins
    public float slideDuration = 0.40f;
    public Ease slideEase = Ease.OutCubic;

    [Header("Fade Settings")]
    public float fadeDuration = 0.30f;

    [Header("Optional Scale Punch")]
    public bool useScalePunch = true;
    public float punchScale = 1.05f;
    public float punchDuration = 0.25f;

    [SerializeField]private CanvasGroup cg;
    private RectTransform rt;
    private Vector2 originalPos;

    private void Awake()
    {
        rt = transform as RectTransform;

        if(cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0;

        originalPos = rt.anchoredPosition;
    }


    public void PlayIntro()
    {
        float direction = slideFromLeft ? -1f : 1f;

        AudioHelpers.PlayMyClipAtPoint(slideInSound, AudioChannel.SFX, Camera.main.transform.position);

        // Start offscreen
        rt.anchoredPosition = originalPos + new Vector2(slideOffset * direction, 0);
        cg.alpha = 0;

        // Slide to the real position
        rt.DOAnchorPos(originalPos, slideDuration)
          .SetEase(slideEase);

        // Fade in
        cg.DOFade(1f, fadeDuration);

        // Little scale bounce
        if (useScalePunch)
        {
            rt.localScale = Vector3.one;
            rt.DOPunchScale(Vector3.one * (punchScale - 1f), punchDuration, 8, 0.8f);
        }
    }

    public void SnapToFinal()
    {
        rt.anchoredPosition = originalPos;
        cg.alpha = 1f;
    }

    public void PlayOutro()
    {
        float direction = slideFromLeft ? 1f : -1f;

        // Slide offscreen
        rt.DOAnchorPos(originalPos + new Vector2(slideOffset * direction, 0), slideDuration)
          .SetEase(slideEase);

        // Fade out
        cg.DOFade(0f, fadeDuration);

        // Little scale bounce
        if (useScalePunch)
        {
            rt.localScale = Vector3.one;
            rt.DOPunchScale(Vector3.one * (punchScale - 1f), punchDuration, 8, 0.8f);
        }
    }
}
