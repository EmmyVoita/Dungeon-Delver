using System.Collections;
using UnityEngine;

public class PlaySoundEffectWithDelay : MonoBehaviour
{
    [SerializeField] private SoundEffect soundEffect;
    [SerializeField] private float delay;

    private void Start()
    {
        StartCoroutine(PlaySoundEffect());
    }

    private IEnumerator PlaySoundEffect()
    {
        yield return new WaitForSeconds(delay);

        AudioHelpers.PlaySoundEffect(soundEffect, transform.position);
    }
}