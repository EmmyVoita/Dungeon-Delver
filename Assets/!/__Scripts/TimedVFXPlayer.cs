using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class TimedVFXPlayer : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;

    private Coroutine activeRoutine;

    public void PlayForDuration(float duration)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(PlayRoutine(duration));
    }

    private IEnumerator PlayRoutine(float duration)
    {
        visualEffect.Play();

        yield return new WaitForSeconds(duration);

        // Stops spawning but keeps existing particles alive
        visualEffect.Stop();

        activeRoutine = null;
    }
}