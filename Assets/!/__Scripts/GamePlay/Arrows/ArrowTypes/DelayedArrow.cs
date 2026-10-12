using UnityEngine;
using System.Collections;
using UnityEngine.VFX;


public class DelayedArrow : ArrowBase
{
    [Header("Delay Settings")]
    [Tooltip("Distance from player where the arrow will stop")]
    public float stopDistance = 2f;

    [Tooltip("How many beats the arrow will pause")]
    public float delayBeats = 2f;

    [Header("Optional")]
    public bool useManualStopDistance = true;

    [Header("Animation")]
    [SerializeField] private VisualEffect trailEffect;
    [SerializeField] private SpriteCyclerPerFrameTime spriteAnimator;
    [SerializeField] private SpriteCyclerPerFrameTime idleSpriteAnimator;


    [Header("Trail Colors")]
    [ColorUsage(true, true)] [SerializeField] private Color trailColorOutter;
    [ColorUsage(true, true)] [SerializeField] private Color trailColorInner;
    [ColorUsage(true, true)] [SerializeField] private Color timeTrailColorOutter;
    [ColorUsage(true, true)] [SerializeField] private Color timeTrailColorInner;

    private static readonly int OutterColor = Shader.PropertyToID("OutterColor");
    private static readonly int InnerColor = Shader.PropertyToID("InnerColor");

    private bool hasStopped = false;
    private Coroutine delayRoutine;
    private float stopTime;
    private float resumeTime;
    private Vector2 stopPos;
    private bool _stopped;
    private bool _resumed;



    public override void Init(Vector2 direction, float speed, float spawnTime, float arrivalTime, Vector3 startPos, Vector3 endPos)
    {
        base.Init(direction, speed, spawnTime, arrivalTime, startPos, endPos);

        float totalDistance = Vector2.Distance(startPos, endPos);

        float clampedStopDistance = Mathf.Min(totalDistance - stopDistance, totalDistance);
        float stopRatio = clampedStopDistance / totalDistance;

        stopTime = Mathf.Lerp(spawnTime, arrivalTime, stopRatio);

        float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
        float delayDuration = delayBeats * secondsPerBeat;

        resumeTime = stopTime + delayDuration;
        this._arrivalTime = delayDuration + arrivalTime;


        stopPos = Vector2.Lerp(startPos, endPos, stopRatio);

        trailEffect.Play();
        spriteAnimator.Play();
        _stopped = false;
        _resumed = false;
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
            trailEffect.SetVector4(OutterColor, trailColorOutter);
            trailEffect.SetVector4(InnerColor, trailColorInner);
        }
    }

    public override void SetEmpoweredArrow()
    {
        base.SetEmpoweredArrow();
        trailEffect.SetVector4(OutterColor, timeTrailColorOutter);
        trailEffect.SetVector4(InnerColor, timeTrailColorInner);
    }

    protected override void Update()
    {
        if (_isDead) return;

        float elapsed = (float)MusicManager.Instance.ScaledElapsedTime;

        Vector2 targetPos = Vector2.zero;

        if (elapsed < stopTime)
        {
            float t = Mathf.InverseLerp(_spawnTime, stopTime, elapsed);
            targetPos = Vector2.Lerp(_startPos, stopPos, t);
        }
        else if (elapsed < resumeTime)
        {
            if(!_stopped)
            {
                trailEffect.Stop();
                _stopped = true;
                spriteAnimator.Stop();
                idleSpriteAnimator.Restart();
            }

            //Debug.Log($"Stop Time => {stopTime}, Resume Time => {resumeTime}, Elapsed => {elapsed}");
            targetPos = stopPos;
        }
        else if(elapsed <= _arrivalTime)
        {
            if(!_resumed)
            {
                trailEffect.Play();
                _resumed = true;
                idleSpriteAnimator.Stop();
                spriteAnimator.Restart();
            }

            float t = Mathf.InverseLerp(resumeTime, _arrivalTime, elapsed);
            targetPos = Vector2.Lerp(stopPos, _endPos, t);
        }
        else
        {
            // AFTER goal → continue to center
            float extraTime = elapsed - _arrivalTime;

            float postTravelDuration = 0.2f; // tweak this

            float t = Mathf.Clamp01(extraTime / postTravelDuration);

            targetPos = Vector2.Lerp(_endPos, Vector2.zero, t);
        }

        SmoothTranslate(targetPos);
    }
}