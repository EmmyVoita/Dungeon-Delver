using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BetweenRoundFlowManager : MonoBehaviour
{
    public static BetweenRoundFlowManager Instance { get; private set; }
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private RectTransform duplicateCardRootRect;
    [SerializeField] private DuplicateCardFilteredInventory duplicateCardUI;
    [SerializeField] private KeyPromptController promptController;
    [SerializeField] private GambleReel gambleReel;
    private Coroutine _flowRoutine;

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

        GameStateManager.Instance.SetState(GameState.WorldMapView);

        _flowRoutine = null;
    }

    public void StartNextRound()
    {
        //Debug.LogError("Starting next round from BetweenRoundFlowManager");

        promptController.Clear();
        RoundManager.Instance.SetupAndStartRound();
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
        
        GameStateManager.Instance.SetState(GameStateManager.PostRoundRewardState);
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

        GameStateManager.Instance.SetState(GameStateManager.PostRoundRewardState);
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
        ShopManager.Instance.OpenFreePickShop($"PICK {RunStateManager.Instance.FreePickAmount} CARDS", category, maxPurchases: RunStateManager.Instance.FreePickAmount);

        yield return new WaitUntil(() => complete);

        ShopManager.UpgradeSelectionComplete -= HandleComplete;

        yield return TransitionManager.Instance.PlayCloseShutter();
        StartCoroutine(TransitionManager.Instance.PlayOpenShutter());

        GameStateManager.Instance.SetState(GameStateManager.PostRoundRewardState);
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

        GameStateManager.Instance.SetState(GameStateManager.PostRoundRewardState);
    }

    private IEnumerator HandleBonusReward()
    {
        // Show / grant bonus reward here.

        yield return null;
    }
}