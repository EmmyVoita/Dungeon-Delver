using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BetweenRoundFlowManager : MonoBehaviour
{
    public static BetweenRoundFlowManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private RectTransform duplicateCardRootRect;
    [SerializeField] private DuplicateCardFilteredInventory duplicateCardUI;
    [SerializeField] private KeyPromptController promptController;
    [SerializeField] private GambleReel gambleReel;

    [Header("Reward Timing")]
    [SerializeField] private RouteRewardTiming rewardTiming;


    private Coroutine _flowRoutine;


    public RouteRewardTiming RewardTiming => rewardTiming;

    private void Awake()
    {
        Instance = this;
        duplicateCardRootRect.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    // So, what we want to do is have two options for the between round sequence.
    // For the option where we let the player choose the reward at the end of the level
    // and immediatley give it to them. We go in this order
    //
    // round end -> world map view -> round reward -> pre round countdown -> round active
    //
    // Alternatively, we can make it so that the player chooses the reward for their next level
    // and that sequence goes like this
    //
    // round end -> round reward -> world map view -> pre round countdown -> round active

    // So our round manager will handle transitioning us from the round end to the correct next stage,
    // either round reward or world map view based on our RouteRewardTiming enum.

    // Currently we just transition to the world map view after the rewardroute has been handled,
    // but we need to change it so that it only transitions to the world map view in the case
    // of RouteRewardTiming being "AfterCurrentLevel". And, instead we transition to the 
    // pre-round countdown in the case of RouteRewardTiming being "BeforeNextLevel", or rather
    // RoundManager.SetupAndStartRound();

    private void RewardTransitionToNextState()
    {
        switch(RewardTiming)
        {
            case RouteRewardTiming.BeforeNextLevel:
                promptController.Clear();
                RoundManager.Instance.SetupAndStartRound();
            break;
                case RouteRewardTiming.AfterCurrentLevel:
                GameStateManager.Instance.SetState(GameState.WorldMapView);
            break;
        }
    }

    public void WorldMapTransitionToNextState()
    {
        switch(RewardTiming)
        {
            case RouteRewardTiming.BeforeNextLevel:
                GameStateManager.Instance.SetState(GameState.RoundReward);
            break;
                case RouteRewardTiming.AfterCurrentLevel:
                promptController.Clear();
                RoundManager.Instance.SetupAndStartRound();
            break;
        }
    }


    private void HandleStateChanged(GameState previousState,GameState newState)
    {
        if (newState != GameState.RoundReward)
            return;

        if (_flowRoutine != null)
            StopCoroutine(_flowRoutine);

        

        _flowRoutine = StartCoroutine(HandleBetweenRoundFlow());
    }

    private IEnumerator HandleBetweenRoundFlow()
    {
        WorldMapRouteType route = RunStateManager.Instance.CurrentRoute;

        // Wait for shutter transition
        yield return TransitionManager.Instance.PlayCloseShutter();
        StartCoroutine(TransitionManager.Instance.PlayOpenShutter());

        switch (route)
        {
            case WorldMapRouteType.SupportShop:
                yield return OpenShop(UpgradeCategory.Support);
                break;

            case WorldMapRouteType.AbilityShop:
                yield return OpenShop(UpgradeCategory.Ability);
                break;

            case WorldMapRouteType.CurrencyShop:
                yield return OpenShop(UpgradeCategory.Currency);
                break;

            case WorldMapRouteType.ChallengeShop:
                yield return OpenShop(UpgradeCategory.Challenge);
                break;

            case WorldMapRouteType.DefenseShop:
                yield return OpenShop(UpgradeCategory.Defensive);
                break;

            case WorldMapRouteType.GeneralShop:
                yield return OpenShop(UpgradeCategory.General);
                break;
            case WorldMapRouteType.ShopUtility:
                yield return OpenShop(UpgradeCategory.Shop);
                break;

            case WorldMapRouteType.BonusReward:
                yield return HandleBonusReward();
                break;
            case WorldMapRouteType.DuplicateCard:
                yield return OpenDuplicateInventory();
                break;
            case WorldMapRouteType.PickFreeCard:
                yield return OpenFreePickShop(UpgradeCategory.General);
                break;
            case WorldMapRouteType.GambleReel:
                yield return OpenGambleReel();
                break;

            case WorldMapRouteType.None:
                // First level behavior
                //GameStateManager.Instance.SetState(GameStateManager.PostRoundRewardState);
                //yield return OpenShop(UpgradeCategory.General);
                break;
        }

        RunStateManager.Instance.ClearCurrentWorldMapRoute();

        RewardTransitionToNextState();

        _flowRoutine = null;
    }

    public void StartNextRound()
    {
        //Debug.LogError("Starting next round from BetweenRoundFlowManager");

       
    }

    

    private IEnumerator OpenDuplicateInventory()
    {
        bool complete = false;

        void HandleComplete()
        {
            complete = true;
        }

        duplicateCardRootRect.gameObject.SetActive(true);

        DuplicateCardFilteredInventory.OnDuplicateSelectionComplete += HandleComplete;

        duplicateCardUI.TryOpen(keyPromptController);

        yield return new WaitUntil(() => complete);

        DuplicateCardFilteredInventory.OnDuplicateSelectionComplete -= HandleComplete;

        yield return TransitionManager.Instance.PlayCloseShutter();

        duplicateCardRootRect.gameObject.SetActive(false);

        StartCoroutine(TransitionManager.Instance.PlayOpenShutter());
    }
    

    private IEnumerator OpenShop(UpgradeCategory category)
    {
        bool complete = false;

        void HandleComplete()
        {
            complete = true;
        }

        ShopManager.UpgradeSelectionComplete += HandleComplete;

        //GameStateManager.Instance.SetState(GameState.RoundReward);

        // ShopManager can use the route/category,
        // or you can explicitly tell it here.
        ShopManager.Instance.OpenShop("SHOP", category);


        yield return new WaitUntil(() => complete);

        ShopManager.UpgradeSelectionComplete -= HandleComplete;

        yield return TransitionManager.Instance.PlayCloseShutter();
        StartCoroutine(TransitionManager.Instance.PlayOpenShutter());
    }

    private IEnumerator OpenFreePickShop(UpgradeCategory category)
    {
        bool complete = false;

        void HandleComplete()
        {
            complete = true;
        }

        ShopManager.UpgradeSelectionComplete += HandleComplete;

        //GameStateManager.Instance.SetState(GameState.RoundReward);

        // ShopManager can use the route/category,
        // or you can explicitly tell it here.
        ShopManager.Instance.OpenFreePickShop($"PICK {RunStateManager.Instance.FreePickAmount} CARDS", 
                                              category, 
                                              maxPurchases: RunStateManager.Instance.FreePickAmount,
                                              exclusions: new List<UpgradeCategory>() {UpgradeCategory.Challenge});

        yield return new WaitUntil(() => complete);

        ShopManager.UpgradeSelectionComplete -= HandleComplete;

        yield return TransitionManager.Instance.PlayCloseShutter();
        StartCoroutine(TransitionManager.Instance.PlayOpenShutter());
    }

    private IEnumerator OpenGambleReel()
    {
         bool complete = false;

        void HandleComplete()
        {
            complete = true;
        }

        GambleReel.OnGambleReelClose += HandleComplete;

        //GameStateManager.Instance.SetState(GameState.RoundReward);

        // ShopManager can use the route/category,
        // or you can explicitly tell it here.
        gambleReel.Open();

        yield return new WaitUntil(() => complete);

        GambleReel.OnGambleReelClose -= HandleComplete;

        yield return TransitionManager.Instance.PlayCloseShutter();
        StartCoroutine(TransitionManager.Instance.PlayOpenShutter());
    }

    private IEnumerator HandleBonusReward()
    {
        // Show / grant bonus reward here.

        yield return null;
    }
}