
using System.Collections;
using UnityEngine;

public class ContinuousWallShooter : WallShooterBase
{
    [Header("Continuous Fire Settings")]
    public float fireDelay = 5.0f;
    public int continuousFireCount = 3;

    protected override IEnumerator HandleChallengeSequence()
    {
        for(int i = 0; i < continuousFireCount; i++)
        {
            yield return StartCoroutine(FireSequence());
            yield return new WaitForSeconds(fireDelay);
        }
    }

    protected override void HandleImmediateFireMode()
    {
        SpawnProjectiles();

        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var proj = _projectiles[i];
            var fade = _spriteFade[i];

            if (proj != null)
                proj.StartCoroutine(proj.WindupAnim());

            if (fade != null && fade.gameObject != null)
                fade.StartCoroutine(fade.FadeSequence());

            _projectiles.RemoveAt(i);
            _spriteFade.RemoveAt(i);
        }
    }

    protected override void OnShooterBegin()
    {
        StartCoroutine(ChallengeSequence());
    }
}