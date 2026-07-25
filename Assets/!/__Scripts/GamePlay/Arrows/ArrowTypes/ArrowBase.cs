using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering;

public abstract class ArrowBase : MonoBehaviour
{
    public static event Action<ArrowResolvedData> OnArrowResolved;

    [Header("Arrow Stats")]
    [SerializeField] private float health = 1f;


    [Header("Type Settings")]
    [SerializeField] private ArrowCatchRule catchRule = ArrowCatchRule.Catch;


    [Header("Sprite")]
    [SerializeField] private bool flip180 = false;
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private Material baseMaterial;
    [SerializeField] protected string colorApropertyName = "_ColorA";
    [SerializeField] protected string colorBpropertyName = "_ColorB";

    [Header("Flag Colors")]
    [ColorUsage(true, true)][SerializeField] protected Color goldenColorA;
    [ColorUsage(true, true)][SerializeField] protected Color goldenColorB;
    [ColorUsage(true, true)][SerializeField] protected Color recoveryColorA;
    [ColorUsage(true, true)][SerializeField] protected Color recoveryColorB;

    [Header("Slow Flag")]
    [SerializeField] private float slowFactor = 0.7f;
    [SerializeField] private float slowDuration = 2f;



    [Header("Audio Base Settings")]
    [SerializeField] private AudioClip arrowHitSound;
    [SerializeField] private float normalHitPitch = 1.0f;
    [SerializeField] private float criticalHitPitch = 1.2f;
    [Range(0f, 1f),SerializeField] private float directionalPitchFactor = 1f;
    [Range(0f, 1f)] public float directionalEQStrength = 0.5f;

    [Header("VFX")]
    [SerializeField] private GameObject killEffect;


    [Header("Dynamic")]
    [SerializeField] protected bool _invincible = false;
    [SerializeField] private ArrowStatus _status;


    protected Vector2 _direction;
    protected float _speed;
    protected Rigidbody2D _rb;
    protected bool _isDead = false;
    protected float _spawnTime;
    protected float _arrivalTime;
    protected Vector2 _startPos;
    protected Vector2 _endPos;
    private ArrowType _arrowType = ArrowType.Normal;
    protected Material runTimeMaterial;


    public bool IsGolden => _status.HasFlag(ArrowStatus.Golden);
    public bool IsRecoveryArrow => _status.HasFlag(ArrowStatus.Recovery);
    public bool IsTimeSlowArrow => _status.HasFlag(ArrowStatus.TimeSlow);
    public bool Invincible => _invincible;
    public ArrowCatchRule CatchRule => catchRule;
    public bool IsInverse => catchRule == ArrowCatchRule.Avoid;
    public Vector2 Direction => _direction;
    public bool IsDead => _isDead;



    void OnEnable()
    {
        ArrowSpawner.OnClearArrows += GameOverEffect;   
        UIManager.OnGameOver += GameOverEffect;
        GameStateManager.OnStateChanged += HandleRoundEnd;
    }
    
    void OnDisable()
    {
        ArrowSpawner.OnClearArrows -= GameOverEffect;
        UIManager.OnGameOver -= GameOverEffect;
        GameStateManager.OnStateChanged -= HandleRoundEnd;
    }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        ArrowManager.Instance.RegisterArrow(this);

