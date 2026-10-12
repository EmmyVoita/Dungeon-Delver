using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameSessionBootstrap : MonoBehaviour
{
    public static GameSceneConfig Config { get; private set; }
    public InventoryBase startInventory;
    public GameObject inventoryRoot;
    public KeyPromptController keyPromptController;

    private void Awake()
    {
        Config = GameSceneLoader.PendingConfig ?? Config ?? new GameSceneConfig();
        GameSceneLoader.PendingConfig = null; 
    }

    private void Start()
    {
        if(inventoryRoot != null)
            inventoryRoot.SetActive(false);
        ApplySessionConfig();
    }

    private void ApplySessionConfig()
    {
        switch (Config.Mode)
        {
            case GameMode.StandardRun:
                if(Config.GoldenArrowChance > 0)
                    OnArrowSpawnedRMM.Instance.RegisterPermanent(new RandomGoldenArrowModifier(Config.GoldenArrowChance));

                if(Config.AdditionalGeneralShopWeight > 0)
                    RunStateManager.Instance.AddRouteWeightMultiplier(WorldMapRouteType.GeneralShop, Config.AdditionalGeneralShopWeight);
                StartCoroutine(HandleStartingInventory());
                break;

            case GameMode.ObstaclePractice:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Practice);
                break;

            case GameMode.ObstaclePracticeBoss:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Practice);
                break;

            case GameMode.LevelEditorTest:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Editor);
                break;

            case GameMode.Tutorial:
                GameStateManager.Instance.SetStateForceUpdate(GameState.Tutorial);
                break;
            default:
                Debug.LogError($"Config mode when loading the game session was incorrect => {Config.Mode}");
                break;
        }

        RunStateManager.Instance.AddGuaranteedWorldMapOptions(Config.AdditionalRewardRoutes);
    }

    private IEnumerator HandleStartingInventory()
    {
        GameStateManager.Instance.SetStateForceUpdate(GameState.None);

        if(Config.CardsToGrant == 0)
        {
            GameStateManager.Instance.SetStateForceUpdate(GameState.RunIntro);
            yield break;
        }

        inventoryRoot.SetActive(true);

        //ShopManager.Instance.GrantRandomCards(Config.CardsToGrant);

        ShopManager.Instance.GrantSetCards(Config.CardsToGrant);
        
        bool complete = false;

        void HandleComplete(bool state)
        {
            if(state == false)
                complete = true;
        }

        startInventory.TryOpen(keyPromptController);

        InventoryBase.OnInventoryStateChanged += HandleComplete;

        yield return new WaitUntil(() => complete);

        InventoryBase.OnInventoryStateChanged -= HandleComplete;

        inventoryRoot.SetActive(false);

        GameStateManager.Instance.SetStateForceUpdate(GameState.RunIntro);
    }
}