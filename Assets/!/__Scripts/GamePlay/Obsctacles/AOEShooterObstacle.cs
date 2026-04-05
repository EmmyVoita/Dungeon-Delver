using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AOEShooterObstacle : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnDelay = 3f;
    [SerializeField] private float spawnX = 8f;
    [SerializeField] private int shotCount = 5;
    [SerializeField] private float fireInterval = 1.5f;

    [Header("Tracking")]
    [SerializeField] private float followSmoothTime = 0.6f; // slower than fan shooter
    [SerializeField] private float maxFollowSpeed = 6f;

    private float velocityY;
    private bool registered = false;
    private List<GameObject> activeProjectiles;

    void Start()
    {
        activeProjectiles = new List<GameObject>();
        //ObstacleManager.Instance.RegisterObstacle(gameObject);
        //registered = true;

        //Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);

        StartCoroutine(FireRoutine());
    }

    void Update()
    {
        TrackPlayer();
    }

    void TrackPlayer()
    {
        float targetY = Player.Instance.transform.position.y;

        float newY = Mathf.SmoothDamp(
            transform.position.y,
            targetY,
            ref velocityY,
            followSmoothTime,
            maxFollowSpeed
        );

        transform.position = new Vector3(spawnX, newY, 0f);
    }

    IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);
        for (int i = 0; i < shotCount; i++)
        {
            FireProjectile();
            yield return new WaitForSeconds(fireInterval);
        }

        Cleanup();
    }

    void FireProjectile()
    {
        Vector2 origin = transform.position;

        Vector2 dir = (Player.Instance.transform.position - transform.position).normalized;

        GameObject obj = Instantiate(projectilePrefab, origin, Quaternion.identity);

        activeProjectiles.Add(obj);

        AOEProjectile proj = obj.GetComponent<AOEProjectile>();
        proj.Initialize(dir);
    }

    void Cleanup()
    {
        Destroy(gameObject);
        /*
        if (registered && ObstacleManager.Instance != null)
        {
            //ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
        }
        */
    }

    void OnDestroy()
    {   
        foreach(GameObject projectile in activeProjectiles)
        {
            if(projectile != null)
            {
                Destroy(projectile);
            }
        }
        /*
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
        */
    }
}