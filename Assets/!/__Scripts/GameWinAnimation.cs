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

    [Header("Main Slide")]
    [SerializeField] private Ease slideEase;
    [SerializeField] private float slideBackgroundMoveY = -5f;
    [SerializeField] private float slideHandMoveY = -10f;
    [SerializeField] private float slideDuration = 5f;
    [SerializeField] private float slideUpDelay = 3f;


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
    [SerializeField] private List<VisualEffect> starEffects;

    private Coroutine _animCoroutine;
    private Vector3 _backgroundBasPos;
    

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
        if(Input.GetKeyDown(KeyCode.M))
        {
            GameStateManager.Instance.SetStateForceUpdate(GameState.GameWin);
        }
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
            animator.Reset();
        }
        
        //Player.Instance.SetPlayerControlState(PlayerControlState.BasicJump);
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

        //starGlow.gameObject.SetActive(false);
        //Player.Instance.Health = 0;
        //playerContainer.position = new Vector3(0f,6f,0f);
        //healthVisuals.Glow = false;

        //yield return new WaitForSeconds(startDelay);
        // Make everything light up
        // first make the energy star light up maybe play a clunk sound
        //AudioHelpers.PlaySoundEffect(clunkSound, transform.position);

        

        ScreenShakeRequest ssRequest = new ScreenShakeRequest(duration: 1.0f,
                                                                magnitude: 0.1f,
                                                                direction: Vector2.up,
                                                                directional: true,
                                                                unscaled: true);

        //ScreenShakeManager.Instance.Shake(ssRequest);

        //starGlow.gameObject.SetActive(true);

        yield return new WaitForSeconds(slideDuration + 1.0f);

        GameStateManager.Instance.SetState(GameState.GameOverTally);

        //

        //

        //yield return new WaitForSeconds(secondClunkDelay);

        // then fill the health bar
        //yield return StartCoroutine(FillHealthBar());

        //yield return new WaitForSeconds(0.25f);
        //healthVisuals.Glow = true;
        //ScreenShakeManager.Instance.Shake(ssRequest);
        //AudioHelpers.PlaySoundEffect(clunkSound, transform.position, secondClunkPitchIncrease);



        

       

        //yield return new WaitForSeconds(playerIntroDelay);

        // then maybe play like a powering up sound?
         //
        //yield return StartCoroutine(MovePlayer());

        //AudioHelpers.PlaySoundEffect(thudSound, transform.position);

        //ScreenShakeManager.Instance.Shake(ssRequest);

        /*
        if(landParticleEffect != null)
        {
            Instantiate(landParticleEffect, playerContainer.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(1.5f);

        GameStateManager.Instance.SetState(GameState.RunLoad);

        // then drop the player in and play screen shape with another cluck sound when they fall and hit the center.
        // I think here we have a falling sound and then the thud sound
        */
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