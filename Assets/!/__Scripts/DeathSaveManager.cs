using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeathSaveManager : RuntimeModifierManager<IDeathSave>
{
    public static DeathSaveManager Instance;


    [Header("Feedback")]
    [SerializeField] private ScreenShakeRequest screenShakeData;
    [SerializeField] private SoundEffect procSoundEffect;
    [SerializeField] private GameObject procEffect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    protected override void Unsubscribe()
    {
        Player.OnPreDamageTaken -= HandleDamageTaken;
        base.Unsubscribe();
    }

    protected override void Subscribe()
    {
        Player.OnPreDamageTaken += HandleDamageTaken;
    }



    private void  HandleDamageTaken(int damage)
    {
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
        if(activeModifiers.Count <= 0)
            return false;

        activeModifiers.Sort((x,y) => y.Modifier.Priority.CompareTo(x.Modifier.Priority));

        for(int i = 0; i < activeModifiers.Count; i++)
        {
            IDeathSave deathSave = activeModifiers[i].Modifier;

            if(!deathSave.CanPreventDeath(damage))
                continue;

            deathSave.PreventDeath(damage);
            activeModifiers.RemoveAt(i);
            return true;
        }

        return false;
    }
}