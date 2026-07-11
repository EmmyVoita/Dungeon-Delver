using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageSaveManager : MonoBehaviour
{
    public static DamageSaveManager Instance;


    [Header("Feedback")]
    [SerializeField] private ScreenShakeRequest screenShakeData;
    [SerializeField] private SoundEffect procSoundEffect;
    [SerializeField] private GameObject normalProcEffect;
    [SerializeField] private GameObject currencyProcEffect;


    private bool _initialized;
    private List<IDamageSave> _damageSaves;
    private List<IDamageSave> _renewingDamageSaves;


    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        Player.OnProcessHit -= HandleProcessHit;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _damageSaves = new();
        _renewingDamageSaves = new();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        GameStateManager.OnStateChanged += HandleStateChanged;
        Player.OnProcessHit += HandleProcessHit;
    }


    // Remove temporary at the end of the round;
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameStateManager.LevelStartState)
        {
            foreach(IDamageSave item in _renewingDamageSaves)
            {
                Debug.Log("Adding renewing death save item");
                _damageSaves.Add(item.Clone());
            }
        }

        if(newState == GameStateManager.LevelEndState)
        {
            RemoveTemporary();
        }
    }

    public void RegisterRenewing(IDamageSave upgrade)
    {
        Initialize();

        _renewingDamageSaves.Add(upgrade);

        Debug.Log($"Adding death save upgrade \n"+
                  $"Priority => {upgrade.Priority} \n" +
                  $"Remove at level end => {upgrade.RemoveAtLevelEnd} \n");
    }


    public void Register(IDamageSave upgrade)
    {
        Initialize();

        _damageSaves.Add(upgrade);

        Debug.Log($"Adding death save upgrade \n"+
                  $"Priority => {upgrade.Priority} \n" +
                  $"Remove at level end => {upgrade.RemoveAtLevelEnd} \n");

    }

    private void  HandleProcessHit(HitData hit)
    {
        Debug.Log($"Death Saves Active: {_damageSaves.Count}");

        if(!PullDamageSave(hit.Damage))
            return;

        AudioHelpers.PlaySoundEffect(procSoundEffect, Player.Instance.transform.position);
        ScreenShakeManager.Instance.Shake(screenShakeData);

        //if(procEffect != null)
            //Instantiate(procEffect, Player.Instance.transform.position,Quaternion.identity);
    }

    private bool PullDamageSave(int damage)
    {
        if(_damageSaves.Count <= 0)
            return false;

        _damageSaves.Sort((x,y) => y.Priority.CompareTo(x.Priority));

        for(int i = 0; i < _damageSaves.Count; i++)
        {
            IDamageSave damageSave = _damageSaves[i];

            if(!damageSave.CanPreventDamage(damage))
                continue;

            
            damageSave.PreventDamage(damage);
            _damageSaves.RemoveAt(i);
            HandleDamageSaveVisuals(damageSave);
            return true;
        }

        return false;
    }

    private void RemoveTemporary()
    {
        List<IDamageSave> savesToRemove = new();

        foreach(IDamageSave item in _damageSaves)
        {
            if(item.RemoveAtLevelEnd)
            {
                savesToRemove.Add(item);
            }
        }

        foreach(IDamageSave item in savesToRemove)
        {
            _damageSaves.Remove(item);
        }
    }

    private void HandleDamageSaveVisuals(IDamageSave save)
    {
        switch(save)
        {
            case CurrencyDamageSave:
                if(currencyProcEffect != null)
                    Instantiate(currencyProcEffect, Player.Instance.transform.position,Quaternion.identity);
                break;

            case ShieldDamageSave:
                if(normalProcEffect != null)
                    Instantiate(normalProcEffect, Player.Instance.transform.position,Quaternion.identity);
                break;
        }
    }
    
}