        if(sRend != null && baseMaterial != null)
        {
            runTimeMaterial = Instantiate(baseMaterial);
            sRend.material = runTimeMaterial;
        }
    }

    protected virtual void Update()
    {
        // We dont want to set the position here if this is an editor arrow. 
        // We manually calculate the arrow position in a seperate script for editor arrows.
        // We include the _isDead here to make sure we dont continue to run movement logic 
        // for arrows marked for death.
        if(_arrowType == ArrowType.Editor || _isDead) return;

        float elapsed = (float)MusicManager.Instance.ScaledElapsedTime;

        Vector2 targetPos;

        if (elapsed <= _arrivalTime)
        {
            // Normal movement to goal
            float t = Mathf.InverseLerp(_spawnTime, _arrivalTime, elapsed);
            targetPos = Vector2.Lerp(_startPos, _endPos, t);
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

    protected virtual void OnDestroy()
    {
        ArrowManager.Instance.UnregisterArrow(this);
    }

    // Public
    // ----------------------------------------------------------------------------------------

    // Initialize arrow direction and movement
    public virtual void Init(Vector2 direction, float speed, float spawnTime, float arrivalTime, Vector3 startPos, Vector3 endPos)
    {
        _direction = direction.normalized;
        _speed = speed;
        _spawnTime = spawnTime;
        _arrivalTime = arrivalTime;
        _startPos = startPos;
        _endPos = endPos;

        OrientArrow(direction);
    }

    // Called when something hits the arrow (not necessarily killing it)
    public virtual void OnArrowHit(float damage = 1f, Goal.GoalType goalType = Goal.GoalType.Normal, Vector2 hitDirection = default)
    {
        if (_invincible) return;

        health -= damage;

        if (health <= 0f)
            Die(goalType, hitDirection: hitDirection);

    }

    public virtual void KillArrow(Goal.GoalType goalType = Goal.GoalType.Miss, bool playKillEffect = false)
    {
        // Instantly die, bypassing health
        health = 0f;
        // Miss type since killed externally
        Die(goalType, true); 

        if(playKillEffect && killEffect)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
        {
            Instantiate(killEffect, transform.position, Quaternion.identity);
        }
    }

    public virtual void OrientArrow(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float offset = flip180 ? 90f : -90f;
        transform.rotation = Quaternion.Euler(0, 0, angle + offset);
    }



    // Core Internal
    // ----------------------------------------------------------------------------------------

    protected virtual void Die(Goal.GoalType goalType = Goal.GoalType.Normal, bool invokeDeathEvent = true, Vector2 hitDirection = default)
    {   
        if(_isDead) return;

        _isDead = true;

        if (invokeDeathEvent)
        {
            ArrowResolvedData data = new ArrowResolvedData
            {
                goalType = goalType,
                status = this._status
            };
            
            OnArrowResolved?.Invoke(data);  
        }

        // Recovery Arrows only heal when caught critically.
        if (goalType == Goal.GoalType.Critical &&
            HasStatus(ArrowStatus.Recovery))
        {
            Player.Instance.HealPlayer(1);
        }

        if (goalType == Goal.GoalType.Critical &&
            HasStatus(ArrowStatus.TimeSlow))
        {
            TimeScaleModifier timeMod = new TimeScaleModifier("TimeSlowArrow", slowFactor);
            TimeManager.Instance.AddTemporaryModifier(timeMod, slowDuration);
        }


        PlayAudio(goalType);

        Destroy(gameObject);
    }

    protected void SmoothTranslate(Vector3 targetPos)
    {
        // Smooth movement toward correct DSP position
        transform.position = Vector2.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-20f * Time.deltaTime)
        );
    }

    void HandleRoundEnd(GameState previousState, GameState newState)
    {
        if (newState != GameState.RoundResultsTally) return;
        Die();
    }

    protected virtual void GameOverEffect()
    {
        StartCoroutine(GameOverEffectCoroutine());
    }

    private IEnumerator GameOverEffectCoroutine()
    {
        float randomDestroyDelay = UnityEngine.Random.Range(0f, 0.5f);
        yield return new WaitForSeconds(randomDestroyDelay);
        Destroy(gameObject);
    }


    // Audio
    // ----------------------------------------------------------------------------------------

    public void PlayAudio(Goal.GoalType goalType)
    {
        float directionPitch = GetDirectionPitch(_direction) * directionalPitchFactor;

        float pitch =
            goalType == Goal.GoalType.Critical
                ? criticalHitPitch
                : normalHitPitch;

        pitch += directionPitch;

        AudioHelpers.PlayDirectionalArrowHit(
            arrowHitSound,
            transform.position,
            _direction,
            pitch,
            1f,
            directionalEQStrength
        );
    }

    protected float GetDirectionPitch(Vector2 dir)
    {
        if (dir == Vector2.up) return +0.08f;
        if (dir == Vector2.down) return -0.08f;
        if (dir == Vector2.right) return +0.04f;
        if (dir == Vector2.left) return -0.04f;

        // Diagonals
        if (dir == new Vector2(1, 1).normalized) return +0.10f;
        if (dir == new Vector2(-1, 1).normalized) return +0.02f;
        if (dir == new Vector2(1, -1).normalized) return -0.02f;
        if (dir == new Vector2(-1, -1).normalized) return -0.10f;

        return 0f;
    }


    // Setting Status / State
    // ----------------------------------------------------------------------------------------

    public void SetToEditorArrow()
    {
        _arrowType = ArrowType.Editor;
    }

    public virtual void SetRecoveryArrow()
    {
        AddStatus(ArrowStatus.Recovery);

        if(!runTimeMaterial) 
            return;

        runTimeMaterial.SetColor(colorApropertyName, recoveryColorA);
        runTimeMaterial.SetColor(colorBpropertyName, recoveryColorB);
    }

    public virtual void SetTimeSlowArrow()
    {
        AddStatus(ArrowStatus.TimeSlow);

        if(!runTimeMaterial) 
            return;

        runTimeMaterial.SetColor(colorApropertyName, Color.blue);
        runTimeMaterial.SetColor(colorBpropertyName, Color.purple);
    }

    public virtual void SetGolden()
    {
        AddStatus(ArrowStatus.Golden);
        
        if(!runTimeMaterial) 
            return;

        if(!HasStatus(ArrowStatus.Recovery) && !HasStatus(ArrowStatus.TimeSlow))
        {
            runTimeMaterial.SetColor(colorApropertyName, goldenColorA);
            runTimeMaterial.SetColor(colorBpropertyName, goldenColorB);
        }
    }



    // Arrow Status Helpers
    // ------------------------------------------------------------------

    public bool HasStatus(ArrowStatus s)
    {
        return _status.HasFlag(s);
    }

    public bool HasAny(ArrowStatus mask)
    {
        return (_status & mask) != 0;
    }

    public bool HasAll(ArrowStatus mask)
    {
        return (_status & mask) == mask;
    }

    // ---------- Mutations ----------

    public void AddStatus(ArrowStatus s)
    {
        _status |= s;
    }

    public void RemoveStatus(ArrowStatus s)
    {
        _status &= ~s;
    }

    public void ClearStatus()
    {
        _status = ArrowStatus.None;
    }

    public void SetStatus(ArrowStatus s)
    {
        _status = s;
    }

    public ArrowStatus GetStatus()
    {
        return _status;
    }
}
