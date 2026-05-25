using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;

public class ConfettiEffect : MonoBehaviour
{
    public static event Action OnPlayConfetti;

    [Header("References")]
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private SoundEffect popSound;

    [Header("Settings")]
    [SerializeField] private bool playOnEnable = false;

    [Header("Random Delay")]
    [SerializeField] private float minDelay = 0f;
    [SerializeField] private float maxDelay = 0.25f;

    private Coroutine playRoutine;

    private void OnEnable()
    {
        OnPlayConfetti += Play;

        if (playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        OnPlayConfetti -= Play;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }
    }

    public void Play()
    {
        if (visualEffect == null)
            return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PlayRoutine());
    }

    public static void TriggerConfetti()
    {
        OnPlayConfetti?.Invoke();
    }

    IEnumerator PlayRoutine()
    {
        float delay = UnityEngine.Random.Range(minDelay, maxDelay);

        yield return new WaitForSeconds(delay);

        visualEffect.Stop();
        visualEffect.Play();

        AudioHelpers.PlaySoundEffect(popSound, transform.position);

        playRoutine = null;
    }

  
}
