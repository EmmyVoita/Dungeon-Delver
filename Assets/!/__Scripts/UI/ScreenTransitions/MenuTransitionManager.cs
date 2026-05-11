using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;


public class MenuTransitionManager : MonoBehaviour
{
    public static MenuTransitionManager Instance { get; private set; }

    public event System.Action OnTransitionComplete;
    
    [Header("Transiton Definitions")]
    [SerializeField] private List<MenuStateTransitionSequence> sequences;

    private float currentValue = 0f;
    private Sequence currentSequence;
    private Material transitionMaterial;
    private bool isPlaying;
    private Dictionary<(MenuState, MenuState), MenuStateTransitionSequence> sequenceLookup;
    private MenuState pendingState;
    
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

    public void PlayTransition(MenuState from, MenuState to)
    {
        pendingState = to;

        if (sequenceLookup.TryGetValue((from, to), out var sequence))
        {
            PlaySequence(sequence);
        }
        else
        {
            MenuManager.Instance.TransitionToMenu(to);
            Debug.LogWarning($"No transition for {from} → {to}");
        }
    }




    private void PlaySequence(MenuStateTransitionSequence sequence)
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
                    MenuManager.Instance.OpenMenu(pendingState);
                    //GameStateManager.Instance.SetState(pendingState);
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
}