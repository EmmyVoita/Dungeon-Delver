using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class OrbitingShooterBlackholeSpawner : ChallengeBase
{
    [Header("Setup")]
    public Vector3 centerTarget;
    public GameObject blackholePrefab;

    [Header("Orbit Settings")]
    public int blackholeCount = 4;
    public float orbitRadius = 4f;
    public float orbitSpeed = 40f;

    [Header("Burst Settings")]
    public float burstInterval = 2f;
    public float lifetime = 10f;

    private List<BlackholeEmitter> _emitters = new();
    private bool registered;

    void Start()
    {
       Begin();
    }

    void Update()
    {
        if (centerTarget == null) return;

        transform.position = centerTarget;
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }

    void SpawnBlackholes()
    {
        for (int i = 0; i < blackholeCount; i++)
        {
            float angle = (float)i / blackholeCount * Mathf.PI * 2f;
            Vector3 localPos = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f
            ) * orbitRadius;

            GameObject obj = Instantiate(blackholePrefab, transform);
            obj.transform.localPosition = localPos;

            BlackholeEmitter emitter = obj.GetComponent<BlackholeEmitter>();
            emitter.centerTarget = centerTarget;
            _emitters.Add(emitter);
        }
    }

    IEnumerator BurstRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(burstInterval);

            if (_emitters.Count == 0) continue;

            int randomIndex = Random.Range(0, _emitters.Count);
            _emitters[randomIndex].FireBurst();
        }
    }

    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        End();
    }

    protected override void CleanUp()
    {
        base.CleanUp();
        
        foreach(BlackholeEmitter emitter in _emitters)
        {
            emitter.Cleanup();
        }
    }


    public override void Begin(object config = null)
    {
        base.Begin();

        SpawnBlackholes();
        StartCoroutine(BurstRoutine());
        StartCoroutine(LifetimeRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}