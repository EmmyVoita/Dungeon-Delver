using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using System.Collections;

public class PlayerDeathEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSrend;
    [SerializeField] private Transform heart;
    [SerializeField] private VisualEffect rainbowBurst;

    [Header("Timing")]
    [SerializeField] private float scaleDuration = 0.35f;
    [SerializeField] private float endScale = 3f;

    [SerializeField] private float vfxDelay = 0.4f;

    [Header("Shake")]
    [SerializeField] private float shakeStrength = 0.12f;
    [SerializeField] private int shakeVibrato = 20;

    [Header("Audio")]
    [SerializeField] private SoundEffect destroySound;
    [SerializeField] private SoundEffect finalSound;
    [SerializeField] private int audioLoopCount = 8;
    [SerializeField] private float audioLoopInterval = 0.8f;
    [SerializeField] private float pitchStep = 0.1f;


    private Vector3 heartStartPos;
    private bool isPlaying;

    private void Awake()
    {
        heartStartPos = heart.localPosition;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    // Debug trigger
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            GameStateManager.Instance.SetState(GameState.DeathSequence);
        }
    }

    void HandleStateChanged(GameState previousState, GameState newState)
    {
        if (newState == GameState.DeathSequence && previousState != newState)
        {
            PlayDeathAnimation();
            StartCoroutine(HandleAudio());
        }
    }

    private IEnumerator HandleAudio()
    {
        for(int i = 0; i < audioLoopCount; i++)
        {
            float pitchMult = 1.0f + i * pitchStep;
            AudioHelpers.PlaySoundEffect(destroySound, transform.position,pitchMult);
            yield return new WaitForSeconds(audioLoopInterval);
        }
        AudioHelpers.PlaySoundEffect(finalSound, transform.position);
        playerSrend.color = Color.clear;
    }

    void PlayDeathAnimation()
    {
        if (isPlaying)
            return;

        isPlaying = true;

        // Reset heart
        heart.DOKill();
        heart.localScale = Vector3.one;
        heart.localPosition = heartStartPos;

        Sequence seq = DOTween.Sequence();

        if (rainbowBurst != null)
                rainbowBurst.Play();

        // Heart grows
        seq.Append(
            heart.DOScale(endScale, scaleDuration)
            .SetEase(Ease.OutBack)
        );

        // Heart shake during scale
        seq.Join(
            heart.DOShakePosition(
                scaleDuration,
                shakeStrength,
                shakeVibrato,
                90,
                false,
                false
            )
        );

        // Wait for burst to play
        seq.AppendInterval(vfxDelay);

        // Transition to GameOver
        seq.AppendCallback(() =>
        {
            GameStateManager.Instance.SetState(GameState.GameOverTally);
        });
    }
}