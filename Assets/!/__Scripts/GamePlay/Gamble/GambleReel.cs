using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GambleReel : MonoBehaviour
{
    public static event Action OnGambleReelClose;
    [Header("References")]
    [SerializeField] private RectTransform content;
    [SerializeField] private GambleRewardView rewardPrefab;
    [SerializeField] private GameObject reelRoot;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Rewards")]
    [SerializeField] private GambleRewardDatabase gambleRewardDatabase;
    [SerializeField] private int spinCost = 200;
    [SerializeField] private int rewardsAfterWinner = 4;

    [Header("Reel")]
    [SerializeField] private float baseSpacing = 140f;
    [SerializeField] private float weightSpacingScale = 1.0f;
    [SerializeField] private float extraSpacing = 20f;
    [SerializeField] private int extraLoops = 3;
    [SerializeField] private float startingHorizontalOffset = 500f;

    [Header("Animation")]
    [SerializeField] private float pointerX = 0f;
    [SerializeField] private float fastDuration = 2.5f;
    [SerializeField] private float slowDuration = 2.5f;

    [SerializeField, Range(0.5f, 0.95f)]
    private float fastDistancePercent = 0.8f;


    [Header("Reward Pointer")]
    [SerializeField] private float pointerAngle = 90f;
    [SerializeField] private RectTransform pointerVisual;
    [SerializeField] private RectTransform pointerMov;
    [SerializeField] private float pointerDistance = 220f;
    [SerializeField] private float pointerRotationOffset = -90f;
    
    [Header("Flick Pointer")]
    [SerializeField] private float flickAngle = -15f;
    [SerializeField] private float flickInDuration = 0.1f;
    [SerializeField] private float flickOutDuration = 0.1f;
    [SerializeField] private Ease inEase = Ease.InOutSine;
    [SerializeField] private Ease outEase = Ease.InOutSine;

    [Header("Button press")]
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private float pressDistance = 6f;
    [SerializeField] private float pressDuration = 0.06f;
    [SerializeField] private float releaseDuration = 0.12f;

  


    [Header("KeyPrompts")]
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private List<KeyPrompt> keyPrompts;

    [Header("Input")]
    [SerializeField] private InputActionType spinKey = InputActionType.Confirm;
    [SerializeField] private InputActionType closeKey = InputActionType.Jump;

    [Header("Reel Physics")]
    [SerializeField] private float reelPower = 3000f;
    [SerializeField] private float maxReelVelocity = 3500f;

    [SerializeField] private float maxStopPower = 800f;
    [SerializeField] private float minStopPower = 25f;

    [SerializeField] private float powerVariation = 0.1f;
    [SerializeField] private float rewardDelay = 0.4f;

    [Header("Audio")]
    [SerializeField] private SoundEffect failSoundEffect;

    private float _velocity;
    private float _currentMaxStopPower;
    private float _stoppedTimer;
        

    private readonly List<GambleRewardView> spawnedViews = new();

    private Tween spinTween;
    private bool _isOpen;
    private readonly List<float> _widths = new();
    private readonly List<float> _itemCenters = new();
    private DG.Tweening.Sequence _flickSequence;
    private float _pointerRestAngle;
    private Vector2 _buttonStartPosition;
    private DG.Tweening.Sequence _buttonSequence;
    private bool _buildReel;
    private bool _freePickOpen;

    private void OnEnable()
    {
        ShopManager.UpgradeSelectionComplete += HandleShopSelectionComplete;
    }

    private void OnDisable()
    {
        ShopManager.UpgradeSelectionComplete -= HandleShopSelectionComplete;
    }

    private void HandleShopSelectionComplete()
    {
        if(!_isOpen)
            return;

        if (!_freePickOpen)
            return;

        _freePickOpen = false;

       //InputBindingManager.Instance.BlockUntilRelease(spinKey);

        // Return control to the gamble screen.
        InputFocusManager.Claim(this);

        keyPromptController.PushPrompts(keyPrompts);
    }

    private void Awake()
    {
        _buttonStartPosition = buttonRect.anchoredPosition;
        _isOpen = false;
        _freePickOpen = false;
        buttonText.text = spinCost.ToString();
        reelRoot.SetActive(false);
    }

    public void Open()
    {
        _isOpen = true;

        InputFocusManager.Claim(this);

        keyPromptController.PushPrompts(keyPrompts);

        reelRoot.SetActive(true);

        
        BuildReel();
        _buildReel = false;
    }

    private void Close()
    {
        if(!_isOpen)
            return;

        InputFocusManager.Release(this);

        keyPromptController.PopPrompts();

        OnGambleReelClose?.Invoke();

        reelRoot.SetActive(false);
    }

    private void Update()
    {
        if (!InputFocusManager.HasFocus(this) || 
            !_isOpen || 
            _freePickOpen || 
            !reelRoot.gameObject.activeSelf)
            return;

        if (!spinning && InputBindingManager.Instance.GetKeyDown(spinKey))
        {
            TrySpin();
        }

        if (!spinning && InputBindingManager.Instance.GetKeyDown(closeKey))
        {
            Close();
        }

        if (!spinning)
            return;

        UpdateReelMovement();
        UpdateTick();
        CheckForStop();
    }

    private void UpdateReelMovement()
    {
        if (_velocity <= 0f)
            return;

        Vector2 position = content.anchoredPosition;

        position.x -= _velocity * Time.unscaledDeltaTime;

        content.anchoredPosition = position;

        float t = Mathf.InverseLerp(
            0f,
            maxReelVelocity,
            _velocity
        );

        // Strong braking while fast,
        // extremely weak braking near the end.
        float brake = Mathf.Lerp(
            minStopPower,
            _currentMaxStopPower,
            t
        );

        _velocity = Mathf.MoveTowards(
            _velocity,
            0f,
            brake * Time.unscaledDeltaTime
        );
    }

    private int GetRewardIndexAtPosition(float x)
    {
        if (_widths.Count == 0)
            return -1;

        float currentX = 0;

        for (int i = 0; i < _widths.Count; i++)
        {
            float minX = currentX;
            float maxX = currentX + _widths[i];

            if (x >= minX && x < maxX)
                return i;

            currentX += _widths[i];
        }

        return -1;
    }

    private void TrySpin()
    {
        bool purchased = CurrencyManager.Instance.TrySpendCurrency(spinCost);

        if(purchased)
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.purchase, Camera.main.transform.position);
            StartSpin();
            PlayButtonPress();
        }
        else
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.negative, Camera.main.transform.position);
        }
    }

    private void StartSpin()
    {
        if(_buildReel)
            BuildReel();

        _velocity =
            reelPower *
            (1f + UnityEngine.Random.Range(
                -powerVariation,
                powerVariation
            ));

        _velocity = Mathf.Min(
            _velocity,
            maxReelVelocity
        );

        _currentMaxStopPower =
            maxStopPower *
            (1f + UnityEngine.Random.Range(
                -powerVariation,
                powerVariation
            ));

        lastPassedIndex = -1;
        _stoppedTimer = 0f;
        spinning = true;
    }

    private GambleReward RollReward()
    {
        float totalWeight = 0f;

        List<GambleReward> rewardPool = gambleRewardDatabase.RewardPool;

        foreach (GambleReward reward in rewardPool)
            totalWeight += Mathf.Max(0f, reward.weight);

        float roll = UnityEngine.Random.Range(0f, totalWeight);

        foreach (GambleReward reward in rewardPool)
        {
            roll -= Mathf.Max(0f, reward.weight);

            if (roll <= 0f)
                return reward;
        }

        return rewardPool[^1];
    }

    private void GiveReward(GambleReward winningReward)
    {
        switch (winningReward.type)
        {
            case GambleRewardType.Reroll:
                RunStateManager.Instance.GrantShopReroll(winningReward.amount);
                break;
            case GambleRewardType.Currency:
                CurrencyManager.Instance.AddCurrency(winningReward.amount, silent: true);
                break;
            case GambleRewardType.Heal:
                Player.Instance.HealPlayer(winningReward.amount);
                break;
            case GambleRewardType.Nothing:
                AudioHelpers.PlaySoundEffect(failSoundEffect, Camera.main.transform.position);
                break;
            case GambleRewardType.FreeCard:
                OpenFreeCardReward(winningReward.amount);
                break;
        }
    }

    private void OpenFreeCardReward(int amount)
    {
        _freePickOpen = true;

        // Temporarily give up control while the shop is open.
        InputFocusManager.Release(this);

        // Remove gamble reel prompts while the shop's prompts are active.
        keyPromptController.PopPrompts();

        ShopManager.Instance.OpenFreePickShop(
            shopTitle: "FREE CARD",
            choices: 1,
            maxPurchases: 1
        );
    }

    private int winningIndex;

    private void BuildReel()
    {
        Clear();

        for (int loop = 0; loop < extraLoops; loop++)
        {
            List<GambleReward> loopRewards = new List<GambleReward>(gambleRewardDatabase.RewardPool);

            loopRewards.Shuffle();

            foreach (GambleReward reward in loopRewards)
            {
                SpawnReward(reward);
            }
        }

        content.anchoredPosition = new Vector2(startingHorizontalOffset, 0);
    }

    private void UpdateTick()
    {
        float pointerContentX = pointerX - content.anchoredPosition.x;

        int index = GetRewardIndexAtPosition(pointerContentX);

        if (index < 0 || index == lastPassedIndex)
            return;

        lastPassedIndex = index;

        FlickPointer();

        AudioHelpers.PlaySoundEffect(
            AudioLibrary.Instance.Database.navigate,
            transform.position
        );
    }

    private void CheckForStop()
    {
        if (_velocity > 0f)
        {
            _stoppedTimer = 0f;
            return;
        }

        _stoppedTimer += Time.unscaledDeltaTime;

        if (_stoppedTimer < rewardDelay)
            return;

        spinning = false;
        _stoppedTimer = 0f;
        _buildReel = true;

        GiveCurrentReward();
    }

    private void GiveCurrentReward()
    {
        float pointerContentX =
            pointerX - content.anchoredPosition.x;

        int index = GetRewardIndexAtPosition(pointerContentX);

        if (index < 0 ||
            index >= spawnedViews.Count)
            return;

        GambleReward reward = spawnedViews[index].Reward;

        GiveReward(reward);
    }

    private void Clear()
    {
        for (int i = 0; i < spawnedViews.Count; i++)
        {
            if (spawnedViews[i] != null)
                Destroy(spawnedViews[i].gameObject);
        }

        spawnedViews.Clear();
        _widths.Clear();
        _itemCenters.Clear();
    }

    private void SpawnReward(GambleReward reward)
    {
        GambleRewardView view =Instantiate(rewardPrefab,content,false);

        float width = baseSpacing * Mathf.Lerp(1.0f, reward.weight, weightSpacingScale);

        view.Setup(reward, width);

        width += extraSpacing;

        RectTransform rect = view.transform as RectTransform;

        float startX = _widths.Sum();
        float centerX = startX + width * 0.5f;
        

        rect.anchoredPosition =new Vector2(centerX,0f);

        spawnedViews.Add(view);

        _widths.Add(width);
        _itemCenters.Add(centerX);
    }

    /*
    private int GetRewardIndexAtPosition(float x)
    {
        float currentX = 0f;

        for (int i = 0; i < _widths.Count; i++)
        {
            float nextX =
                currentX + _widths[i];

            if (x >= currentX &&
                x < nextX)
            {
                return i;
            }

            currentX = nextX;
        }

        return -1;
    }
    */


    private IEnumerator AnimateToWinner()
    {
        if (winningIndex < 0 ||
            winningIndex >= _itemCenters.Count)
            yield break;

        float startX = content.anchoredPosition.x;
        float targetX = pointerX - _itemCenters[winningIndex];

        float slowStartX = Mathf.Lerp(
            startX,
            targetX,
            fastDistancePercent
        );

        spinTween?.Kill();

        lastPassedIndex = -1;
        spinning = true;

        DG.Tweening.Sequence sequence = DOTween.Sequence();

        // Quickly cover most of the reel.
        sequence.Append(
            content.DOAnchorPosX(
                slowStartX,
                fastDuration
            )
            .SetEase(Ease.OutQuad)
        );

        // Slowly crawl through the final section.
        sequence.Append(
            content.DOAnchorPosX(
                targetX,
                slowDuration
            )
            .SetEase(Ease.OutQuart)
        );

        spinTween = sequence;

        yield return sequence.WaitForCompletion();

        spinning = false;
    }

    private int lastPassedIndex = -1;
    private bool spinning;

    // Pointer
    //----------------------------------------------------------------------------------

    private void FlickPointer()
    {
        if (pointerMov == null)
            return;

        _flickSequence?.Kill();

        // Reset before every flick so interrupted animations do not accumulate.
        pointerMov.localRotation = Quaternion.Euler(0f, 0f, _pointerRestAngle);

        _flickSequence = DOTween.Sequence()
            .Append(
                pointerMov.DOLocalRotate(
                        new Vector3(0f, 0f, _pointerRestAngle + flickAngle),
                        flickInDuration,
                        RotateMode.Fast)
                    .SetEase(inEase)
            )
            .Append(
                pointerMov.DOLocalRotate(
                        new Vector3(0f, 0f, _pointerRestAngle),
                        flickOutDuration,
                        RotateMode.Fast)
                    .SetEase(outEase)
            )
            .SetLink(pointerMov.gameObject);
    }


    // Button press
    // ----------------------------------------------------------------------------------------




    private void PlayButtonPress()
    {
        _buttonSequence?.Kill();

        buttonRect.anchoredPosition = _buttonStartPosition;

        _buttonSequence = DOTween.Sequence();

        // Push down
        _buttonSequence.Append(
            buttonRect
                .DOAnchorPosY(
                    _buttonStartPosition.y - pressDistance,
                    pressDuration
                )
                .SetEase(Ease.OutQuad)
        );

        // Spring back up
        _buttonSequence.Append(
            buttonRect
                .DOAnchorPosY(
                    _buttonStartPosition.y,
                    releaseDuration
                )
                .SetEase(Ease.OutBack)
        );
    }
   
}