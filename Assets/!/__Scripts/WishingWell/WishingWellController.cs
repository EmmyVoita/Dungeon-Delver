using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework.Constraints;
using UnityEngine;

public class WishingWellController : MonoBehaviour
{
    public static WishingWellController Instance;

    [Header("Rewards")]
    [SerializeField] private List<RewardDefinition> rewards;

    [Header("Coin Toss")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinStartPoint;
    [SerializeField] private Transform coinLandingPoint;
    [SerializeField] private Transform effectParent;

    [SerializeField] private float tossDuration = 0.8f;
    [SerializeField] private float tossHeight = 2f;
    [SerializeField] private float coinRotations = 4f;
    [SerializeField] private float landingVariation = 1.0f;

    [Header("Landing Feedback")]
    [SerializeField] private ParticleSystem splashParticles;
    [SerializeField] private SoundEffect coinTossSound;
    [SerializeField] private SoundEffect splashSound;
    [SerializeField] private float rewardRevealDelay = 0.25f;
    [SerializeField] private WaterSplashEffect splashPrefab;

    [Header("Reward Icons")]
    [SerializeField] private FloatingRewardIcon rewardIconPrefab;
    [SerializeField] private FloatingRewardIcon failureEffectPrefab;
    [SerializeField] private Transform rewardSpawnPoint;
    [SerializeField] private float iconSpawnInterval = 0.1f;
    [SerializeField] private float horizontalSpacing = 0.4f;
    [SerializeField] private float rewardSpawnVariation = 1.0f;
    [SerializeField] private float rewardApplyDelay = 2f;


    [Header("References")]
    [SerializeField] private GameObject wellParent;
    [SerializeField] private float closeDelay;
    [SerializeField] private BounceOnConfirm bounceText;

    [Header("Input")]
    [SerializeField] private InputActionType tossKey = InputActionType.Confirm;


    private bool _isPlaying;
    private Vector3 _coinLandingPosition;
    private bool _canTossCoin;
    private RewardDefinition _selectedReward;

    public bool IsPlaying => _isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        wellParent.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            PlaySequence();
        }

        if(_canTossCoin && InputFocusManager.HasFocus(this) && InputBindingManager.Instance.GetKeyDown(tossKey))
        {
            TossCoin(_selectedReward);
            _canTossCoin = false;
        }
    }


    public void PlaySequence(List<RewardDefinition> rewards = null)
    {
        if (_isPlaying)
            return;

        if(rewards != null)
            this.rewards = rewards;

        wellParent.SetActive(true);

        _selectedReward = RollReward();

        if (_selectedReward == null)
            return;

        _isPlaying = true;

        UpgradeCardManager.Instance.HideUpgradeUI();

        InputFocusManager.Claim(this);

        StartCoroutine(InputStartDelay());

        bounceText.Show();
    }

    private IEnumerator InputStartDelay()
    {
        yield return new WaitForSeconds(0.2f);
        _canTossCoin = true;
    }

    private void TossCoin(RewardDefinition reward)
    {
        GameObject coin = Instantiate(
            coinPrefab,
            coinStartPoint.position,
            Quaternion.identity,
            effectParent
        );

        AudioHelpers.PlaySoundEffect(
            coinTossSound,
            Camera.main.transform.position
        );

        Sequence sequence = DOTween.Sequence();

        Vector2 unityCirclePos = Random.insideUnitCircle * landingVariation; 
        _coinLandingPosition = coinLandingPoint.position + new Vector3(unityCirclePos.x,unityCirclePos.y,0);

        sequence.Join(
            coin.transform
                .DOJump(
                    _coinLandingPosition,
                    tossHeight,
                    1,
                    tossDuration
                )
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            coin.transform
                .DORotate(
                    new Vector3(0f, 0f, 360f * coinRotations),
                    tossDuration,
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.Linear)
        );

        sequence.Insert(
            tossDuration * 0.65f,
            coin.transform
                .DOScale(0.5f, tossDuration * 0.35f)
                .SetEase(Ease.InQuad)
        );

        sequence.OnComplete(() =>
        {
            Destroy(coin);
            HandleCoinLanding(reward);
        });
    }

    private void HandleCoinLanding(RewardDefinition reward)
    {
        if (splashParticles != null)
            splashParticles.Play();

        AudioHelpers.PlaySoundEffect(
            splashSound,
            Camera.main.transform.position
        );

        WaterSplashEffect splash = Instantiate(
            splashPrefab,
            _coinLandingPosition,
            Quaternion.identity
        );

        splash.Play();

        StartCoroutine(RevealRewardRoutine(reward));
    }

    private IEnumerator RevealRewardRoutine(RewardDefinition reward)
    {
        yield return new WaitForSeconds(rewardRevealDelay);

        

        int iconCount = reward.Amount;

        if (iconCount <= 0)
        {
            SpawnFailureEffect();
        }
        else
        {
            yield return SpawnRewardIcons(iconCount, reward);
        }

        _isPlaying = false;

        yield return new WaitForSeconds(rewardApplyDelay);

        reward.Apply();

        yield return new WaitForSeconds(closeDelay);

        Cleanup();

    
    }

    private IEnumerator SpawnRewardIcons(int amount, RewardDefinition reward)
    {
        for (int i = 0; i < amount; i++)
        {
            float centeredIndex = i - (amount - 1) * 0.5f;

           Vector2 unityCirclePos = Random.insideUnitCircle * rewardSpawnVariation; 
           Vector3 spawnPos = rewardSpawnPoint.position + new Vector3(unityCirclePos.x,unityCirclePos.y,0);

            Vector3 spawnPosition =
                spawnPos +
                Vector3.right * centeredIndex * horizontalSpacing;

            FloatingRewardIcon icon = Instantiate(
                rewardIconPrefab,
                spawnPosition,
                Quaternion.identity,
                effectParent
            );

            icon.Play(reward);

            yield return new WaitForSeconds(iconSpawnInterval);
        }
    }

    private void SpawnFailureEffect()
    {
        if (failureEffectPrefab == null)
            return;

        FloatingRewardIcon effect = Instantiate(
            failureEffectPrefab,
            rewardSpawnPoint.position,
            Quaternion.identity,
            effectParent
        );

        effect.Play();
    }

    private RewardDefinition RollReward()
    {
        if (rewards == null || rewards.Count == 0)
        {
            Debug.LogWarning("The wishing well has no rewards.");
            return null;
        }

        float totalWeight = 0f;

        foreach (RewardDefinition reward in rewards)
        {
            if (reward == null)
                continue;

            totalWeight += Mathf.Max(0f, reward.Weight);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("All wishing-well reward weights are zero.");
            return null;
        }

        float roll = Random.Range(0f, totalWeight);

        foreach (RewardDefinition reward in rewards)
        {
            if (reward == null)
                continue;

            roll -= Mathf.Max(0f, reward.Weight);

            if (roll <= 0f)
                return reward;
        }

        return rewards[^1];
    }

    private void Cleanup()
    {
        wellParent.SetActive(false);

        UpgradeCardManager.Instance.ShowUpgradeUI();

        InputFocusManager.Release(this);
    }
}