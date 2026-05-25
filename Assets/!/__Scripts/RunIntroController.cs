using UnityEngine;
using System.Collections;
using DG.Tweening;

public class RunIntroController : MonoBehaviour
{
    [SerializeField] private bool skipInEditor = true;
    [SerializeField] private bool stop = false;
    [Header("References")]
    [SerializeField] private GuiPanelArrow healthVisuals;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private GameObject landParticleEffect;

    [SerializeField] private RectTransform starGlow;
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private AnimationCurve healthFillCurve;
    [SerializeField] private float startHealthAnimationDelay = 2.0f;
    [SerializeField] private float healthFillDuration = 3.0f;
    [SerializeField] private float secondClunkDelay = 1.0f;
    [SerializeField] private float secondClunkPitchIncrease = 1.2f;
    [SerializeField] private float playerIntroDelay = 1.0f;
    [SerializeField] private float playerMoveDuration = 1.0f;
    [SerializeField] private AnimationCurve playerMoveCurve;

    [Header("Audio")]
    [SerializeField] private AudioSource fallSoundSource;
    [SerializeField] private SoundEffect clunkSound;
    [SerializeField] private SoundEffect raiseHealthSound;
    [SerializeField] private SoundEffect thudSound;
    [SerializeField] private float healthPitchIncrease;
    private Coroutine introCoroutine;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(ObstacleManager.Instance.TestOn || stop) return;
        
        if(newState == GameState.RunIntro)
        {
            if(Application.isEditor)
            {
                if(skipInEditor)
                {
                   playerContainer.position = new Vector3(0f,0f,0f);
                   Player.Instance.HealPlayer(999);
                   healthVisuals.Glow = true;
                   starGlow.gameObject.SetActive(true);
                   GameStateManager.Instance.SetState(GameState.RunLoad);
                } 
                else
                {
                     introCoroutine = StartCoroutine(IntroSequence());
                }
            }
            else
            {
                introCoroutine = StartCoroutine(IntroSequence());
            }              
        }
    }

    private IEnumerator IntroSequence()
    {
        starGlow.gameObject.SetActive(false);
        Player.Instance.Health = 0;
        playerContainer.position = new Vector3(0f,6f,0f);
        healthVisuals.Glow = false;

        yield return new WaitForSeconds(startDelay);
        // Make everything light up
        // first make the energy star light up maybe play a clunk sound
        AudioHelpers.PlaySoundEffect(clunkSound, transform.position);

        ScreenShakeRequest ssRequest = new ScreenShakeRequest(duration: 1.0f,
                                                                magnitude: 0.1f,
                                                                direction: Vector2.up,
                                                                directional: true,
                                                                unscaled: true);

        ScreenShakeManager.Instance.Shake(ssRequest);

        starGlow.gameObject.SetActive(true);

        yield return new WaitForSeconds(startHealthAnimationDelay);

        //

        //

        //yield return new WaitForSeconds(secondClunkDelay);

        // then fill the health bar
        yield return StartCoroutine(FillHealthBar());

        yield return new WaitForSeconds(0.25f);
        healthVisuals.Glow = true;
        ScreenShakeManager.Instance.Shake(ssRequest);
        AudioHelpers.PlaySoundEffect(clunkSound, transform.position, secondClunkPitchIncrease);



        

       

        yield return new WaitForSeconds(playerIntroDelay);

        // then maybe play like a powering up sound?
         //
        yield return StartCoroutine(MovePlayer());

        AudioHelpers.PlaySoundEffect(thudSound, transform.position);

        ScreenShakeManager.Instance.Shake(ssRequest);

        if(landParticleEffect != null)
        {
            Instantiate(landParticleEffect, playerContainer.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(1.5f);

        GameStateManager.Instance.SetState(GameState.RunLoad);

        // then drop the player in and play screen shape with another cluck sound when they fall and hit the center.
        // I think here we have a falling sound and then the thud sound
    }

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

   
}