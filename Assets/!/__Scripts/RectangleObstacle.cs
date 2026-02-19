using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class RectangleObstacle : MonoBehaviour
{
    private enum ExitMode
    {
        LeaveScreen,
        ShrinkAndFadeOut,
        AnimateThenFadeOut,
        FadeOut,
        JustWait
    }

    [SerializeField] private GameObject obstacleRoot;
    [SerializeField] private int laserBursts = 1;
    [SerializeField] private float delayBetweenBursts = 0.4f;
    [SerializeField] private bool flipBetweenBursts = false;


    

    [Header("Exit Mode: General")]
    [SerializeField] private float waitDuration = 1.0f;
    [SerializeField] private float completionDelay = 1.0f;

    [Header("Exit Mode: Animate Then Fade Out")]
    [SerializeField] private float fadeOutDelay = 0.0f;



    [SerializeField] private ExitMode exitMode = ExitMode.LeaveScreen;
    [Header("Timing")]
    [SerializeField] private float startDelay = 1.25f;
    [SerializeField] private int warningPulseCount = 3;
    [SerializeField] private float pulseDuration = 0.15f;
    [SerializeField] private float delayBetweenPulses = 0.1f;
    [SerializeField] private bool killOnFinish = true;

    [Header("Active Phase")]
    [SerializeField] private float activeDuration = 0.5f;
    [SerializeField] private GameObject childRenderer;
    [SerializeField] private LaserBeamHeightPulse beamPulse;


    [Header("Laser Cannons")]
    [SerializeField] private Transform leftCannon;
    [SerializeField] private Transform rightCannon;

    [SerializeField] private Transform leftCannonVFX;
    [SerializeField] private Transform rightCannonVFX;

    [SerializeField] private List<SpriteRenderer> cannonSprites;
    [SerializeField] private List<SpriteRenderer> hazardLineSprites;
    [SerializeField] private float spriteFadeInDuration = 0.15f;
    [SerializeField] private GameObject cannonFireEffect;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private float destroyEffectTimeOffset = 0.2f;
    [SerializeField] private GameObject cannonProjectileEffect;
    [SerializeField] private List<SpriteCycler> spriteCyclers;
    [SerializeField] private List<SpriteCyclerPerFrameTime> spriteCyclersPerFrameTime;

    [SerializeField] private float cannonFinalScale = 1.18f;

    [SerializeField] private float minShakeStrength = 0.01f;
    [SerializeField] private float maxShakeStrength = 0.12f;
    [SerializeField] private float shakeStepDuration = 0.2f;
    [SerializeField] private int shakeVibrato = 20;

    [Header("Auto Rotation")]
    [SerializeField] private bool autoRotate = false;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilDistance = 5f;
    [SerializeField] private float recoilDuration = 0.1f;

    [Header("Exit Settings")]
    [SerializeField] private float exitFadeDelay = 0.15f;
    [SerializeField] private float exitFadeDuration = 0.3f;
    [SerializeField] private float exitShrinkDuration = 0.3f;


    [Header("Audio")]
    public AudioClip activationSound;
    public SoundEffect pulseSound;
    public AudioClip chargeSound;


    private BoxCollider2D col;
    private Sequence flashSequence;

    private Vector3 leftCannonBaseScale;
    private Vector3 rightCannonBaseScale;

    private bool hitLogicComplete = false;
    private bool exitAnimationComplete = false;
    private int laserIndex = 0;

    void Awake()
    {
        if (obstacleRoot == null)
            obstacleRoot = transform.root.gameObject;

        col = GetComponent<BoxCollider2D>();
        col.enabled = false;

        if (leftCannon != null)
            leftCannonBaseScale = leftCannon.localScale;

        if (rightCannon != null)
            rightCannonBaseScale = rightCannon.localScale;

        if (childRenderer != null)
            childRenderer.SetActive(false);

        ObstacleManager.Instance.RegisterObstacle(obstacleRoot);

        if (autoRotate)
        {
            float randomZ = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, randomZ);
            rotationSpeed *= Random.value < 0.5f ? 1f : -1f;
        }

        foreach (var sr in cannonSprites)
        {
            sr.color = new Color(1f, 1f, 1f, 0f);
        }

        foreach (var sr in hazardLineSprites)
        {
            sr.color = new Color(1f, 1f, 1f, 0f);
        }

        hitLogicComplete = false;
        exitAnimationComplete = false;


        StartCoroutine(StartFlashCoroutine());
    }

    void Update()
    {
        if (autoRotate)
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        /*
        if (hitLogicComplete && exitAnimationComplete && killOnFinish)
        {
            killOnFinish = false;
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Destroy(gameObject, 0.3f);
        }
        */
    }

    private IEnumerator StartFlashCoroutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < laserBursts; i++)
        {
            laserIndex = i;
            hitLogicComplete = false;
            exitAnimationComplete = false;

            PlayWarningFlash();

            // wait until this burst fully completes
            yield return new WaitUntil(() => hitLogicComplete && exitAnimationComplete);

            // small pause before next warning
            if (i < laserBursts - 1)
                yield return new WaitForSeconds(delayBetweenBursts);
        }

        // Only now allow destruction
        if (killOnFinish)
        {
            ObstacleManager.Instance.UnregisterObstacle(obstacleRoot);
            Destroy(obstacleRoot, 0.3f);
        }
    }

    private void ResetCannonsVisuals()
    {
        if (leftCannon != null)
            leftCannon.localScale = leftCannonBaseScale;

        if (rightCannon != null)
            rightCannon.localScale = rightCannonBaseScale;

        foreach (var sr in cannonSprites)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        foreach (var sr in hazardLineSprites)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
    }



    public void PlayWarningFlash()
    {
        flashSequence?.Kill();
        ResetCannonsVisuals();
        flashSequence = DOTween.Sequence();
        foreach (var spriteCyclyer in spriteCyclers)
            spriteCyclyer.PlayMainAnimation();

        foreach (var spriteCyclyer in spriteCyclersPerFrameTime)
            spriteCyclyer.Restart();

        if(flipBetweenBursts)
        {
            transform.localScale = new Vector3(
                -transform.localScale.x,
                -transform.localScale.y,
                transform.localScale.z
            );
        }

        // Reset transforms
        if (leftCannon != null) leftCannon.localScale = leftCannonBaseScale;
        if (rightCannon != null) rightCannon.localScale = rightCannonBaseScale;

        // ------------------------------------------------------------
        // PHASE 1 — FADE IN ALL SPRITES TOGETHER
        // ------------------------------------------------------------
        Sequence fadeInSeq = DOTween.Sequence();

        foreach (var sr in cannonSprites)
        {
            // Make sure they start invisible
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            fadeInSeq.Join(sr.DOFade(1f, spriteFadeInDuration));
        }

        foreach (var sr in hazardLineSprites)
        {
            // Make sure they start invisible
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            fadeInSeq.Join(sr.DOFade(1f, spriteFadeInDuration));
        }

        // Append fade phase
        flashSequence.Append(fadeInSeq);

        // ------------------------------------------------------------
        // PHASE 2 — WARNING TIMELINE (ticks + scale + shake)
        // ------------------------------------------------------------

        float tickStep = pulseDuration + delayBetweenPulses;
        float warningDuration = warningPulseCount * tickStep;

        // SCALE + SHAKE start at time 0 of *this* phase
        if (leftCannon != null)
        {
            flashSequence.Insert(spriteFadeInDuration,
                leftCannon
                    .DOScale(leftCannonBaseScale * cannonFinalScale, warningDuration)
                    .SetEase(Ease.InOutSine)
            );

            flashSequence.Insert(spriteFadeInDuration, CreateEscalatingShake(leftCannon, warningDuration));
        }

        if (rightCannon != null)
        {
            flashSequence.Insert(spriteFadeInDuration,
                rightCannon
                    .DOScale(rightCannonBaseScale * cannonFinalScale, warningDuration)
                    .SetEase(Ease.InOutSine)
            );

            flashSequence.Insert(spriteFadeInDuration, CreateEscalatingShake(rightCannon, warningDuration));
        }

        // TICKS scheduled relative to phase start
        for (int i = 0; i < warningPulseCount; i++)
        {
            int pulseIndex = i;
            float t = i * tickStep;

            flashSequence.InsertCallback(
                fadeInSeq.Duration() + t,  // 🔥 offset by fade duration
                () =>
                {
                    AudioHelpers.PlaySoundEffect(
                        pulseSound,
                        Camera.main.transform.position,
                        1.0f + pulseIndex * 0.2f
                    );
                }
            );
        }

        // SNAP + CHARGE SOUND
        flashSequence.InsertCallback(fadeInSeq.Duration() + warningDuration, () =>
        {
            if (chargeSound != null)
            {
                AudioHelpers.PlayClipWithVariation(
                    chargeSound,
                    AudioChannel.SFX,
                    Camera.main.transform.position,
                    1.0f,
                    0.05f,
                    1.0f
                );
            }
        });

 

        // FIRE
        flashSequence.OnComplete(() =>
        {
            PlayCannonExit();
            PlayCannonEffects();

            if (beamPulse != null)
                beamPulse.Play(activeDuration);

            StartCoroutine(EnableColliderTemporarily());
        });

    }

    private void PlayCannonEffects()
    {
        if (cannonFireEffect != null)
        {
            if (leftCannonVFX != null)
            {
                Instantiate(
                    cannonFireEffect,
                    leftCannonVFX.position,
                    leftCannonVFX.rotation
                );
            }

            if (rightCannonVFX != null)
            {
                Instantiate(
                    cannonFireEffect,
                    rightCannonVFX.position,
                    rightCannonVFX.rotation
                );
            }
        }
    }

    private void PlayJustWait()
    {
        Sequence s = DOTween.Sequence();

        // 1) Pure wait
        if (waitDuration > 0f)
            s.AppendInterval(waitDuration);

        // 2) Fire destroy effect + sprite exit animation
        s.AppendCallback(() =>
        {
            PlayDestroyEffect();
        });

        // 3) Optional extra delay before completion
        if (completionDelay > 0f)
            s.AppendInterval(completionDelay);

        // 4) Mark exit done
        s.OnComplete(() =>
        {
            exitAnimationComplete = true;
        });

        return; 
    }

    private void PlayFadeOut()
    {
        Sequence s = DOTween.Sequence();

        float t = 0f;

        t += waitDuration;

        foreach (var sr in cannonSprites)
        {
            s.Insert(
                t,
                sr.DOFade(0f, exitFadeDuration).SetEase(Ease.OutQuad)
            );
        }

        foreach (var sr in hazardLineSprites)
        {
            s.Insert(
                t,
                sr.DOFade(0f, exitFadeDuration).SetEase(Ease.OutQuad)
            );
        }

        float destroyEffectTime = t + destroyEffectTimeOffset;

        s.InsertCallback(destroyEffectTime, () =>
        {
            PlayDestroyEffect();
        });

        t += exitFadeDuration + completionDelay;

        s.OnComplete(() =>
        {
            exitAnimationComplete = true;
        });

    }

    private void PlayAnimateThenFadeOut()
    {
        Sequence s = DOTween.Sequence();

        float t = 0f;

        // wait before animation
        t += waitDuration;

        s.InsertCallback(t, () =>
        {
            PlayDestroyEffect();
        });

        // wait before fade
        t += fadeOutDelay;

        foreach (var sr in cannonSprites)
        {
            s.Insert(
                t,
                sr.DOFade(0f, exitFadeDuration).SetEase(Ease.OutQuad)
            );
        }

 
        foreach (var sr in hazardLineSprites)
        {
            s.Insert(
                t,
                sr.DOFade(0f, exitFadeDuration).SetEase(Ease.OutQuad)
            );
        }
        
       

        // completion delay
        t += exitFadeDuration + completionDelay;

        s.OnComplete(() =>
        {
            exitAnimationComplete = true;
        });

    }


    private void PlayShrinkAndFadeOut()
    {
        Sequence s = DOTween.Sequence();

        float t = 0f;

        // Existing fade delay
        if (waitDuration > 0f)
            t += waitDuration;

        foreach (var sr in cannonSprites)
        {
            s.Insert(
                t,
                sr
                    .DOFade(0f, exitFadeDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        foreach (var sr in hazardLineSprites)
        {
            s.Insert(
                t,
                sr
                    .DOFade(0f, exitFadeDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        if (leftCannon != null)
        {
            s.Insert(
                t,
                leftCannon
                    .DOScale(Vector3.zero, exitShrinkDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        if (rightCannon != null)
        {
            s.Insert(
                t,
                rightCannon
                    .DOScale(Vector3.zero, exitShrinkDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        float destroyEffectTime = t + destroyEffectTimeOffset;

        s.InsertCallback(destroyEffectTime, () =>
        {
            PlayDestroyEffect();
        });

        // 3) Optional extra delay before completion
        if (completionDelay > 0f)
            s.AppendInterval(completionDelay);

        s.OnComplete(() =>
        {
            exitAnimationComplete = true;
        });
    }

    private void PlayLeaveScreen()
    {
        float slideDuration = 0.35f;

        if (leftCannon != null)
        {
            Sequence s = DOTween.Sequence();

            s.Append(
                leftCannon
                    .DOMoveX(leftCannon.position.x - recoilDistance, recoilDuration)
                    .SetEase(Ease.OutQuad)
            );

            s.Append(
                leftCannon
                    .DOMoveX(leftCannon.position.x - 3f, slideDuration)
                    .SetEase(Ease.InQuad)
            );

            s.Join(
                leftCannon
                    .DOScale(leftCannonBaseScale, recoilDuration + slideDuration)
            );

            s.OnComplete(() =>
            {
                exitAnimationComplete = true;
            });
        }

        if (rightCannon != null)
        {
            Sequence s = DOTween.Sequence();

            s.Append(
                rightCannon
                    .DOMoveX(rightCannon.position.x + recoilDistance, recoilDuration)
                    .SetEase(Ease.OutQuad)
            );

            s.Append(
                rightCannon
                    .DOMoveX(rightCannon.position.x + 3f, slideDuration)
                    .SetEase(Ease.InQuad)
            );

            s.Join(
                rightCannon
                    .DOScale(rightCannonBaseScale, recoilDuration + slideDuration)
            );

            s.OnComplete(() =>
            {
                exitAnimationComplete = true;
            });
        }
    }

    private void PlayDestroyEffect()
    {
        foreach (var spriteCyclyer in spriteCyclers)
            spriteCyclyer.PlayExitAnimation();

        if (destroyEffect != null)
        {
            Instantiate(
                destroyEffect,
                leftCannonVFX != null ? leftCannonVFX.position : transform.position,
                Quaternion.identity
            );
        }
    }



    private void PlayCannonExit()
    {
        switch (exitMode)
        {
            case ExitMode.LeaveScreen:
                PlayLeaveScreen();
                break;
            case ExitMode.ShrinkAndFadeOut:
                PlayShrinkAndFadeOut();
                break;
            case ExitMode.AnimateThenFadeOut:
                PlayAnimateThenFadeOut();
                break;
            case ExitMode.FadeOut:
                PlayFadeOut();
                break;
            case ExitMode.JustWait:
                PlayJustWait();
                break;
        }
    }







    // 🔧 Escalating shake helper
    private Sequence CreateEscalatingShake(Transform target, float totalTime)
    {
        Sequence s = DOTween.Sequence();

        int steps = Mathf.Max(1, Mathf.FloorToInt(totalTime / shakeStepDuration));
        float actualStepDuration = totalTime / steps;

        for (int i = 0; i < steps; i++)
        {
            float t = (steps == 1) ? 1f : (float)i / (steps - 1);
            float strength = Mathf.Lerp(minShakeStrength, maxShakeStrength, t);

            s.Append(
                target.DOShakePosition(
                    actualStepDuration,
                    strength,
                    shakeVibrato,
                    90,
                    false,
                    true
                )
            );
        }

        return s;
    }



    private IEnumerator EnableColliderTemporarily()
    {
        col.enabled = true;

        if (childRenderer != null)
        {
            childRenderer.SetActive(true);
            var childColor = childRenderer.GetComponent<SpriteRenderer>().color;
            childColor.a = 1f;
            childRenderer.GetComponent<SpriteRenderer>().color = childColor;
        }

        if (activationSound != null)
        {
            AudioHelpers.PlayClipWithVariation(
                activationSound,
                AudioChannel.SFX,
                Camera.main.transform.position,
                1.0f,
                0.1f,
                1.0f
            );
        }

        yield return new WaitForSeconds(activeDuration);

        col.enabled = false;

        if (childRenderer != null)
            childRenderer.GetComponent<SpriteRenderer>().DOFade(0f, 0.3f);

        hitLogicComplete = true;
    }

    private void OnDisable()
    {
        flashSequence?.Kill();
    }
}
