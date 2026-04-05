using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BounceBombSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bounceObject;

    [Header("Inscribed")]
    [SerializeField] private Vector2 spawnPosition;
    [SerializeField] private float exitDelay;

    [Tooltip("VFX event name to trigger")]
    public string playEventName = "OnPlay";
    
    private BounceBomb activeObject;
    [SerializeField] private List<VisualEffect> confettiEffects;
    [SerializeField] private SoundEffect congratsSoundEffect;


    void OnDisable()
    {
        BounceBomb.OnBombCleared -= HandleBombCleared;
    }

    void HandleBombCleared()
    {
        foreach(VisualEffect effect in confettiEffects)
        {
            effect.SendEvent(playEventName);
        }

        Debug.Log("Playing bounce bomb cleared");
        AudioHelpers.PlaySoundEffect(congratsSoundEffect,transform.position);
    }


    void Start()
    {
         ObstacleManager.Instance.RegisterObstacle(gameObject);
         StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        GameObject bounceObj = Instantiate(bounceObject, spawnPosition, Quaternion.identity);

        BounceBomb bomb = bounceObj.GetComponent<BounceBomb>();

        if(!bomb)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            yield return null;
        }

        float timer = bomb.LifeTime;

        yield return null;

        BounceBomb.OnBombCleared += HandleBombCleared;

        yield return new WaitForSeconds(timer + exitDelay);

        ObstacleManager.Instance.UnregisterObstacle(gameObject);

        Destroy(gameObject);
    }
}