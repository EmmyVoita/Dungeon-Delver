using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BurstWallShooter : WallShooterBase
{
    [Header("Oscillation")]
    public Ease easeCurve = Ease.OutQuad;    
    public float startWindupDelay = 0.0f;
    public float oscilationXSpeed = 3.0f;
    

    [Header("Audio")]
    [SerializeField] private SoundEffect shiftStartSound;


    protected override IEnumerator HandleChallengeSequence()
    {
        yield return StartCoroutine(FireSequence());
    }
     
    protected override void HandleImmediateFireMode()
    {
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

    protected IEnumerator OscilateProjectiles()
    {
        SpawnProjectiles();
        
        int targetIndex = Mathf.Clamp(Random.Range(0, _projectiles.Count-2) + 1, 0, _projectiles.Count-1);

        Transform target = _projectiles[targetIndex].transform;

        float centerX = transform.position.x;
        float delta = centerX - target.position.x; // how far target must move

        Sequence s = DOTween.Sequence();

        if(settingsData.projectileFireMode == ProjectileFireMode.MoveBackForth)
        {
            AudioHelpers.PlaySoundEffect(shiftStartSound, Camera.main.transform.position);

            // Perpendicular axis to fire direction
            Vector2 perpAxis = new Vector2(-projectileFireDirection.y, projectileFireDirection.x).normalized;

            // How far target must move along that axis to align with shooter center
            Vector3 toCenter = transform.position - target.position;
            float deltaA = Vector3.Dot(toCenter, perpAxis);


            for (int i = 0; i < _projectiles.Count; i++)
            {
                Transform proj = _projectiles[i].transform;

                Vector3 startPos = proj.position;
                Vector3 targetPos = startPos + (Vector3)perpAxis * deltaA;

                s.Insert(0f,proj.DOMove(targetPos, oscilationXSpeed).SetEase(easeCurve));
            }

            s.AppendInterval(startWindupDelay);

            s.OnComplete(() =>
            {
                StartCoroutine(ChallengeSequence());
            });

            yield return s.WaitForCompletion();
        }
        else
        {
            StartCoroutine(ChallengeSequence());
        }
    }

    protected override void OnShooterBegin()
    {
        StartCoroutine(OscilateProjectiles());
    }
}