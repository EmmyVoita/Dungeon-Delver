using UnityEngine;
using DG.Tweening;
using System.Collections;

public class RecipeSlotUI : MonoBehaviour
{
    public AudioClip appearSound;
    public Transform slotTransform;
    public SpriteRenderer icon;
    public float slotScale = 1.0f;

    [Header("Colors")]
    public Color filledColor = Color.white;
    public Color emptyColor = new Color(1, 1, 1, 0.25f);

    [Header("Floating Settings")]
    public bool enableFloating = true;
    public float floatAmplitudeMin = 0.05f;
    public float floatAmplitudeMax = 0.15f;
    public float floatSpeedMin = 1.0f;
    public float floatSpeedMax = 2.0f;

    [Header("Animation Settings")]
    public float centerPullPercent = 0.7f;
    public float moveDuration = 0.75f;

    [Header("Collect Spin")]
    public Transform spinRoot;                   // PARENT of slotTransform
    public float collectSpinDegrees = 360f;
    public float collectSpinDuration = 0.35f;
    public Ease collectSpinEase = Ease.OutCubic;

    [Header("Audio / VFX")]
    public GameObject destroyEffect;
    public SortingLayerPicker collectEffectSortingLayer;
    public int sortingOrder = -10;
    public GameObject collectEffect;
    public float audioVolume = 0.5f;
    public AudioClip destoryAudio;

    private Vector3 basePos;
    private Quaternion spinBaseRotation;


    void Awake()
    {
        // Start fully invisible
        if (icon != null)
        {
            Color c = icon.color;
            c.a = 0f;
            icon.color = c;
        }
    }


    void Start()
    {
        basePos = slotTransform.localPosition;

        if (spinRoot != null)
            spinBaseRotation = spinRoot.localRotation;

        if (enableFloating)
            StartFloating();

    }

    public IEnumerator FadeIn(float duration, float pitch = 1.0f)
    {
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0f);

        Tween t = icon.DOColor(emptyColor, duration).SetEase(Ease.OutCubic);

        if (appearSound != null)
            AudioHelpers.PlayMyClipAtPoint(
                appearSound,
                AudioChannel.SFX,
                Camera.main.transform.position,
                pitch: pitch
            );

        yield return t.WaitForCompletion();
    }

    private void StartFloating()
    {
        float amplitude = Random.Range(floatAmplitudeMin, floatAmplitudeMax);
        float speed = Random.Range(floatSpeedMin, floatSpeedMax);
        float startOffset = Random.Range(0f, 0.5f);

        slotTransform.DOLocalMoveY(basePos.y + amplitude, speed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(startOffset);

        slotTransform.DOLocalRotate(
            new Vector3(0, 0, Random.Range(-3f, 3f)),
            speed * 1.3f
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo)
        .SetDelay(startOffset);
    }

    public void SetSprite(Sprite s)
    {
        icon.sprite = s;
    }

    public void SetFilled()
    {
        icon.DOColor(filledColor, 0.25f);

        Vector3 baseScale = transform.localScale;

        transform.DOScale(baseScale * 1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
                transform.DOScale(baseScale, 0.15f)
            );

        PlayCollectSpin();

        if (collectEffect != null)
        {
            GameObject fx = Instantiate(
                collectEffect,
                transform.position,
                Quaternion.identity
            );

            var psRenderer = fx.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerID =
                    collectEffectSortingLayer.layerID;
                psRenderer.sortingOrder = sortingOrder;
            }
        }
    }

    private void PlayCollectSpin()
    {
        if (spinRoot == null)
            return;

        spinRoot.DOKill();

        spinRoot.localRotation = spinBaseRotation;

        Sequence spinSeq = DOTween.Sequence();

        spinSeq.Append(
            spinRoot.DOLocalRotate(
                new Vector3(0, 0, collectSpinDegrees),
                collectSpinDuration,
                RotateMode.FastBeyond360
            ).SetEase(collectSpinEase)
        );

        spinSeq.Append(
            spinRoot.DOLocalRotateQuaternion(
                spinBaseRotation,
                0.15f
            ).SetEase(Ease.OutCubic)
        );
    }

    private void KillAllTweens()
    {
        DOTween.Kill(transform);
        DOTween.Kill(slotTransform);

        if (spinRoot != null)
            DOTween.Kill(spinRoot);

        if (icon != null)
            DOTween.Kill(icon);
    }


    public void AnimateDisappear()
    {
        Sequence seq = DOTween.Sequence();

        Vector3 current = transform.localPosition;
        Vector3 toCenter = new Vector3(current.x, 0f, 0f) - current;
        Vector3 targetPos = current + toCenter * centerPullPercent;

        seq.Append(
            transform.DOLocalMove(targetPos, moveDuration)
                .SetEase(Ease.InOutQuad)
        );

        seq.Join(
            transform.DOScale(0.3f, moveDuration * 0.9f)
                .SetEase(Ease.InBack)
        );

        seq.AppendCallback(() =>
        {
            icon.color = Color.clear;

            if (destroyEffect != null)
                Instantiate(
                    destroyEffect,
                    transform.position,
                    Quaternion.identity
                );

            if (destoryAudio != null)
                AudioHelpers.PlayMyClipAtPoint(
                    destoryAudio,
                    AudioChannel.SFX,
                    Camera.main.transform.position,
                    volume: audioVolume
                );
        });

        seq.Append(icon.DOFade(0f, 0.2f));

        seq.OnComplete(() => 
        {
            KillAllTweens();
            Destroy(gameObject, 0.5f);
        });
    }
}
