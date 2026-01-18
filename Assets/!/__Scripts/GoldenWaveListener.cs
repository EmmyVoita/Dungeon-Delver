using UnityEngine;

public class GoldenWaveListener
{
    private int arrowsCaught = 0;

    public GoldenWaveListener()
    {
        ArrowBase.OnArrowResolved += OnArrowResolved;
        SlowTimeAbilityObject.OnSlowTimeEnded += OnSlowTimeEnded;
        SlowTimeAbilityObject.OnSlowTimeStarted += OnSlowTimeStarted;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnSlowTimeStarted(SlowTimeAbilityObject ability)
    {
        arrowsCaught = 0;
    }

    private void OnArrowResolved(ArrowResolvedData data)
    {
        arrowsCaught++;
    }

    private void OnSlowTimeEnded(SlowTimeAbilityObject ability)
    {
        BuffHelpers.OnGoldenArrowSessionStarted?.Invoke();
        
        float t = Mathf.Clamp01(arrowsCaught / (float)ability.maxCountForFullRange);
        float radius = Mathf.Lerp(ability.minRange, ability.maxRange, t);

        if (ability.schockwavePrefab != null)
        {
            GameObject shockwave = GameObject.Instantiate(
                ability.schockwavePrefab,
                Player.Instance.transform.position,
                Quaternion.identity
            );

            shockwave.GetComponent<ShockwaveEffect>()?.Initialize(radius);
        }
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if (newState != GameState.RoundActive)
        {
            Cleanup();
        }
    }

    private void Cleanup()
    {
        ArrowBase.OnArrowResolved -= OnArrowResolved;
        SlowTimeAbilityObject.OnSlowTimeEnded -= OnSlowTimeEnded;
    }
}
