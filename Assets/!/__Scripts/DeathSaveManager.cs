using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathSaveManager : MonoBehaviour
{
    public static DeathSaveManager Instance;


    [Header("Feedback")]
    [SerializeField] private ScreenShakeRequest screenShakeData;
    [SerializeField] private SoundEffect procSoundEffect;
    [SerializeField] private GameObject procEffect;


    private bool _initialized;
    private List<IDeathSave> _deathSaves;
    private List<IDeathSave> _renewingDeathSaves;


    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        Player.OnPreDamageTaken -= HandleDamageTaken;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _deathSaves = new();
        _renewingDeathSaves = new();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        GameStateManager.OnStateChanged += HandleStateChanged;
        Player.OnPreDamageTaken += HandleDamageTaken;
    }


    // Remove temporary at the end of the round;
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameStateManager.LevelStartState)
        {
            foreach(IDeathSave item in _renewingDeathSaves)
            {
                Debug.Log("Adding renewing death save item");
                _deathSaves.Add(item.Clone());
            }
        }

        if(newState == GameStateManager.LevelEndState)
        {
            RemoveTemporary();
        }
    }

    public void RegisterRenewing(IDeathSave upgrade)
    {
        Initialize();

        _renewingDeathSaves.Add(upgrade);

        Debug.Log($"Adding death save upgrade \n"+
                  $"Priority => {upgrade.Priority} \n" +
                  $"Remove at level end => {upgrade.RemoveAtLevelEnd} \n");
    }


    public void Register(IDeathSave upgrade)
    {
        Initialize();

        _deathSaves.Add(upgrade);

        Debug.Log($"Adding death save upgrade \n"+
                  $"Priority => {upgrade.Priority} \n" +
                  $"Remove at level end => {upgrade.RemoveAtLevelEnd} \n");

    }

    private void  HandleDamageTaken(int damage)
    {
        Debug.Log($"Death Saves Active: {_deathSaves.Count}");

        if(!(Player.Instance.Health - damage <= 0))
            return;

        if(!PullDeathSave(damage))
            return;

        AudioHelpers.PlaySoundEffect(procSoundEffect, Player.Instance.transform.position);
        ScreenShakeManager.Instance.Shake(screenShakeData);

        if(procEffect != null)
            Instantiate(procEffect, Player.Instance.transform.position,Quaternion.identity);
    }

    private bool PullDeathSave(int damage)
    {
        if(_deathSaves.Count <= 0)
            return false;

        _deathSaves.Sort((x,y) => y.Priority.CompareTo(x.Priority));

        for(int i = 0; i < _deathSaves.Count; i++)
        {
            IDeathSave deathSave = _deathSaves[i];

            if(!deathSave.CanPreventDeath(damage))
                continue;

            deathSave.PreventDeath(damage);
            _deathSaves.RemoveAt(i);
            return true;
        }

        return false;
    }

    private void RemoveTemporary()
    {
        List<IDeathSave> savesToRemove = new();

        foreach(IDeathSave item in _deathSaves)
        {
            if(item.RemoveAtLevelEnd)
            {
                savesToRemove.Add(item);
            }
        }

        foreach(IDeathSave item in savesToRemove)
        {
            _deathSaves.Remove(item);
        }
    }
    
}