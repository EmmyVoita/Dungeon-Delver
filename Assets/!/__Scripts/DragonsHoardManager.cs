using UnityEngine;

public class DragonHoardManager : MonoBehaviour
{
    public static DragonHoardManager Instance;
    

    [SerializeField] private int goldenArrowBonusThreshold = 1000;
    [SerializeField] private float goldenArrowBonusMult = 2.0f;
    
    private DragonsCrownGoldenArrowWorthMultiplier _goldenArrowWorthModifier;
    private bool _prevGoldenActive;
    private int _dragonsCrowns;
    private int _prevDragonsCrowns;

    public float GoldenArrowMultiplier => goldenArrowBonusMult * _dragonsCrowns;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Activate()
    {
        CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        GameStateManager.OnStateChanged += HandleStateChanged;

        RefreshBonuses();
    }

    public void Deactivate()
    {
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
        GameStateManager.OnStateChanged -= HandleStateChanged;

        //RemoveBonuses();
    }

    private void HandleStateChanged(GameState previous, GameState current)
    {
        //if (current == GameStateManager.LevelStartState)
            //TryGrantLevelStartHitBlock();
    }

    private void HandleCurrencyChanged(int newCurrency)
    {
        RefreshBonuses();
    }

    public void AddDragonsCrown()
    {
        _dragonsCrowns++;

        RefreshBonuses();
    }

    private void OnCurrencyChanged(int amount)
    {
        RefreshBonuses();
    }

    private void RefreshBonuses()
    {
        GoldenArrowBonus();
    }
       

    private void GoldenArrowBonus()
    {
        int currentCurrency = CurrencyManager.Instance.CurrentCurrency;

        bool shouldBeActive = currentCurrency >= goldenArrowBonusThreshold &&
                              _dragonsCrowns > 0;

        bool shouldUpdate = _prevGoldenActive != shouldBeActive || 
                            _prevDragonsCrowns != _dragonsCrowns;

        // check whether or not whether it should be active has changed
        if(shouldUpdate)
        {
            if(_goldenArrowWorthModifier == null)
                _goldenArrowWorthModifier = new DragonsCrownGoldenArrowWorthMultiplier(GoldenArrowMultiplier);

            if(shouldBeActive)
            {
                GoldenArrowManager.Instance.AddWorthModifier(_goldenArrowWorthModifier);
            }
            else
            {
                GoldenArrowManager.Instance.RemoveWorthModifier(_goldenArrowWorthModifier);
            }
        }

        _prevGoldenActive = shouldBeActive;
        _prevDragonsCrowns = _dragonsCrowns;
    }
}