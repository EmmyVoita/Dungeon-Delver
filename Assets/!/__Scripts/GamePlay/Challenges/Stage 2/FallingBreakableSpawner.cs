using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBreakableSpawner : ChallengeBase
{
    
    public enum LifetimeMode
    {
        SelfManaged,
        SelfManageDontDestroy,
        External
    }
    
    
    public enum CompletionConditon
    {
        AllDestroyed,
        Timer
    }
    [SerializeField] private bool listenForBossActive = false;
    [SerializeField] private LifetimeMode lifetimeMode = LifetimeMode.SelfManaged;
    [SerializeField] private bool switchPlayerState = true;
    [SerializeField] private CompletionConditon completionCondition = CompletionConditon.Timer;

    public GameObject breakablePrefab;
    public GameObject playerProjectile;
    public SoundEffect shootSoundEffect;

    public float spawnDistance = 6f;
    public float spawnInterval = 1.5f;
    public int spawnCount = 4;

    [Header("Shooting Settings")]
    public float minFireDelay = 0.4f;
    private float nextAllowedFireTime = 0f;
    private int lastDirectionIndex = -1;

    public float unregisterDelay = 8f;

    public LifetimeMode LifetimeSetting => lifetimeMode;
    private int aliveBreakables = 0;
    private bool spawningComplete = false;
    private List<FallingBreakable> activeBreakables = new();



    public Vector2[] directions =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    void OnEnable()
    {
        ObstacleManager.OnObstacleCleared += HandleObstacleCleared;
    }

    void OnDisable()
    {
        ObstacleManager.OnObstacleCleared -= HandleObstacleCleared;
    }

    private void HandleObstacleCleared()
    {
        // If this spawner belongs to a parent obstacle
        if (transform.parent == null) return;

        // If parent obstacle no longer exists in manager, force cleanup
        if (!ObstacleManager.Instance.AnyActive)
        {
            ForceKillAll();
        }
    }

    void Update()
    {
        if(listenForBossActive && !BossManager.Instance.IsBossActive) return;
        
        if (switchPlayerState && InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            if (Time.time >= nextAllowedFireTime)
            {
                nextAllowedFireTime = Time.time + minFireDelay;

                AudioHelpers.PlaySoundEffect(shootSoundEffect, Player.Instance.transform.position);
                Player.Instance.ShootProjectile(playerProjectile.GetComponent<PlayerProjectile>());
            }
        }

    }

    void Start()
    {
        if(!listenForBossActive && lifetimeMode != LifetimeMode.External) 
            Begin();

        if(listenForBossActive && BossManager.Instance.IsBossActive)
            Begin();
    }



    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }

        spawningComplete = true;

        if(completionCondition == CompletionConditon.Timer)
        {
            yield return new WaitForSeconds(unregisterDelay);
            End();
            /*
            if (lifetimeMode != LifetimeMode.External)
                ObstacleManager.Instance.UnregisterObstacle(gameObject);

            if (switchPlayerState)
                Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
            
            if (lifetimeMode != LifetimeMode.SelfManageDontDestroy)
                Destroy(this.gameObject);
            */
        }
            

        CheckForCompletion();
    }

    public void NotifyBreakableDestroyed(FallingBreakable breakable)
    {
        aliveBreakables--;
        activeBreakables.Remove(breakable);

        CheckForCompletion();
    }

    private void CheckForCompletion()
    {
        if(completionCondition != CompletionConditon.AllDestroyed) return;

        if (!spawningComplete) return;

        if (aliveBreakables <= 0)
        {
            End();
            /*
            if (lifetimeMode != LifetimeMode.External)
                ObstacleManager.Instance.UnregisterObstacle(gameObject);

            if (switchPlayerState)
                Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);

           if (lifetimeMode != LifetimeMode.SelfManageDontDestroy)
                Destroy(this.gameObject);
                */
        }
    }

    public void ForceKillAll()
    {
        // Make a copy to avoid modifying during iteration
        var snapshot = new List<FallingBreakable>(activeBreakables);

        foreach (var b in snapshot)
        {
            if (b != null)
                b.ForceKill();
        }

        activeBreakables.Clear();
        aliveBreakables = 0;
    }

    void SpawnOne()
    {
        int newIndex = Random.Range(0, directions.Length - 1);

        if (newIndex >= lastDirectionIndex)
            newIndex++;

        lastDirectionIndex = newIndex;

        Vector2 dir = directions[newIndex];
        Vector3 spawnPos = dir * spawnDistance;

        Vector2 toCenter = (-spawnPos).normalized;

        float angle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        GameObject obj = Instantiate(breakablePrefab, spawnPos, rot);

        FallingBreakable breakable = obj.GetComponent<FallingBreakable>();
        breakable.owner = this;   // 👈 assign owner

        activeBreakables.Add(breakable);
        aliveBreakables++;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ForceKillAll();
    }

    public override void Begin(object config = null)
    {
        base.Begin();
        StartCoroutine(SpawnRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}
