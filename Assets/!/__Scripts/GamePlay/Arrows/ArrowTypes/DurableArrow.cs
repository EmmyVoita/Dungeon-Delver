using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.VFX;

public class DurableArrow : ArrowBase
{
    [Header("Bounce Settings")]
    [Tooltip("How many beats the whole out-and-back bounce should last.")]
    public float bounceTimeBeats = 4f;

    [Tooltip("How far (in world units) the arrow moves away at the peak of the bounce.\n" +
             "If <= 0, it will be derived from speed & duration.")]
    public float maxBounceDistance = 0f;

    public AudioClip bounceSound;

    [Header("Durability Settings")]
    public int hitsRequired = 2;
    public float invincibilityDuration = 0.8f;

    [Header("Runtime")]
    [SerializeField] private int hitsTaken = 0;
    [SerializeField] private float invincibleDone = 0;

    [Header("Animation")]
    [SerializeField] private VisualEffect trailEffect;

    [Header("Trail Colors")]
    [ColorUsage(true, true)] [SerializeField] private Color goldTrailColorOutter;
    [ColorUsage(true, true)] [SerializeField] private Color goldTrailColorInner;

    [ColorUsage(true, true)] [SerializeField] private Color timeTrailColorOutter;
    [ColorUsage(true, true)] [SerializeField] private Color timeTrailColorInner;

    private static readonly int OutterColor = Shader.PropertyToID("OutterColor");
    private static readonly int InnerColor = Shader.PropertyToID("InnerColor");

    private Coroutine bounceRoutine;

    private float bounceStartTime;
    private float bounceEndTime;
    private Vector2 bounceStartPos;
    private float bounceDistance;
    private bool isBouncing = false;
    private bool _isBouncingToMid = false;

    private float BounceMidTime => bounceStartTime + (bounceEndTime - bounceStartTime) / 2f;


    protected override void Update()
    {
        #if UNITY_ENGINE
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("MANUAL VFX START");

            trailEffect.Reinit();
            trailEffect.Play();
        }
        #endif

        if (_invincible && Time.time > invincibleDone)
            _invincible = false;

        if (_isDead) return;

        float elapsed = (float)MusicManager.Instance.ScaledElapsedTime;

        // --- BOUNCE PHASE ---
        if (isBouncing)
        {
            float t = Mathf.InverseLerp(bounceStartTime, bounceEndTime, elapsed);
            t = Mathf.Clamp01(t);

            float h = 4f * t * (1f - t); // parabola

            Vector2 awayDir = _direction.normalized;

            Vector2 targetPos = bounceStartPos + awayDir * (h * bounceDistance);

            SmoothTranslate(targetPos);

            if(elapsed >= BounceMidTime && _isBouncingToMid)
            {
                trailEffect.Reinit();
                trailEffect.Play();
                _isBouncingToMid = false;
            }


            if (elapsed >= bounceEndTime)
            {
                isBouncing = false;

                // IMPORTANT: shift arrow timeline forward
                float delay = bounceEndTime - bounceStartTime;
                _spawnTime += delay;
                _arrivalTime += delay;
            }

            return;
        }

        // --- NORMAL MOVEMENT ---
        base.Update();
    }

    private void Squish()
    {
        sRend.transform.DOKill();

        sRend.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            sRend.transform.DOScale(
                new Vector3(0.5f, 1.6f, 1f), // squash X, stretch Y
                0.06f
            )
        );

        seq.Append(
            sRend.transform.DOScale(
                new Vector3(1.15f, 0.85f, 1f), // overshoot opposite
                0.08f
            )
            .SetEase(Ease.OutQuad)
        );

        seq.Append(
            sRend.transform.DOScale(
                Vector3.one,
                0.15f
            )
            .SetEase(Ease.OutBack)
        );
    }


    public override void Init(Vector2 direction, float speed, float spawnTime, float arrivalTime, Vector3 startPos, Vector3 endPos)
    {
        base.Init(direction, speed, spawnTime, arrivalTime, startPos, endPos);
        
        float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
        float duration = bounceTimeBeats * secondsPerBeat;

        float secondArrivalTime = arrivalTime + duration;

        trailEffect.Play();
    }

    public override void SetGolden()
    {
        AddStatus(ArrowStatus.Golden);
        
        if(!runTimeMaterial)
        {
            Debug.LogError("When trying to set arrow to golden, the runtime material was null");
            return;
        } 
            

        if(!HasStatus(ArrowStatus.Recovery) && !HasStatus(ArrowStatus.Empowered))
        {
            runTimeMaterial.SetColor(colorApropertyName, goldenColorA);
            runTimeMaterial.SetColor(colorBpropertyName, goldenColorB);
            trailEffect.SetVector4(OutterColor, goldTrailColorOutter);
            trailEffect.SetVector4(InnerColor, goldTrailColorInner);
        }
    }

    public override void SetEmpoweredArrow()
    {
        base.SetEmpoweredArrow();

        trailEffect.SetVector4(OutterColor, timeTrailColorOutter);
        trailEffect.SetVector4(InnerColor, timeTrailColorInner);
    }

    public override void OnArrowHit(float damage = 1f,
                                    Goal.GoalType goalType = Goal.GoalType.Normal,
                                    Vector2 hitDirection = default)
    {
        if (_invincible) return;

        hitsTaken++;

        if (hitsTaken >= hitsRequired)
        {
            Die(goalType, true, hitDirection);
        }
        else
        {
            Squish();
            trailEffect.Stop();
            _invincible = true;
            invincibleDone = Time.time + invincibilityDuration;

            base.PlayAudio(goalType);

            //if (bounceRoutine != null)
               // StopCoroutine(bounceRoutine);

            //bounceRoutine = StartCoroutine(BounceParabola());

            float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
            float duration = bounceTimeBeats * secondsPerBeat;

            bounceStartTime = (float)MusicManager.Instance.ScaledElapsedTime;
            bounceEndTime = bounceStartTime + duration;

            bounceStartPos = transform.localPosition;

            bounceDistance = maxBounceDistance > 0f
                ? maxBounceDistance
                : _speed * (duration * 0.5f);

            isBouncing = true;
            _isBouncingToMid = true;

        }
    }
}
