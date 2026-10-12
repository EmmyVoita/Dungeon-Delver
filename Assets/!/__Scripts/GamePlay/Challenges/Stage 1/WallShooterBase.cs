using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEngine;

public abstract class WallShooterBase : ChallengeBase
{
    

    [SerializeField] protected WallShooterData settingsData;

    [Header("Activation")]
    [SerializeField] private bool listenForBossActive = false;


    [Header("References")]
    public GameObject portalPrefab;
    public GameObject projectilePrefab;
    public GameObject linePrefab;


    [Header("General Settings")]
    public float unregisterDelay = 1.0f;
    public float startDelay = 0.0f;



    [Header("MoveBackForth Settings")]
    public float oscillateDistance = 0.5f;   // how far shooters move left/right
    public float oscillateSpeed = 2f;        // how fast they oscillate
    public float shiftDuration = 0.5f;      // how long they shift for during windup
    

    [Header("Position List FirePositionMode")]
    public List<Vector3> predefinedOffsets = new List<Vector3>();


    [Header("Projectile Settings")]
    public Vector2 projectileFireDirection = Vector2.down;
    public int projectileCount = 8;               
    public float projectileSpacing = 1.0f;
    public float projectileSpeed = 10f;
    [Tooltip("The projectile winds up pulling back before firing so this describes the length of that")] 
    public float firingWindupDuration = 0.5f;
    [Tooltip("If using Randomized fire delay mode, this is the max delay applied to each projectile before they fire in sequence")] 
    public float maxRandFireDelay = 0.25f;   
    public float postWindupDelay = 0.0f;


    [Header("Spawn Position Settings")]   
    public Vector3 spawnPositionOffset = Vector3.zero;   
    [Range(0f, 2f)] public float randPosOffsetX = 0.3f;
    [Range(0f, 2f)] public float randPosOffsetY = 0.15f;


    [Header("Audio")]
    public SoundEffect fireSound;
    public SoundEffect tickSound;
    public int tickCount = 3;
    

    


    protected List<WallProjectile> _projectiles = new List<WallProjectile>();
    protected List<Vector3> _originalPositions = new List<Vector3>();
    protected GameObject _obstacleContainer;
    protected List<SpriteFadeInOut> _spriteFade = new List<SpriteFadeInOut>();


    protected virtual void Awake()
    {
        _obstacleContainer = new GameObject("WallShooter_Container");
    }

    protected virtual void SpawnProjectiles()
    {
        _projectiles.Clear();
        _spriteFade.Clear();
        _originalPositions.Clear();

         Vector2 spawnAxis = new Vector2(-projectileFireDirection.y, projectileFireDirection.x).normalized;
        float totalWidth = (projectileCount - 1) * projectileSpacing;
        float startOffset = -totalWidth / 2f;

        float shiftSign = Random.value < 0.5f ? -1f : 1f;
        
        for (int i = 0; i < projectileCount; i++)
        {

            switch(settingsData.firePositionMode)
            {
                case FirePositionMode.PositionList:
                    int randIndex = Random.Range(0, predefinedOffsets.Count); 
                    spawnPositionOffset = predefinedOffsets[randIndex];
                    break;
                case FirePositionMode.Default:
                default:
                    break;
            }

            Vector3 basePos = spawnPositionOffset + (Vector3)spawnAxis * (startOffset + i * projectileSpacing);

            basePos += new Vector3(Random.Range(-randPosOffsetX, randPosOffsetX),Random.Range(-randPosOffsetY, randPosOffsetY),0);

            
            // Spawn projectile
            GameObject obj = Instantiate(projectilePrefab, basePos, Quaternion.identity);
            WallProjectile wp = obj.GetComponent<WallProjectile>();

        

            wp.Init(projectileFireDirection, 
                    projectileSpeed, 
                    firingWindupDuration, 
                    shiftDuration,
                    false, 
                    oscillateDistance, 
                    oscillateSpeed * shiftSign);

            _projectiles.Add(wp);

            obj.transform.parent = _obstacleContainer.transform;

            _originalPositions.Add(obj.transform.position);


            // Orientation
            float angle = Mathf.Atan2(projectileFireDirection.y, projectileFireDirection.x) * Mathf.Rad2Deg;
            obj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Spawn preview line at same randomized pos
            GameObject line = Instantiate(linePrefab, basePos, Quaternion.identity);
            line.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            if(portalPrefab != null)
            {
                GameObject portal = Instantiate(portalPrefab, basePos, Quaternion.identity);
                portal.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
        

            if(settingsData.projectileFireMode == ProjectileFireMode.MoveBackForth)
            {
                line.transform.parent = obj.transform;
            }

            _spriteFade.Add(line.GetComponent<SpriteFadeInOut>());
        }
    }

    

    protected virtual IEnumerator HandleChallengeSequence()
    {
        yield return null;
    }

    protected IEnumerator ChallengeSequence()
    {
        yield return new WaitForSeconds(startDelay);
        yield return HandleChallengeSequence();
        yield return new WaitForSeconds(unregisterDelay);
        End();
    }

    protected IEnumerator FireSequence()
    {
        switch (settingsData.fireDelayMode)
        {
            case FireDelayMode.Immediate:
                HandleImmediateFireMode();
                break;
            case FireDelayMode.WithSetDelay:
                HandleFireModeWithSetDelay();
                break;
            case FireDelayMode.Randomized:
                HandleRandomizedFireMode();
                break;
        }

       
        float tickInterval = firingWindupDuration / tickCount;

        for (int i = 0; i < tickCount; i++)
        {
            float pitch = 1.0f + i * 0.2f;
            AudioHelpers.PlaySoundEffect(tickSound, Camera.main.transform.position, pitch);
            yield return new WaitForSeconds(tickInterval);
        }

        AudioHelpers.PlaySoundEffect(fireSound, Camera.main.transform.position);
    }

    protected virtual void HandleImmediateFireMode()
    {
        
    }

    protected virtual void HandleFireModeWithSetDelay()
    {
        for(int i = 0; i < _projectiles.Count; i++)
        {
            StartCoroutine(FireWithDelay(_projectiles[i], _spriteFade[i], postWindupDelay));
        }
    }

    protected virtual void HandleRandomizedFireMode()
    {
        for(int i = 0; i < _projectiles.Count; i++)
        {
            float randomDelay = Random.Range(0f, maxRandFireDelay);
            StartCoroutine(FireWithDelay(_projectiles[i], _spriteFade[i], randomDelay));
        }
    }
    

    protected IEnumerator FireWithDelay(WallProjectile projectile,SpriteFadeInOut fade,float delay)
    {
        yield return new WaitForSeconds(delay);

        if (fade != null)
            fade.StartCoroutine(fade.FadeSequence());

        if (projectile != null)
            projectile.StartCoroutine(projectile.WindupAnim());
    }

    protected override void CleanUp()
    {
        base.CleanUp();

        foreach(WallProjectile projectile in _projectiles)
        {
            Destroy(projectile.gameObject);
        }

        Destroy(_obstacleContainer);
    }

    public override void Begin(object config = null)
    {
        if(listenForBossActive && !BossManager.Instance.IsBossActive)
            return;
        
        base.Begin();

        OnShooterBegin();
    }

    public override void End()
    {
        base.End();
        
        Destroy(gameObject,1.0f);
    }

    protected virtual void OnShooterBegin()
    {
        
    }

}