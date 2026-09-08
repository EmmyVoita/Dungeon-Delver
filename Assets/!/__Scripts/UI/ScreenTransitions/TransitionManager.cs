using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;




public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    public System.Action OnTransitionComplete;

    [SerializeField] private List<GameStateTransitionSequence> sequences;
    [SerializeField] private TransitionStep shutterTransitionOpen;
    [SerializeField] private TransitionStep shutterTransitionClose;

    //[SerializeField] private string transitionProperty = "_WipeTransitionValue";
    

    //[SerializeField] private float duration = 0.4f;
    //[SerializeField] private Ease ease = Ease.OutCubic;

    private float currentValue = 0f;
    private Sequence currentSequence;
    private Material transitionMaterial;
        private bool isPlaying;


    public bool IsPlayingTransition => isPlaying;

    private Dictionary<(GameState, GameState), GameStateTransitionSequence> sequenceLookup;

    private GameState pendingState;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        sequenceLookup = new();

        foreach (var seq in sequences)
        {
            sequenceLookup[(seq.from, seq.to)] = seq;
        }
    }

    void Start()
    {
        currentValue = 0f;
    }

    public void PlayTransition(GameState from, GameState to)
    {
        pendingState = to;

        if (sequenceLookup.TryGetValue((from, to), out var sequence))
        {
            PlaySequence(sequence);
        }
        else
        {
            Debug.LogWarning($"No transition for {from} → {to}");
        }
    }

 
    

    private void PlaySequence(GameStateTransitionSequence sequence, Action halfWayCallback = null)
    {
        if (isPlaying) return;
        isPlaying = true;

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();

        foreach (var step in sequence.steps)
        {
            var stepCopy = step;

            currentSequence.AppendCallback(() =>
            {
                transitionMaterial = stepCopy.material;

                currentValue = stepCopy.startValue;
                transitionMaterial.SetFloat(stepCopy.transitionProperty, currentValue);

                if (stepCopy.triggerStateSwitch)
                {
                    halfWayCallback?.Invoke();
                    GameStateManager.Instance.SetState(pendingState);
                }
            });

            currentSequence.Append(
                DOTween.To(
                    () => currentValue,
                    x =>
                    {
                        currentValue = x;
                        transitionMaterial.SetFloat(stepCopy.transitionProperty, currentValue);
                    },
                    stepCopy.targetValue,
                    stepCopy.duration
                ).SetEase(stepCopy.ease)
            );

            if (stepCopy.holdTime > 0)
                currentSequence.AppendInterval(stepCopy.holdTime);
        }

        currentSequence.OnComplete(() =>
        {
            isPlaying = false;
            OnTransitionComplete?.Invoke();
        });
    }


    // Basic Shutter open and close
    //--------------------------------------------------------------------------------------
    public IEnumerator PlayOpenShutter()
    {
        if (isPlaying)
            yield break;

        PlayTransitionStepSequence(shutterTransitionOpen);

        yield return new WaitUntil(() => !isPlaying);
    }

    public IEnumerator PlayCloseShutter()
    {
        if (isPlaying)
            yield break;

        PlayTransitionStepSequence(shutterTransitionClose);

        yield return new WaitUntil(() => !isPlaying);
    }

    private void PlayTransitionStepSequence(TransitionStep transitionStep)
    {
        if (isPlaying) return;
        isPlaying = true;

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();


        var stepCopy = transitionStep;

        currentSequence.AppendCallback(() =>
        {
            transitionMaterial = stepCopy.material;

            currentValue = stepCopy.startValue;
            transitionMaterial.SetFloat(stepCopy.transitionProperty, currentValue);
        });

        currentSequence.Append(
            DOTween.To(
                () => currentValue,
                x =>
                {
                    currentValue = x;
                    transitionMaterial.SetFloat(stepCopy.transitionProperty, currentValue);
                },
                stepCopy.targetValue,
                stepCopy.duration
            ).SetEase(stepCopy.ease)
        );

        if (stepCopy.holdTime > 0)
            currentSequence.AppendInterval(stepCopy.holdTime);
    

        currentSequence.OnComplete(() =>
        {
            isPlaying = false;
            OnTransitionComplete?.Invoke();
        });
    }
}