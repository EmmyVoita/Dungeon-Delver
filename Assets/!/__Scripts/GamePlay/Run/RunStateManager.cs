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

    private readonly Dictionary<StageLevelIndex, WorldMapRouteType> _routeHistory = new();
      private Dictionary<WorldMapRouteType, float> routeWeightMultipliers = new();

    public int ShopRerollsRemaining { get; private set; }

    public WorldMapRouteType CurrentRoute { get; private set; } = WorldMapRouteType.None;

    public IReadOnlyDictionary<StageLevelIndex, WorldMapRouteType> RouteHistory => _routeHistory;

    public int BonusWorldMapOptions { get; private set; }
    public float ExtraWorldMapOptionChance { get; private set; }

    public int FreePickAmount { get; private set; } = 1;

    public int BonusShopOptions { get; private set; }
    public float ExtraShopOptionChance { get; private set; }

    public void SelectWorldMapRoute(WorldMapRouteType route)
    {
        CurrentRoute = route;
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
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.T))
            {
                GrantShopReroll(1);
            }
        #endif
    }

    public void GrantShopReroll(int amount = 1)
    {
        ShopRerollsRemaining += amount;

        OnRefreshRerollsChanged?.Invoke();
        OnRerollGained?.Invoke(amount);

        AudioHelpers.PlaySoundEffect(
            AudioLibrary.Instance.Database.rerollGain,
            Camera.main.transform.position
        );
    }

    public bool CanRerollShop => ShopRerollsRemaining > 0;

    public bool ConsumeShopReroll()
    {
        if (ShopRerollsRemaining <= 0)
            return false;

        AudioHelpers.PlaySoundEffect(
            rerollSoundEffect,
            Camera.main.transform.position
        );

        ShopRerollsRemaining--;

        OnRefreshRerollsChanged?.Invoke();
        OnUseReroll?.Invoke();

        return true;
    }

    public void ResetRun()
    {
        ShopRerollsRemaining = 0;
        CurrentRoute = WorldMapRouteType.None;
        ClearRouteHistory();

        BonusWorldMapOptions = 0;
        ExtraWorldMapOptionChance = 0f;

        FreePickAmount = 1;

        BonusShopOptions = 0;
        ExtraShopOptionChance = 0f;
    }

    // World Map Route Count
    // ---------------------------------------------------------------------------------

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

    // Shop Count
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

    // Pick a Free Card Count
    // ---------------------------------------------------------------------------------

    public void AddGuaranteedFreePickCardAmount(int amount)
    {
        FreePickAmount += amount;
    }

    // World Map Route History
    // ---------------------------------------------------------------------------------

    public List<RouteRecord> GetRouteHistoryRecords()
    {
        List<RouteRecord> routeRecords = new();

        foreach (var kvp in RouteHistory)
        {
            RouteRecord record = new RouteRecord
            {
                routeType = kvp.Value,
                stageIndex = kvp.Key.stageIndex,
                levelIndex = kvp.Key.levelIndex
            };

            routeRecords.Add(record);
        }

        return routeRecords;
    }

    public void SetRouteHistory(StageLevelIndex stageLevelIndex,WorldMapRouteType route)
    {
        _routeHistory[stageLevelIndex] = route;
    }

    public bool TryGetRouteHistory(StageLevelIndex stageLevelIndex,out WorldMapRouteType route)
    {
        return _routeHistory.TryGetValue(stageLevelIndex, out route);
    }

    public void ClearRouteHistory()
    {
        _routeHistory.Clear();
    }

    // World map Weights

  

    public float GetRouteWeightMultiplier(WorldMapRouteType type)
    {
        return routeWeightMultipliers.TryGetValue(type, out float multiplier)? multiplier: 1f;
    }

    public void AddRouteWeightMultiplier(WorldMapRouteType type,float amount)
    {
        if (!routeWeightMultipliers.ContainsKey(type))
            routeWeightMultipliers[type] = 1f;

        routeWeightMultipliers[type] += amount;
    }
}