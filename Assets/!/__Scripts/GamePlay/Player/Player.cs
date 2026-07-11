using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.VFX;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public static event System.Action OnJumpInput;
    public static event System.Action<Vector2> OnJumped;
    public static event System.Action OnAbilityFilled;
    public static event System.Action<int, int, int> OnAbilityChargeChanged;
    //public static event System.Action OnMaxAbilityChargeChanged;
    public static event System.Action OnAbilityUsed;
    public static event System.Action<int> OnDamageTaken;
    public static event System.Action<int, bool> OnHeal;
    public static event System.Action OnMaxHealthChanged;
    public static event System.Action<HitData> OnProcessHit;
    public static event System.Action<int> OnPreDamageTaken;
    public static event System.Action<PlayerControlState> OnControlStateChanged;





    [Header("Set in Inspector")]
    public GameObject goal;
    public GameObject spriteObj;
    public GameObject invincibleSpriteObj;
    [SerializeField] private int _maxHealth = 10;


    [Header("Ability Charge Settings")]
    //[SerializeField] private AudioClip abilitySound;
    [SerializeField] private SoundEffect abilityChargedSoundEffect;
    [SerializeField] private float _critWindow = 0.2f;
    [SerializeField] private int _abilityChargeGain = 1;
    
  
    [Header("Rotation Settings")]
    public float goalRotateSpeed = 10f;
    

    [Header("On Damage Settings")]
    [SerializeField] private bool playAudioWhenInvincible = false;
    [SerializeField] private SoundEffect damageSoundEffect;
    [SerializeField] private SoundEffect abilityChargeDamageSoundEffect;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float hitShakeStrength = 0.05f;
    [SerializeField] private float hitShakeDuration = 0.15f;


    [Header("Heal Settings")]
    [SerializeField] private AudioClip healSound;
    [SerializeField] private ParticleSystem healParticleSystem;
    [SerializeField] private VisualEffect healEffect;


    [Header("Boost Settings")]
    public PlayerWings wings;
    [SerializeField] private float boostDistance = 3f;
    [SerializeField] private float boostDuration = 0.15f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private float boostCooldown = 0.75f;
    [SerializeField] private AudioClip boostSound;


    [Header("Jump Settings")]
    public AudioClip jumpSound;
    public float jumpPitch = 2.0f;
    public float maxJumpHoldTime = 0.5f;
    public float jumpForce = 7f;           // initial jump velocity
    public float fallMultiplier = 2.5f;    // stronger gravity when falling
    public float lowJumpMultiplier = 2f;   // extra gravity if jump key is released early

    [Header("Lande Dodger Control State")]
    [SerializeField] private LaneVisualizer laneVisualizer;
    [SerializeField] private SoundEffect moveLaneSoundEffect;
    [SerializeField] private float lanePitchStep = 0.05f;


    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpawnOffset = 0.5f;
    [SerializeField] private SoundEffect controlModeSwitchSound;



    [Header("Set Dynamically")]
    [SerializeField] private int _health;
    [SerializeField] private AbilityBase _currentAbility;
    [SerializeField] private bool _useEightDirections = false;
    [SerializeField] private int currentLane = 0;
    [SerializeField] private LaneDodgerConfig currentLaneConfig;




    // Upgrades
    private List<UpgradeEffectBase> activeUpgrades = new List<UpgradeEffectBase>();

    // Ability Logic
    private int _abilityCharge = 0;
    private int _maxAbilityCharge = 10;

    // Rotation
    private Vector2 _lastFacingDir = Vector2.up;
    private Quaternion _targetRotation;
    private bool _isRotating = false;
    private float _rotateStartTime;

    // Input
    public bool _lockInput = false;

    // Visuals
    private SpriteRenderer _sRend;

    // Jump Logic
    private Rigidbody2D _rb;
    private Vector3 _centerPosition;
    private Vector2 _jumpAxis; 
    private bool _isJumping = false;
    private float _jumpElapsedTime = 0f;
    private Tween _laneMoveTween;
    private bool _hoverActive;
    private bool _hoverLockedOut;
    private Vector2 _lastToCenter;

    // Invincible
    private bool _invincible;
    private float invincibileDone = 0;
    private int _blockedHits = 0;

   


    public PlayerControlState playerControlState { get; private set; } = PlayerControlState.Normal;
    public string LastDamageSource { get; private set; } = "Unknown";
    public Vector2 LastFacingDirection => _lastFacingDir;
    public bool IsRotating => _isRotating;
    public float RotateStartTime => _rotateStartTime;
    public float CritWindow => _critWindow;
    public bool FullAbilityCharge => AbilityCharge >= MaxAbilityCharge; 
    public int CurrentLane => currentLane;
    public bool FullyLocked { get; private set; } = false;
    public bool AbilityChargeLocked { get; set; } = false;
    public bool CanJump => playerControlState == PlayerControlState.Shooter || playerControlState ==  PlayerControlState.BasicJump;
    public bool CanTakeDamage => GameStateEffectManager.PlayerDamageAllowed && !DevCheats.Invincible;

  
    public bool Invincible
    {
        get => _invincible;
        set
        {
            if (_invincible == value) return;
            _invincible = value;
            _sRend.color = _invincible ? Color.red : Color.white;
        }
    }


    public bool UseEightDirections
    {
        get { return _useEightDirections; }
        set { _useEightDirections = value; }
    }

    public int Health
    {
        get { return _health; }
        set { _health = value; }
    }

    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = Mathf.Max(1, value);
            OnMaxHealthChanged?.Invoke();
        }
    }

    public int MaxAbilityCharge
    {
        get { return UpgradeManager.Instance != null ? Mathf.Max((int) UpgradeManager.Instance.ModifyAbilityCost(_maxAbilityCharge),10) : 0; }
        set { _maxAbilityCharge = value; }
    }   

    public int AbilityCharge
    {
        get => _abilityCharge;
        set
        {
            int previous = _abilityCharge;

            int attemptedDelta = value - previous;

            int clamped = Mathf.Clamp(value, 0, MaxAbilityCharge);
            int appliedDelta = clamped - previous;

            _abilityCharge = clamped;

            // Feedback / juice / UI
            OnAbilityChargeChanged?.Invoke(previous, attemptedDelta, appliedDelta);

            // Filled event
            if (previous < MaxAbilityCharge && clamped >= MaxAbilityCharge)
            {
                AudioHelpers.PlaySoundEffect(abilityChargedSoundEffect, Camera.main.transform.position);
                OnAbilityFilled?.Invoke();
            }
        }
    }

    
    public AbilityBase CurrentAbility
    {
        get { return _currentAbility; }
        set
        {
            _currentAbility = value;
            MaxAbilityCharge = _currentAbility != null ? _currentAbility.abilityBaseCost : 0;
        }
    }

    public void SetInvincible(float duration)
    {
        Invincible = true;

        invincibleSpriteObj.SetActive(true);

        float newEndTime = Time.time + duration;

        // Extend instead of overwrite (feels better)
        if (newEndTime > invincibileDone)
            invincibileDone = newEndTime;
    }

    public void AddHitBlock(int amount = 1)
    {
        _blockedHits += amount;
        Debug.Log($"Add hit block amount => {amount}");
    }

    
    void OnEnable()
    {
        RoundManager.OnRoundStart += HandleRoundStart;
        RoundManager.OnRoundEnd += HandleRoundEnd;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        RoundManager.OnRoundStart -= HandleRoundStart;
        RoundManager.OnRoundEnd -= HandleRoundEnd;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.RunLoad)
        {
            Health = MaxHealth;
        }

        if(GameStateEffectManager.ShowPlayer)
            Show();
        else
            Hide();
    }

    void Show()
    {
        goal.SetActive(true);
        spriteObj.SetActive(true);
    }

    void Hide()
    {
        goal.SetActive(false);
        spriteObj.SetActive(false);
    }

    void OnDestroy()
    {
        _laneMoveTween?.Kill();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _targetRotation = Quaternion.identity;
        _sRend = spriteObj.GetComponent<SpriteRenderer>();
        Health = 0;
        _centerPosition = transform.position; // cache original center
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            HealPlayer(1,true);
        }

        goal.transform.position = transform.position;

        Vector2 dir = Vector2.zero;

        if (Invincible && Time.time > invincibileDone)
        {
            Invincible = false;
            invincibleSpriteObj.SetActive(false);
        }
            

        if (FullyLocked)
            return;

        if(!GameStateEffectManager.PlayerInputEnabled) return;

        if (_useEightDirections)
        {
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveUp)) dir += Vector2.up;
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveDown)) dir += Vector2.down;
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveLeft)) dir += Vector2.left;
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveRight)) dir += Vector2.right;

            if (dir.sqrMagnitude > 0.1f)
            {
                dir.Normalize();
                RotateCollider(dir);
                _lastFacingDir = dir;
            }
        }
        else
        {
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveUp)) dir = Vector2.up;
            else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveDown)) dir = Vector2.down;
            else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveLeft)) dir = Vector2.left;
            else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveRight)) dir = Vector2.right;

            if (dir != Vector2.zero)
            {
                RotateCollider(dir);
                _lastFacingDir = dir;
            }
        }

        goal.GetComponentInChildren<Goal>().SetGoalDirection(dir);

        // Ability usage
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.UseAbility) && 
           FullAbilityCharge
           && !_lockInput)
        {
            AbilityCharge -= MaxAbilityCharge;
            OnAbilityUsed?.Invoke();
            SpawnAbility();
        }

        if (playerControlState == PlayerControlState.LaneDodger)
        {
            HandleLaneMovement();
            return;
        }


        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Jump) && !_lockInput)
        {
            if (playerControlState == PlayerControlState.LockedShooter)
            {
                OnJumpInput?.Invoke();
                if (wings != null) wings.PlayFlap();  
                return;
            }

       

            // Normal jump path
            if (CanJump && !_isJumping)
            {
                PerformJump(dir);
            }
        }
        
    }

    void FixedUpdate()
    {
        ApplySmartGravity();
    }

    void PerformJump(Vector2 dir)
    {
        //UIToast.Show("Performing Jump", 1);
        AudioHelpers.PlayMyClipAtPoint(boostSound, AudioChannel.SFX, Camera.main.transform.position);

        // Use last direction if no input
        Vector2 inputDir = dir != Vector2.zero ? dir : _lastFacingDir;
        inputDir.Normalize();

        _jumpAxis = inputDir;  // store jump direction
        _isJumping = true;

        _hoverActive = true;        // Hover is allowed at start
        _hoverLockedOut = false;    // Not locked yet
        _jumpElapsedTime = 0f;

        _rb.linearVelocity = _jumpAxis * jumpForce;

        if (wings != null)
        wings.PlayFlap();

        AudioHelpers.PlayClipWithVariation(jumpSound, AudioChannel.SFX, Camera.main.transform.position, basePitch: jumpPitch, pitchRange: 0.1f);

        OnJumped?.Invoke(inputDir);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        BounceBomb bomb = coll.GetComponentInParent<BounceBomb>();
        if (bomb != null)
        {
            bomb.OnPlayerHit(_lastFacingDir);
            return;
        }

        if (coll.gameObject.CompareTag("Player")) return;

        // If arrow is the collider and the arrow is invincible, ignore
        ArrowBase arrow = coll.GetComponent<ArrowBase>();
        
        
        

        //
            
    
        if(arrow != null && arrow.IsInverse)
        {
            goal.GetComponentInChildren<Goal>().ResolveArrowCaught(arrow);
            if (arrow != null) arrow.OnArrowHit(1, Goal.GoalType.Normal, arrow.Direction);
            return;
        }

        if (arrow != null && arrow.Invincible) return;

        // If we are invincible, destroy the arrow and return
        if (Invincible)
        {
            if (arrow != null) arrow.KillArrow();
            return;
        }

        

        DamageEffect dEf = coll.GetComponent<DamageEffect>();

        if(dEf == null) return;

        HitData hit = new()
        {
            Damage = dEf.damage,
            AbilityDamage = dEf.abilityChargeDamage,
            SourceName = dEf.sourceName,
            Arrow = arrow,
            PlayHitSound = dEf.playHitSound
        };

        ProcessHit(hit);
    }

    private void ProcessHit(HitData hit)
    {
        OnProcessHit?.Invoke(hit);

        if (hit.PlayHitSound && (!Invincible || playAudioWhenInvincible))
            AudioHelpers.PlaySoundEffect(damageSoundEffect, transform.position);

        HandleArrowLogic(hit);

        if(!CanTakeDamage)
            return;

        if(TryBlockHit(hit))
            return;

        ApplyAbilityDamage(hit);

        bool tookHealthDamage = ApplyHealthDamage(hit);

        if(tookHealthDamage)
        {
            PlayerDamageFeedback();
            CheckForDeath();
        }
    }

    private bool TryBlockHit(HitData hit)
    {
        if (_blockedHits <= 0)
            return false;

        _blockedHits--;

        SetInvincible(0.15f);

        hit.Arrow?.KillArrow();

        OnPreDamageTaken?.Invoke(0);

        return true;
    }

    private void HandleArrowLogic(HitData hit)
    {
        // If we hit an arrow, we kill the arrow
        if (hit.Arrow != null) hit.Arrow.KillArrow();
    }

    private void ApplyAbilityDamage(HitData hit)
    {
        if(hit.AbilityDamage > 0)
        {
            AudioHelpers.PlaySoundEffect(abilityChargeDamageSoundEffect, transform.position);
            AbilityCharge -= hit.AbilityDamage;
        }
    }

    private bool ApplyHealthDamage(HitData hit)
    {
        // We need to keep track of the last damage source for the game over screen
        if(hit.Damage > 0)
            LastDamageSource = hit.SourceName;

        // Damage logic
        int finalDamage = UpgradeManager.Instance.ModifyDamageTaken(hit.Damage);
        OnPreDamageTaken?.Invoke(finalDamage);
        Health -= finalDamage;
        OnDamageTaken?.Invoke(finalDamage);

        // Handle temporary invincibility
        if(finalDamage > 0)
        {
            Invincible = true;
            invincibileDone = Time.time + invincibilityDuration;
        }

        return finalDamage > 0;
    }

    private void CheckForDeath()
    {
        if(Health <= 0 && GameStateEffectManager.PlayerDeathAllowed)
        {
            GameStateManager.Instance.SetState(GameState.DeathSequence);
        }
    }

    private void PlayerDamageFeedback()
    {
        // Visual Effects
        if(damageEffectPrefab != null)
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        
        Squish();
    }

    public void DamageSelf(DamageEffect dEf, ArrowBase arrow = null)
    {
        

        //if(!GameStateEffectManager.PlayerDamageAllowed)
            //return;

        // Play Hit sound
        if (dEf.playHitSound && (!Invincible || playAudioWhenInvincible))
            AudioHelpers.PlaySoundEffect(damageSoundEffect, transform.position);

        

        // If we hit an arrow, we kill the arrow
        if (arrow != null) arrow.KillArrow();

        if (DevCheats.Invincible)
            return;

        if (_blockedHits > 0)
        {
            _blockedHits--;

            // optional feedback
            SetInvincible(0.15f);

            if (arrow != null)
                arrow.KillArrow();

            Debug.Log("Blocked hit !");

            OnPreDamageTaken?.Invoke(0);

            return;
        }

        

        // Handle death case
        if (Health <= 0 && GameStateEffectManager.PlayerDeathAllowed)
        {
            GameStateManager.Instance.SetState(GameState.DeathSequence);
        }

        

        

        Squish();
    }


    public void ShootProjectile(PlayerProjectile projectilePrefab)
    {
        Vector2 snappedDir = GetSnappedDirection(_lastFacingDir, _useEightDirections);

        Vector3 spawnPos = transform.position + (Vector3)(snappedDir * projectileSpawnOffset);

        PlayerProjectile proj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        Vector2 normalizedDir = snappedDir.normalized;
        float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg - 90f;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        proj.Initialize(snappedDir);
    }

    
    public void SetFullyLocked(bool value)
    {
        FullyLocked = value;
    }

    public void SetPlayerControlState(PlayerControlState newState, object config = null)
    {
        if (playerControlState == newState) return;

        if(playerControlState == PlayerControlState.LaneDodger
            && newState != PlayerControlState.LaneDodger)
        {
            _laneMoveTween.Kill();
            laneVisualizer?.Clear();
        }

        Debug.Log($"Control State Changed -> old {playerControlState} : new {newState}");

        bool resetPosition = playerControlState == PlayerControlState.LaneDodger && newState == PlayerControlState.Normal;
        playerControlState = newState;

        


        // Reset / clear previous state data
        currentLaneConfig = null;

        switch (newState)
        {
            case PlayerControlState.LaneDodger:
                currentLaneConfig = config as LaneDodgerConfig;
                InitializeLaneDodger();
                wings.ShowWings();
                break;
            case PlayerControlState.Shooter:
                wings.ShowWings();
                break;
            case PlayerControlState.LockedShooter:
                _rb.linearVelocity = Vector2.zero;
                wings.HideWings();
                break;
            case PlayerControlState.Normal:
                if(resetPosition)
                    transform.position = Vector3.zero;
                wings.HideWings();
                break;
            case PlayerControlState.BasicJump:
                transform.position = Vector3.zero;
                wings.ShowWings();
                break;
            default:
                break;
        }

        OnControlStateChanged?.Invoke(playerControlState);
        AudioHelpers.PlaySoundEffect(controlModeSwitchSound, this.transform.position);
    }

 


    public void ResetPositionAndVelocity()
    {
        transform.position = _centerPosition;
        _rb.linearVelocity = Vector2.zero;
        _isJumping = false;
    }

        

    public void AddUpgrade(UpgradeEffectBase effect)
    {
        effect.Apply(this);
        activeUpgrades.Add(effect);
    }

    public void HealPlayer(int amount, bool useEffects = true)
    {
        int previousHealth = Health;

        Health = Mathf.Min(MaxHealth, Health + amount);

        int actualHealing = Health - previousHealth;
        bool wasFullHealth = previousHealth >= MaxHealth;

        OnHeal?.Invoke(amount, wasFullHealth);

        if (useEffects && actualHealing > 0)
        {
            PlayHealEffects();
        }
    }

    private void PlayHealEffects()
    {
        if (healParticleSystem != null)
            healParticleSystem.Play();

        if (healEffect != null)
            healEffect.SendEvent("OnPlay");
            //healEffect.Play();

        AudioHelpers.PlayMyClipAtPoint(
            healSound,
            AudioChannel.SFX,
            Camera.main.transform.position
        );
    }





    public void IncreaseMaxHealth(int amount)
    {
        MaxHealth += amount;
        Health = Mathf.Min(Health + amount, MaxHealth);
        OnMaxHealthChanged?.Invoke();
    }


    private void HandleRoundEnd()
    {
        //canJump = false;
        _lockInput = true;
    }

    private void HandleRoundStart()
    {
        //canJump = true;
        _lockInput = false;
    }

    public void OnCriticalCatch()
    {
        if (AbilityChargeLocked)
            return;
        AbilityCharge += _abilityChargeGain;
        Debug.Log($"⚡ Gained {_abilityChargeGain} ability charge from crit catch!");
    }

 

    // -------------------------------------

    private void SpawnAbility()
    {
        // Find the current facing direction the player is allowed to use
        Vector2 snappedDir = GetSnappedDirection(_lastFacingDir, _useEightDirections);

        // Compute snapped rotation
        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion snappedRotation = Quaternion.Euler(0, 0, angle);

        // Spawn the ability prefab
        //currentAbility.transform.rotation = snappedRotation;
        CurrentAbility.Activate(snappedRotation);
        //GameObject wave = Instantiate(shockwavePrefab, transform.position, snappedRotation);
    }

    /// <summary>
    /// Snaps a direction vector to the nearest cardinal (or diagonal) direction.
    /// </summary>
    private Vector2 GetSnappedDirection(Vector2 inputDir, bool useEight)
    {
        if (inputDir == Vector2.zero)
            return _lastFacingDir; // fallback to previous

        // 4 or 8 directions (unit vectors)
        Vector2[] dirs4 = {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };

        Vector2[] dirs8 = {
            Vector2.up,
            new Vector2(1, 1).normalized,
            Vector2.right,
            new Vector2(1, -1).normalized,
            Vector2.down,
            new Vector2(-1, -1).normalized,
            Vector2.left,
            new Vector2(-1, 1).normalized
        };

        Vector2[] pool = useEight ? dirs8 : dirs4;

        // Find direction with smallest angle to input
        float bestDot = -Mathf.Infinity;
        Vector2 bestDir = Vector2.up;

        foreach (Vector2 dir in pool)
        {
            float dot = Vector2.Dot(inputDir.normalized, dir);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestDir = dir;
            }
        }

        return bestDir;
    }



    public void RotateCollider(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle -= 90f;
        Quaternion newTarget = Quaternion.Euler(0, 0, angle);

        if (newTarget != _targetRotation)
        {
            _targetRotation = newTarget;
            _isRotating = true;
            _rotateStartTime = Time.time;
        }
    }

  
    private void Squish()
    {
        _sRend.transform.DOKill();

        _sRend.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            _sRend.transform.DOScale(
                new Vector3(0.5f, 1.6f, 1f), // squash X, stretch Y
                0.06f
            )
        );

        seq.Append(
            _sRend.transform.DOScale(
                new Vector3(1.15f, 0.85f, 1f), // overshoot opposite
                0.08f
            )
            .SetEase(Ease.OutQuad)
        );

        seq.Append(
            _sRend.transform.DOScale(
                Vector3.one,
                0.15f
            )
            .SetEase(Ease.OutBack)
        );

        spriteObj.GetComponent<PlayerSpriteShaker>()?.Shake(hitShakeStrength, hitShakeDuration);
    }


    void ApplySmartGravity()
    {
        if (!_isJumping) return;

        // Project onto jump axis
        Vector2 projected = Vector2.Dot(_rb.linearVelocity, _jumpAxis) * _jumpAxis;
        _rb.linearVelocity = projected;

        Vector2 currentToCenter = (Vector2)_centerPosition - _rb.position;
        Vector2 toCenterDir = currentToCenter.normalized;

        float outwardDot = Vector2.Dot(_rb.linearVelocity, -toCenterDir);
        //bool jumpHeld = InputBindingManager.Instance.GetKeyInput(InputActionType.Jump);
       

        bool jumpHeld = InputBindingManager.Instance.GetKeyInput(InputActionType.Jump);

        // If they release jump while rising, permanently disable hover
        if (!jumpHeld && _hoverActive)
        {
            _hoverLockedOut = true;
        }

        // Hover only works if:
        // - jump is held
        // - not locked out
        // - within hold time
        bool canHover = jumpHeld && !_hoverLockedOut && _jumpElapsedTime <= maxJumpHoldTime;

        if (canHover)
        {
            _jumpElapsedTime += Time.deltaTime;
        }

         //UIToast.Show($"Jump Being Held -> {jumpHeld}", 0.05f);

        if (outwardDot < 0f)
        {
            _rb.linearVelocity += toCenterDir * fallMultiplier * Time.deltaTime;
        }
        //else if (outwardDot > 0f && (!jumpHeld || jumpElapsedTime > maxJumpHoldTime))
        else if (outwardDot > 0f && !canHover)
        {
            _rb.linearVelocity += toCenterDir * lowJumpMultiplier * Time.deltaTime;
        }

        bool distanceCheck = Vector2.Distance(transform.position, _centerPosition) <= 0.175f && Vector2.Dot(_rb.linearVelocity, currentToCenter) > 0f;
        bool dotCheck = _lastToCenter != Vector2.zero && Vector2.Dot(_lastToCenter, currentToCenter) <= 0f;

        // 🔒 Only check crossing AFTER lastToCenter is valid
        if (distanceCheck || dotCheck)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.position = _centerPosition;
            _isJumping = false;
            _lastToCenter = Vector2.zero;
            _hoverActive = false;
            _hoverLockedOut = false;
            return;
        }

        _lastToCenter = currentToCenter;
    }

    void HandleLaneMovement()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            SetLane(currentLane + 1);
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            SetLane(currentLane - 1);
        }
    }

    void SetLane(int newLane)
    {
        int direction = (int)Mathf.Sign(newLane - currentLane);

        newLane = FindNextValidLane(
            currentLane,
            direction
        );

        if (newLane == currentLane)
            return;

        wings.PlayFlap();

        AudioHelpers.PlaySoundEffect(
            moveLaneSoundEffect,
            transform.position,
            1.0f + newLane * lanePitchStep
        );

        currentLane = newLane;

        float targetY =
            GetLaneY(currentLane);

        _laneMoveTween?.Kill();

        _laneMoveTween =
            transform.DOMoveY(
                targetY,
                currentLaneConfig.laneMoveDuration
            )
            .SetEase(Ease.OutQuad);
    }

    int FindNextValidLane(
    int startLane,
    int direction)
    {
        int lane = startLane;

        while (true)
        {
            lane += direction;

            if (
                lane < 0 ||
                lane >= currentLaneConfig.maxLanes
            )
            {
                return startLane;
            }

            if (!LaneState.IsLaneCollapsed(lane))
            {
                return lane;
            }
        }
    }

    float GetLaneY(int lane)
    {
        float centerOffset = (currentLaneConfig.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * currentLaneConfig.laneSpacing;
    }

    void InitializeLaneDodger()
    {
        if (currentLaneConfig == null)
        {
            Debug.LogWarning("LaneDodger state missing config!");
            currentLaneConfig = new LaneDodgerConfig(); // fallback
        }

        laneVisualizer?.ShowLanes(
            currentLaneConfig.maxLanes,
            currentLaneConfig.laneSpacing,
            currentLaneConfig.laneWidthScale
        );

        currentLane = currentLaneConfig.maxLanes / 2;

        float y = GetLaneY(currentLane);
        transform.position = new Vector3(_centerPosition.x, y, transform.position.z);
    }
}
