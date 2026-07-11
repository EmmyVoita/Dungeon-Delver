using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollectEffectsManager : MonoBehaviour
{
    public static CoinCollectEffectsManager Instance;


    [Header("Feedback")]
    [SerializeField] private SoundEffect procSoundEffect;

    private bool _initialized;
    private List<ICoinCollectEffect> _collectEffects;

    


    private void OnDisable()
    {
        CurrencyManager.OnCurrencyAdded -= HandleCurrencyAdded;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _collectEffects = new();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        CurrencyManager.OnCurrencyAdded += HandleCurrencyAdded;
    }


    // Remove temporary at the end of the round;
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        /*
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
        */
    }

    /*
    public void RegisterRenewing(IDamageSave upgrade)
    {
        Initialize();

        _renewingDamageSaves.Add(upgrade);

        Debug.Log($"Adding death save upgrade \n"+
                  $"Priority => {upgrade.Priority} \n" +
                  $"Remove at level end => {upgrade.RemoveAtLevelEnd} \n");
    }
    */

    public void Register(ICoinCollectEffect upgrade)
    {
        Initialize();

        _collectEffects.Add(upgrade);

        Debug.Log($"Adding death save upgrade \n"+
                  $"Priority => {upgrade.Priority} \n" +
                  $"Remove at level end => {upgrade.RemoveAtLevelEnd} \n");
    }

    private void  HandleCurrencyAdded(int amount)
    {
        if(!PullCurrencyEffect(amount))
            return;

        AudioHelpers.PlaySoundEffect(procSoundEffect, Player.Instance.transform.position);
    }

    private bool PullCurrencyEffect(int amount)
    {
        if(_collectEffects.Count <= 0)
            return false;

        _collectEffects.Sort((x,y) => y.Priority.CompareTo(x.Priority));

        for(int i = 0; i < _collectEffects.Count; i++)
        {
            ICoinCollectEffect effect = _collectEffects[i];

            if(!effect.CanTriggerEffect(amount))
                continue;

            
            effect.TriggerEffect(amount);
            return true;
        }

        return false;
    } 
}