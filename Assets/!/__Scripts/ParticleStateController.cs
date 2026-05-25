using System.Collections.Generic;
using UnityEngine;

public class ParticleStateController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem_;
    
    [Header("Play During States")]
    [SerializeField] private List<GameState> activeStates;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        UpdateParticleState(
            GameStateManager.Instance.CurrentState
        );
    }

    private void HandleStateChanged(
        GameState previousState,
        GameState newState)
    {
        if(previousState == GameState.Paused || newState == GameState.Paused) return;
        UpdateParticleState(newState);
    }

    private void UpdateParticleState(GameState state)
    {
        bool shouldPlay = activeStates.Contains(state);

        if (shouldPlay)
        {
            if (!particleSystem_.isPlaying)
                particleSystem_.Play();
        }
        else
        {
            if (particleSystem_.isPlaying)
                particleSystem_.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
        }
    }
}