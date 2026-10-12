using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.VFX;

public class GameWinAnimation : MonoBehaviour
{
    [SerializeField] private bool skipInEditor = true;

    //[Header("References")]
    //[SerializeField] private GuiPanelArrow healthVisuals;
    //[SerializeField] private Transform playerContainer;
    //[SerializeField] private GameObject landParticleEffect;

   // [SerializeField] private RectTransform starGlow;
//    [SerializeField] private float startDelay = 1.0f;
    //[SerializeField] private AnimationCurve healthFillCurve;
   // [SerializeField] private float startHealthAnimationDelay = 2.0f;
   // [SerializeField] private float healthFillDuration = 3.0f;
    //[SerializeField] private float secondClunkDelay = 1.0f;
    //[SerializeField] private float secondClunkPitchIncrease = 1.2f;
    //[SerializeField] private float playerIntroDelay = 1.0f;
    //[SerializeField] private float playerMoveDuration = 1.0f;
    //[SerializeField] private AnimationCurve playerMoveCurve;

    //[Header("Audio")]
    //[SerializeField] private AudioSource fallSoundSource;
    //[SerializeField] private SoundEffect clunkSound;
    //[SerializeField] private SoundEffect raiseHealthSound;
    //[SerializeField] private SoundEffect thudSound;
    //[SerializeField] private float healthPitchIncrease;

    [Header("References")]
    [SerializeField] private GameObject handObject;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform background;
    [SerializeField] private List<SpriteCyclerPerFrameTime> spriteAnimators;
    [SerializeField] private RectTransform gameOverRootRect;

    [Header("Main Slide")]
    [SerializeField] private Ease slideEase;
    [SerializeField] private float slideBackgroundMoveY = -5f;
    [SerializeField] private float slideHandMoveY = -10f;
    [SerializeField] private float slideDuration = 5f;
    [SerializeField] private float slideUpDelay = 3f;

    [Header("Player Charge")]
    [SerializeField] private float pulseStartDelay = 1f;
    [SerializeField] private float pulse1Scale = 1.15f;
    [SerializeField] private float pulse2Scale = 1.25f;
    [SerializeField] private float pulse3Scale = 1.4f;
    [SerializeField] private float pulseDuration = 0.2f;
    [SerializeField] private float pulseDelay = 0.2f;

    [Header("Launch")]
    [SerializeField] private float launchDelay = 0.15f;
    [SerializeField] private float launchMoveDelay = 0.15f;
    [SerializeField] private float launchY = 10f;
    [SerializeField] private float launchDuration = 1.2f;
    [SerializeField] private float launchSpins = 3f;

    [Header("Confetti")]
    [SerializeField] private int confettiAmount = 3;
    [SerializeField] private float confettiInterval = 0.75f;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffect chargeEffect;
    [SerializeField] private VisualEffect launchEffect;
    [SerializeField] private VisualEffect burstEffect;
    [SerializeField] private List<VisualEffect> starEffects;

    [Header("Audio")]
    [SerializeField] private float pulsePitchStep = 0.1f;
    [SerializeField] private SoundEffect pulseSound;
    [SerializeField] private SoundEffect launchSound;
    [SerializeField] private SoundEffect softChime;

    /*
    [Header("Hand Movement")]
    [SerializeField] private Vector3 handStartPosition = new Vector3(0,-5,0);
    [SerializeField] private float handMoveY = 5f;
    [SerializeField] private int stepCount = 10;
    [SerializeField] private float duration = 3f;

    [Header("Player Movement")]
    [SerializeField] private float playerMoveDelay = 3f;
    [SerializeField] private float playerMoveY = -5f;
    [SerializeField] private float playerMoveDuration = 3.0f;

    [Header("Audio")]
    [SerializeField] private SoundEffect softChime;
    [SerializeField] private SoundEffect handMove;

    [Header("Visual Effect")]
    
    */

