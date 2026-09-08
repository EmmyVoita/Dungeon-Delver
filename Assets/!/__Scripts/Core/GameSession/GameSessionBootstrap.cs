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

        GameSceneLoader.PendingConfig = null; // prevent leakage

        
    }

    private void Start()
    {
        if(inventoryRoot != null)
            inventoryRoot.SetActive(false);
        ApplySessionConfig();
    }

    private void ApplySessionConfig()
    {
        Debug.Log($"Applying Session config for Game mode {Config.Mode}");
        switch (Config.Mode)
        {
            case GameMode.StandardRun:
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
        if(Config.CardsToGrant == 0)
        {
            GameStateManager.Instance.SetStateForceUpdate(GameState.RunIntro);
            yield break;
        }

        inventoryRoot.SetActive(true);

        ShopManager.Instance.GrantRandomCards(Config.CardsToGrant);
        
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