using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.VFX;

public class PlayerWings : MonoBehaviour
{
    [SerializeField] private ParticleSystem activatePS;
    [SerializeField] private VisualEffect activateVE;

    [Header("Wing Sprites")]
    [SerializeField] private SpriteRenderer wingRenderer;
    [SerializeField] private Sprite[] wingSprites; // 0–2 for flap frames

    [Header("Flap Timing")]
    [SerializeField] private float frameTime = 0.05f; // time per frame
    [SerializeField] private float flapHoldTime = 0.2f; // time to hold at peak

    [Header("Hide Animation")]
    [SerializeField] private float fadeOutDuration = 0.25f;  // how long the fade lasts
    [SerializeField] private bool foldScale = true;          // should wings slightly fold while fading?

    private Coroutine flapRoutine;
    private Tween fadeTween;

    public void ShowWings()
    {
        // If already visible or fading in, skip
        if (wingRenderer.enabled && wingRenderer.color.a > 0.95f)
            return;

        fadeTween?.Kill();
        wingRenderer.enabled = true;

        // Instantly set visible
        Color c = wingRenderer.color;
        c.a = 1f;
        wingRenderer.color = c;

        if (activatePS != null)
            activatePS.Play();

        if(activateVE != null)
            activateVE.Play();
    }

    public void HideWings()
    {
        fadeTween = wingRenderer.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                wingRenderer.enabled = false;

                // Reset alpha for next ShowWings()
                Color c = wingRenderer.color;
                c.a = 1f;
                wingRenderer.color = c;
            });

        // Optional: fold a little as it fades (adds nice motion)
        if (foldScale)
        {
            transform.DOScaleX(0.7f, fadeOutDuration).SetEase(Ease.InSine)
                .OnComplete(() => transform.localScale = Vector3.one);
        }
    }

    // Call this when jump/boost starts
    public void PlayFlap()
    {
        Debug.Log("Playing wing flap animation.");
        if (flapRoutine != null)
            StopCoroutine(flapRoutine);

        flapRoutine = StartCoroutine(FlapSequence());
    }

    private IEnumerator FlapSequence()
    {
        // 3-frame flap
        for (int i = 0; i < wingSprites.Length; i++)
        {
            wingRenderer.sprite = wingSprites[i];
            yield return new WaitForSeconds(frameTime);
        }

        // Hold at peak before resetting
        yield return new WaitForSeconds(flapHoldTime);
        wingRenderer.sprite = wingSprites[0];
        flapRoutine = null;
    }
}
