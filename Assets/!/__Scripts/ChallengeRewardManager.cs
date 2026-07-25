using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;

public class ChallengeRewardManager : RuntimeModifierManager<IChallengeReward>
{
    public static ChallengeRewardManager Instance;

    [SerializeField] private Material borderMaterial;
    [SerializeField] string transitionValuePropertyName = "_TransitionValue";
    [SerializeField] private float transitionDuration = 0.25f;

    private Tween _borderTween; 


    private List<IChallengeReward> _rewardsRolledToBeActive;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _rewardsRolledToBeActive = new();
    }


    protected override void Subscribe()
    {
        ObstacleManager.OnAllObstaclesCleared += HandleObstaclesCleared;
        ObstacleManager.OnFirstObstacleAppeared += HandleFirstObstacleSpawned;
    }

    protected override void Unsubscribe()
    {
        ObstacleManager.OnAllObstaclesCleared -= HandleObstaclesCleared;
        ObstacleManager.OnFirstObstacleAppeared -= HandleFirstObstacleSpawned;

        base.Unsubscribe();
    }

    public void RegisterOrStackCurrencyReward(
        int currencyReward,
        int maxUses = 999,
        float appearancePercentage = 1f)
    {
        Initialize();

        CurrencyChallengeReward existingReward =
            renewingModifiers
                .OfType<CurrencyChallengeReward>()
                .FirstOrDefault();

        if (existingReward != null)
        {
            existingReward.AddStack();
            return;
        }

        CurrencyChallengeReward newReward = new(
            currencyReward,
            maxUses,
            appearancePercentage
        );

        RegisterRenewing(newReward);
    }

    // We include a variable within the IChallengeReward interface for the chance that the effect triggers
    // because I want certin effects to only trigger sometimes. When one of these effects are active, I would
    // like to provide a boarder around the screen to act as a visual. 

    private void HandleFirstObstacleSpawned()
    {
        bool anyActive = RollActive();

        // Perform visual logic
        if(anyActive)
        {
            AnimateBorder(1.0f);
        }
    }

    private void HandleObstaclesCleared(int damageTaken)
    {
        PullRewards(damageTaken);
        _rewardsRolledToBeActive.Clear();
        AnimateBorder(0.0f);
    }

    // We want to be able to say that an modifier can trigger a max number of times per level,
    // and we might want to remove it at the end of the level or have it reset at the next level.
    // To accomplish this we can store a variable within the modifier for how much "health" it has. 
    // if it doesnt deminish by the end of the level we want to remove it, so we can just go a head 
    // and clear everything. Then, for modifiers that should persist between levels we just add them 
    // back to the current modifiers at the start of the level using a secondary.
    
    private bool PullRewards(int damage)
    {
        if (_rewardsRolledToBeActive.Count == 0)
            return false;

        bool grantedAny = false;

        _rewardsRolledToBeActive.Sort(
            (x, y) => y.Priority.CompareTo(x.Priority)
        );

        for (int i = 0; i < _rewardsRolledToBeActive.Count; i++)
        {
            IChallengeReward reward = _rewardsRolledToBeActive[i];

            if (!reward.ShouldGrantReward(damage))
                continue;

            bool granted = reward.GrantReward(damage);
            grantedAny |= granted;

            if (reward.UsesRemaining <= 0)
                activeModifiers.Remove(reward);
        }

        return grantedAny;
    }

    // If the challenge reward is rolled to be active, then we need a way of communicating
    // with the object that it should only return true if its currently active. One way to 
    // approach it is to set a variable within the IChallengeReward object for whether or not
    // it is currently active, and then we set that its inactive after the obstacles have been
    // cleared. Another way to do it would to be to define another IChallengeReward list for
    // the active IChallengeRewards when we loop through and roll for them to be active.
    // Then, when we check whether rewards should be granted we only loop througrh that seperate
    // list. When all objects have been cleared then we clear the list.


    private bool RollActive()
    {
        _rewardsRolledToBeActive.Clear();

        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            IChallengeReward reward = activeModifiers[i];

            if (UnityEngine.Random.value <= reward.AppearancePercentage)
            {
                _rewardsRolledToBeActive.Add(reward);
            }
        }

        return _rewardsRolledToBeActive.Count > 0;
    }

    protected override void RemoveTemporaryModifiers()
    {
        _rewardsRolledToBeActive.Clear();
        AnimateBorder(0f);

        base.RemoveTemporaryModifiers();
    }



    private void AnimateBorder(float targetValue)
    {
        _borderTween?.Kill();

        borderMaterial
            .DOFloat(
                targetValue,
                transitionValuePropertyName,
                transitionDuration)
            .SetEase(Ease.OutQuad);
    }
}