using UnityEngine;
using System;

public class RunStateManager : MonoBehaviour
{
    public static RunStateManager Instance { get; private set; }

    public static event Action OnRefreshRerollsChanged;

    public int ShopRerollsRemaining { get; private set; }

    public SoundEffect rerollSoundEffect;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            GrantShopReroll(1);
        }
    }

    public void GrantShopReroll(int amount = 1)
    {
        ShopRerollsRemaining += amount;
        OnRefreshRerollsChanged?.Invoke();
    }

    public bool CanRerollShop => ShopRerollsRemaining > 0;

    public bool ConsumeShopReroll()
    {
        if (ShopRerollsRemaining <= 0)
            return false;

        AudioHelpers.PlaySoundEffect(rerollSoundEffect, Camera.main.transform.position);

        ShopRerollsRemaining--;
        OnRefreshRerollsChanged?.Invoke();
        return true;
    }

    public void ResetRun()
    {
        ShopRerollsRemaining = 0;
        // reset other run-level state here
    }
}
