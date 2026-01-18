using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WallShooter : MonoBehaviour
{
    public enum FireMode
    {
        Burst,
        Continuous
    }

    public enum ProjectileFireMode
    {
        Still,
        MoveBackForth
    }

    public enum FireDelayMode
    {
        Immediate,
        WithSetDelay,
        Randomized
    }

    public enum FirePositionMode
    {
        Default,
        PositionList
    }

    public enum ActivationMode
    {
        AutoOnStart,
        Manual
    }

    public enum LifetimeMode
    {
        SelfManaged,
        External
    }

    [SerializeField] private LifetimeMode lifetimeMode = LifetimeMode.SelfManaged;


    [Header("Activation")]
    [SerializeField] private ActivationMode activationMode = ActivationMode.AutoOnStart;


    [Header("References")]
    public GameObject projectilePrefab;
    public GameObject linePrefab;


    [Header("General Settings")]
    public FireMode fireMode = FireMode.Burst;
    public ProjectileFireMode projectileFireMode = ProjectileFireMode.MoveBackForth;
    public FireDelayMode fireDelayMode = FireDelayMode.Randomized;
    public FirePositionMode firePositionMode = FirePositionMode.Default;
    public float unregisterDelay = 1.0f;


    [Header("Continuous Fire Settings")]
    public float fireDelay = 5.0f;
    public int continuousFireCount = 3;


    [Header("MoveBackForth Settings")]
    public float oscillateDistance = 0.5f;   // how far shooters move left/right
    public float oscillateSpeed = 2f;        // how fast they oscillate
    public float shiftDuration = 0.5f;      // how long they shift for during windup
    public SoundEffect shiftStartSound;

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
    

    
    public LifetimeMode LifetimeSetting => lifetimeMode;


    private List<WallProjectile> projectiles = new List<WallProjectile>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private GameObject obstacleContainer;

    private List<SpriteFadeInOut> spriteFade = new List<SpriteFadeInOut>();



    void Awake()
    {
        if (lifetimeMode == LifetimeMode.SelfManaged)
            ObstacleManager.Instance.RegisterObstacle(gameObject);

        obstacleContainer = new GameObject("WallShooter_Container");
    }


    void Start()
    {
        if (activationMode == ActivationMode.AutoOnStart)
        {
            StartChallenge();
        }
    }

    public void StartChallenge()
    {
        StartCoroutine(ChallengeSequence());
    }


    void OnDestroy()
    {
        ObstacleManager.Instance.UnregisterObstacle(gameObject);
    }

    


    void SpawnProjectiles()
    {
        projectiles.Clear();
        spriteFade.Clear();
        originalPositions.Clear();


        Vector2 spawnAxis = new Vector2(-projectileFireDirection.y, projectileFireDirection.x).normalized;
        float totalWidth = (projectileCount - 1) * projectileSpacing;
        float startOffset = -totalWidth / 2f;

        float shiftSign = Random.value < 0.5f ? -1f : 1f;
          
        for (int i = 0; i < projectileCount; i++)
        {

            switch(firePositionMode)
            {
                case FirePositionMode.PositionList:
                    int randIndex = Random.Range(0, predefinedOffsets.Count); 
                    spawnPositionOffset = predefinedOffsets[randIndex];
                    break;
                case FirePositionMode.Default:
                default:
                    break;
            }

            Vector3 basePos = transform.position + spawnPositionOffset + (Vector3)spawnAxis * (startOffset + i * projectileSpacing);

            // 🎯 Random offset
            basePos += new Vector3(
                Random.Range(-randPosOffsetX, randPosOffsetX),
                Random.Range(-randPosOffsetY, randPosOffsetY),
                0
            );

            

            // Spawn projectile
            GameObject obj = Instantiate(projectilePrefab, basePos, Quaternion.identity);
            WallProjectile wp = obj.GetComponent<WallProjectile>();

          

            wp.Init(projectileFireDirection, 
                    projectileSpeed, 
                    firingWindupDuration, 
                    shiftDuration,
                    projectileFireMode == ProjectileFireMode.MoveBackForth, 
                    oscillateDistance, 
                    oscillateSpeed * shiftSign);

            projectiles.Add(wp);

            obj.transform.parent = obstacleContainer.transform;

            originalPositions.Add(obj.transform.position);


            // Orientation
            float angle = Mathf.Atan2(projectileFireDirection.y, projectileFireDirection.x) * Mathf.Rad2Deg;
            obj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Spawn preview line at same randomized pos
            GameObject line = Instantiate(linePrefab, basePos, Quaternion.identity);
            line.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            if(projectileFireMode == ProjectileFireMode.MoveBackForth)
            {
                line.transform.parent = obj.transform;
            }

            spriteFade.Add(line.GetComponent<SpriteFadeInOut>());
        }
    }

    IEnumerator ChallengeSequence()
    {
        switch(fireMode)
        {
            case FireMode.Burst:
                yield return StartCoroutine(FireSequence());
                break;

            case FireMode.Continuous:
                for(int i = 0; i < continuousFireCount; i++)
                {
                    yield return StartCoroutine(FireSequence());
                    yield return new WaitForSeconds(fireDelay);
                }
                break;
        }

        if (lifetimeMode == LifetimeMode.SelfManaged)
        {
            yield return new WaitForSeconds(unregisterDelay);
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Destroy(gameObject, 1.0f);
        }
    }

    IEnumerator FireSequence()
    {
        SpawnProjectiles();


        // 🔫 Fire projectiles

        switch (fireDelayMode)
        {
            case FireDelayMode.Immediate:
                for(int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Fire();
                    spriteFade[i].StartCoroutine(spriteFade[i].FadeSequence());
                }
                break;

            case FireDelayMode.WithSetDelay:
                for(int i = 0; i < projectiles.Count; i++)
                {
                    StartCoroutine(FireWithDelay(projectiles[i], spriteFade[i], postWindupDelay));
                }
                break;

            case FireDelayMode.Randomized:
                for(int i = 0; i < projectiles.Count; i++)
                {
                    float randomDelay = Random.Range(0f, maxRandFireDelay);
                    StartCoroutine(FireWithDelay(projectiles[i], spriteFade[i], randomDelay));
                }
                break;
        }

        if(projectileFireMode == ProjectileFireMode.MoveBackForth)
        {
            AudioHelpers.PlaySoundEffect(shiftStartSound, Camera.main.transform.position);
            yield return new WaitForSeconds(shiftDuration);
        }
        
        float tickInterval = firingWindupDuration / tickCount;

        // 🔔 Windup ticks
        for (int i = 0; i < tickCount; i++)
        {
            float pitch = 1.0f + i * 0.2f;
            AudioHelpers.PlaySoundEffect(tickSound, Camera.main.transform.position, pitch);
            yield return new WaitForSeconds(tickInterval);
        }

        AudioHelpers.PlaySoundEffect(fireSound, Camera.main.transform.position);
    }

    IEnumerator FireWithDelay(
        WallProjectile projectile,
        SpriteFadeInOut fade,
        float delay
    )
    {
        yield return new WaitForSeconds(delay);

        if (fade != null)
            fade.StartCoroutine(fade.FadeSequence());

        if (projectile != null)
            projectile.Fire();
    }

}