    private Coroutine _animCoroutine;
    private Vector3 _backgroundBasPos;
    private float _currentPitchStep;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }
    private void Awake()
    {
        _backgroundBasPos = background.position;
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.M))
        {
            GameStateManager.Instance.SetStateForceUpdate(GameState.GameWin);
        }
        #endif
        
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.GameWin)
        {
            if(Application.isEditor)
            {
                if(skipInEditor)
                {
                   //playerContainer.position = new Vector3(0f,0f,0f);
                   //Player.Instance.HealPlayer(999);
                   //healthVisuals.Glow = true;
                   //starGlow.gameObject.SetActive(true);
                   //GameStateManager.Instance.SetState(GameState.RunLoad);
                } 
                else
                {
                    if(_animCoroutine != null)
                    {
                        StopCoroutine(_animCoroutine);
                        _animCoroutine = null;
                    }
                    
                    _animCoroutine = StartCoroutine(AnimSequence());
                }
            }
            else
            {
                if(_animCoroutine != null)
                {
                    StopCoroutine(_animCoroutine);
                    _animCoroutine = null;
                }
                
                _animCoroutine = StartCoroutine(AnimSequence());
            }              
        }
    }

    /*
    IEnumerator MoveStepped(
    Transform target,
    float distance,
    int steps,
    float totalTime)
    {
        Vector3 start = target.position;

        float stepDistance = distance / steps;
        float stepTime = totalTime / steps;

        for (int i = 1; i <= steps; i++)
        {
            target.position =
                start + Vector3.up * (stepDistance * i);

            AudioHelpers.PlaySoundEffect(handMove, handObject.transform.position);

            yield return new WaitForSeconds(stepTime);
        }
    }
    */

    /*
    private IEnumerator AnimSequence()
    {
        background.position = _backgroundBasPos;
        playerRoot.transform.position = Vector3.zero;

        foreach(VisualEffect effect in starEffects)
        {
            effect.Stop();
        }

        foreach(SpriteCyclerPerFrameTime animator in spriteAnimators)
        {
            animator.Restart();
        }
        

        Player.Instance.wings.ShowWings();
        Player.Instance.goal.GetComponentInChildren<Goal>().sRend.enabled = false;

        handObject.transform.position = handStartPosition;

        HoverAndSway hoverComp = playerRoot.GetComponentInChildren<HoverAndSway>();
        hoverComp.enabled = true;

        yield return StartCoroutine(
            MoveStepped(
                handObject.transform,
                handMoveY,
                stepCount,
                duration
            )
        );

        yield return new WaitForSeconds(playerMoveDelay);

        playerRoot.DOMoveY(playerMoveY, playerMoveDuration).OnComplete(() =>
        {
            AudioHelpers.PlaySoundEffect(softChime, Player.Instance.transform.position);

            foreach(VisualEffect effect in starEffects)
            {
                effect.Play();
            }

            foreach(SpriteCyclerPerFrameTime animator in spriteAnimators)
            {
                animator.Play();
            }
        });

        yield return new WaitForSeconds(slideUpDelay);

        playerRoot.DOMoveY(slideHandMoveY, slideDuration).SetEase(slideEase);
        handObject.transform.DOMoveY(slideHandMoveY, slideDuration).SetEase(slideEase);
        background.DOMoveY(slideBackgroundMoveY, slideDuration).SetEase(slideEase);

        ScreenShakeRequest ssRequest = new ScreenShakeRequest(duration: 1.0f,
                                                                magnitude: 0.1f,
                                                                direction: Vector2.up,
                                                                directional: true,
                                                                unscaled: true);

        yield return new WaitForSeconds(slideDuration + 1.0f);

        GameStateManager.Instance.SetState(GameState.GameOverTally);
    }
    */

    private IEnumerator AnimSequence()
    {
        playerRoot.position = Vector3.zero;
        playerRoot.localScale = Vector3.one;
        playerRoot.localRotation = Quaternion.identity;

        Player.Instance.wings.ShowWings();
        Player.Instance.goal.GetComponentInChildren<Goal>().sRend.enabled = false;

        HoverAndSway hoverComp = playerRoot.GetComponentInChildren<HoverAndSway>();
        hoverComp.enabled = true;

        AudioHelpers.PlaySoundEffect(softChime, Player.Instance.transform.position);

        foreach(VisualEffect effect in starEffects)
        {
            effect.Play();
        }

        yield return new WaitForSeconds(pulseStartDelay);

        

        yield return PulsePlayer(pulse1Scale);
        yield return new WaitForSeconds(pulseDelay);

        yield return PulsePlayer(pulse2Scale);
        yield return new WaitForSeconds(pulseDelay);

        yield return PulsePlayer(pulse3Scale);

        hoverComp.enabled = false;

        // Anticipation squash.
        yield return playerRoot.DOScale(new Vector3(1.25f, 0.7f, 1f), 0.12f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        

        yield return new WaitForSeconds(launchDelay);

        launchEffect?.Play();

        AudioHelpers.PlaySoundEffect(launchSound, Player.Instance.transform.position);

        Sequence launch = DOTween.Sequence();

        launch.Append(playerRoot.DOScale(new Vector3(0.8f, 1.25f, 1f), 0.08f));

        launch.Append(playerRoot.DOMoveY(launchY, launchDuration)
            .SetEase(Ease.InCubic));

        launch.Insert(launchMoveDelay, playerRoot.DORotate(
            new Vector3(0f, 360f * launchSpins, 0f),
            launchDuration,
            RotateMode.FastBeyond360
        ).SetEase(Ease.InQuad));
        
        launch.Join(background.DOMoveY(slideBackgroundMoveY, slideDuration).SetEase(slideEase).OnComplete(() =>
        {   
            gameOverRootRect.gameObject.SetActive(true);
            GameStateManager.Instance.SetState(GameState.GameOverTally);
        }));

        yield return launch.WaitForCompletion();

        burstEffect.transform.position = playerRoot.position;
        burstEffect?.Play();

        for(int i = 0; i < confettiAmount; i++)
        {
            ConfettiEffect.TriggerConfetti();
            yield return new WaitForSeconds(confettiInterval);
        }
        

        
        

        //AudioHelpers.PlaySoundEffect(softChime, transform.position);

        //yield return new WaitForSeconds(0.75f);

        
    }

    private IEnumerator PulsePlayer(float targetScale)
    {
        Sequence pulse = DOTween.Sequence();

        chargeEffect?.Play();

        float pitchMult = 1.0f + _currentPitchStep;
        AudioHelpers.PlaySoundEffect(pulseSound, Player.Instance.transform.position, pitchMult);

        pulse.Append(playerRoot.DOScale(targetScale, pulseDuration * 0.5f).SetEase(Ease.OutQuad));
        pulse.Append(playerRoot.DOScale(1f, pulseDuration * 0.5f).SetEase(Ease.InOutQuad));

        _currentPitchStep += pulsePitchStep;

        yield return pulse.WaitForCompletion();
    }

    /*
    private IEnumerator MovePlayer()
    {
        fallSoundSource.Play();
        Tween tween = playerContainer.DOMoveY(0f,1.0f)
        .SetEase(playerMoveCurve)
        .OnComplete(()=>
        {
            fallSoundSource.Stop();
        });

        yield return tween.WaitForCompletion();
    }

    private IEnumerator FillHealthBar()
    {
        int startHealth = Player.Instance.Health;
        int targetHealth = Player.Instance.MaxHealth;

        Tween tween = DOTween.To(
            () => 0,
            value =>
            {
                int desired = startHealth + value;
                int delta = desired - Player.Instance.Health;

                if (delta > 0)
                {
                    float pitchMult = 1.0f + healthPitchIncrease * value;
                    Player.Instance.HealPlayer(delta, false);
                    AudioHelpers.PlaySoundEffect(raiseHealthSound, transform.position,pitchMult);
                }
            },
            targetHealth - startHealth,
            healthFillDuration
        ).SetEase(healthFillCurve);

        yield return tween.WaitForCompletion();

      
    }
    */
   
}