using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BounceBombSpawner : ChallengeBase
{
    [Header("References")]
    [SerializeField] private GameObject bounceObject;

    [Header("Inscribed")]
    [SerializeField] private Vector2 spawnPosition;
    [SerializeField] private float exitDelay;

    [Tooltip("VFX event name to trigger")]
    public string playEventName = "OnPlay";
    
    private GameObject _bombObj;
    [SerializeField] private SoundEffect congratsSoundEffect;


    void OnDisable()
    {
        BounceBomb.OnBombCleared -= HandleBombCleared;
    }

    void Start()
    {
        Begin();
    }

    private IEnumerator SpawnSequence()
    {
        _bombObj = Instantiate(bounceObject, spawnPosition, Quaternion.identity);

        BounceBomb bomb = _bombObj.GetComponent<BounceBomb>();

        if(!bomb)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            yield return null;
        }

        float timer = bomb.LifeTime;

        yield return null;

        BounceBomb.OnBombCleared += HandleBombCleared;

        yield return new WaitForSeconds(timer + exitDelay);

        End();
    }

    void HandleBombCleared()
    {
        ConfettiEffect.TriggerConfetti();
        AudioHelpers.PlaySoundEffect(congratsSoundEffect,transform.position);
    }

    protected override void CleanUp()
    {
        Destroy(_bombObj);
    }
  
    public override void Begin(object config = null)
    {
        base.Begin();
        StartCoroutine(SpawnSequence());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}