using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageSaveManager : RuntimeModifierManager<IDamageSave>
{
    public static DamageSaveManager Instance;


    [Header("Feedback")]
    [SerializeField] private ScreenShakeRequest screenShakeData;
    [SerializeField] private SoundEffect procSoundEffect;
    [SerializeField] private GameObject normalProcEffect;
    [SerializeField] private GameObject currencyProcEffect;

    [Header("Shields")]
    [SerializeField] private float shieldSizeStep = 0.3f;
    [SerializeField] private SoundEffect ignoreMissSoundEffect;
    [SerializeField] private GameObject ignoreMissVFXPrefab;


    private List<GameObject> _activeShieldObjects = new();

    public void CreateShieldVisual(GameObject shieldPrefab)
    {
        if (shieldPrefab != null)
        {
            GameObject shieldObject = Instantiate(shieldPrefab, Player.Instance.transform.position,Quaternion.identity);
            shieldObject.transform.localScale = Vector3.one * (1f + _activeShieldObjects.Count * shieldSizeStep);
            shieldObject.transform.SetParent(Player.Instance.transform);
            _activeShieldObjects.Add(shieldObject);
        }
    }

    public void ClearTopShield()
    {
        GameObject shield = _activeShieldObjects[^1];
        _activeShieldObjects.RemoveAt(_activeShieldObjects.Count - 1);
        Destroy(shield);
    }

    public void ClearAllSheilds()
    {
        for(int i = 0; i < _activeShieldObjects.Count; i++)
        {
            Destroy(_activeShieldObjects[i]);
        }

        _activeShieldObjects.Clear();
    }

    private void BreakShield()
    {
        AudioHelpers.PlaySoundEffect(ignoreMissSoundEffect, Camera.main.transform.position);

        if (ignoreMissVFXPrefab != null)
        {
            Instantiate(
                ignoreMissVFXPrefab,
                Player.Instance.transform.position,
                Quaternion.identity
            );
        }

        GameObject shield = _activeShieldObjects[^1];
        _activeShieldObjects.RemoveAt(_activeShieldObjects.Count - 1);
        Destroy(shield);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Subscribe()
    {
        Player.OnProcessHit += HandleProcessHit;
    }

    protected override void Unsubscribe()
    {
        Player.OnProcessHit -= HandleProcessHit;
        base.Unsubscribe();
    }

    private void Update()
    {
        RemoveExpiredShields();
    }

    private void RemoveExpiredShields()
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            RuntimeModifierEntry<IDamageSave> entry = activeModifiers[i];

            if (!entry.Modifier.HasExpired())
                continue;

            ExpireModifier(entry);

            // Remove corresponding shield visual.
            ClearTopShield();
        }
    }

    protected override void OnGameStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameStateManager.LevelStartState)
            CreateShields();

        if(newState == GameStateManager.LevelEndState)
            ClearAllSheilds();
    }

    public void RegisterGeneral(IDamageSave modifier)
    {
        if (modifier == null)
            return;

        Initialize();

        RuntimeModifierEntry<IDamageSave> entry = new RuntimeModifierEntry<IDamageSave>(
                modifier,
                null,
                removeCardWhenExpired: true
            );

        AddActiveModifier(entry);

        if(modifier.ShieldPrefab)
            CreateShieldVisual(modifier.ShieldPrefab);
    }


    private void HandleProcessHit(HitData hit)
    {
        if(!PullDamageSave(hit.Damage))
            return;

        AudioHelpers.PlaySoundEffect(procSoundEffect, Player.Instance.transform.position);
        ScreenShakeManager.Instance.Shake(screenShakeData);
    }


    // We create the shields in order of priority because then we dont have to about assining
    // them to their IDamageSave object. When we take damage we just sort by priority then check
    // If the current damage save has a shield object prefab and if it does, break it.
    private void CreateShields()
    {
        if (activeModifiers.Count <= 0)
            return;

        activeModifiers.Sort((x, y) => y.Modifier.Priority.CompareTo(x.Modifier.Priority));
        activeModifiers.Reverse();

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IDamageSave> damageSave = activeModifiers[i];
            if(damageSave.Modifier.ShieldPrefab != null)
            {
                CreateShieldVisual(damageSave.Modifier.ShieldPrefab);
            }
        }
    }

    private bool PullDamageSave(int damage)
    {
        if (activeModifiers.Count <= 0)
            return false;

        activeModifiers.Sort((x, y) => y.Modifier.Priority.CompareTo(x.Modifier.Priority));

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            RuntimeModifierEntry<IDamageSave> damageSave = activeModifiers[i];

            if (!damageSave.Modifier.CanPreventDamage(damage))
                continue;

            damageSave.Modifier.PreventDamage(damage);

            if(damageSave.Modifier.ShieldPrefab != null)
                BreakShield();


            HandleDamageSaveVisuals(damageSave.Modifier);

            ExpireModifier(damageSave);

            return true;
        }

        return false;
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