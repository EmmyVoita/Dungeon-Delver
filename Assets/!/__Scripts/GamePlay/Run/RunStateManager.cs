using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RunStateManager : MonoBehaviour
{
    public static RunStateManager Instance { get; private set; }

    public static event Action OnRefreshRerollsChanged;
    public static event Action OnUseReroll;
    public static event Action<int> OnRerollGained;

    

    public SoundEffect rerollSoundEffect;
    

    private readonly Dictionary<int, WorldMapRouteType> _routeHistory = new();

    public int ShopRerollsRemaining { get; private set; }
    
    public WorldMapRouteType CurrentRoute { get; private set; } = WorldMapRouteType.None;
    public WorldMapRouteType NextRoute { get; private set; } = WorldMapRouteType.None;
    public IReadOnlyDictionary<int, WorldMapRouteType> RouteHistory => _routeHistory;

    public int BonusWorldMapOptions { get; private set; }
    public float ExtraWorldMapOptionChance { get; private set; }
    public int FreePickAmount { get; private set; } = 1;

    public int BonusShopOptions { get; private set; }
    public float ExtraShopOptionChance { get; private set; }


 

    public void SetNextWorldMapRoute(WorldMapRouteType route)
    {
        NextRoute = route;
    }

    public void AdvanceWorldMapRoute()
    {
        CurrentRoute = NextRoute;
        NextRoute = WorldMapRouteType.None;
    }

    public void ClearCurrentWorldMapRoute()
    {
        CurrentRoute = WorldMapRouteType.None;
    }

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
        if(Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            GrantShopReroll(1);
        }
    }

    public void GrantShopReroll(int amount = 1)
    {
        ShopRerollsRemaining += amount;
        OnRefreshRerollsChanged?.Invoke();
        OnRerollGained?.Invoke(amount);
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.rerollGain, Camera.main.transform.position);
    }

    public bool CanRerollShop => ShopRerollsRemaining > 0;

    public bool ConsumeShopReroll()
    {
        if (ShopRerollsRemaining <= 0)
            return false;

        AudioHelpers.PlaySoundEffect(rerollSoundEffect, Camera.main.transform.position);

        ShopRerollsRemaining--;
  
        OnRefreshRerollsChanged?.Invoke();
        OnUseReroll?.Invoke();
        return true;
    }

    public void ResetRun()
    {
        ShopRerollsRemaining = 0;
        // reset other run-level state here
    }

    // World Map Route Count
    //---------------------------------------------------------------------------------

    public void AddWorldMapOptionChance(float amount)
    {
        ExtraWorldMapOptionChance += amount;
    }

    public void AddGuaranteedWorldMapOptions(int amount)
    {
        BonusWorldMapOptions += amount;
    }

    public int GetWorldMapOptionCount(int baseCount)
    {
        int result = baseCount + BonusWorldMapOptions;

        if (UnityEngine.Random.value <= ExtraWorldMapOptionChance)
            result++;

        return result;
    }

    // Shop count
    // ---------------------------------------------------------------------------------

    public void AddShopOptionChance(float amount)
    {
        ExtraShopOptionChance += amount;
    }

    public void AddGuaranteedShopOptions(int amount)
    {
        BonusShopOptions += amount;
    }

    public int GetShopOptionCount(int baseCount)
    {
        int result = baseCount + BonusShopOptions;

        if (UnityEngine.Random.value < ExtraShopOptionChance)
            result++;

        return result;
    }


    // Pick a free card count
    // ---------------------------------------------------------------------------------

    public void AddGuaranteedFreePickCardAmount(int amount)
    {
        FreePickAmount += amount;
    }


    // World Map Route History
    //---------------------------------------------------------------------------------

    public void SetRouteHistory(int levelIndex, WorldMapRouteType route)
    {
        _routeHistory[levelIndex] = route;
    }

    public bool TryGetRouteHistory(int levelIndex, out WorldMapRouteType route)
    {
        return _routeHistory.TryGetValue(levelIndex, out route);
    }

    public void ClearRouteHistory()
    {
        _routeHistory.Clear();
    }
}
