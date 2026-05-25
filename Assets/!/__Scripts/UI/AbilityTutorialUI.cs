using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System;

public class AbilityTutorialUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArrowTypeDefinition normalArrowType;
    [SerializeField] private TextTypewriter typewriter;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private RectTransform typewriterContianer;
    [SerializeField] private TextMeshProUGUI hintText;
    

    [Header("Normal/Critical Demonstration Settings")]
    [SerializeField] private float demoSpawnInterval = 2.7f;
    [SerializeField] private float demoArrowSpeed = 5f;
    [SerializeField] private float continueTextDelay = 5.0f;
    [SerializeField] private float criticalCatchDemoDelay = 1.8f;


    [Header("Player Inactive State")]
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 1.0f);   
    [SerializeField] private float playerColorChangeDuration = 0.4f;


    [Header("Charging Ability Sequence")]
    [SerializeField] private int playerMaxAbilityCharge = 10;
    [SerializeField] private float arrowSpawnInterval = 1.2f;

    [Header("Hint Text")]
    [SerializeField] private float hintTextFadeDuration = 0.5f;
    
    [Header("Tutorial Text")]
    [SerializeField] private float tutorialTextFadeDuration = 0.5f;

    private bool _tutorialComplete = false;
    private bool _completed = false;
    private bool _lockChargeChanged = false;
    private bool _allowAbilityTextUpdates = false;

    private Coroutine _criticalCatchDemoCoroutine;
    private Tween _idleWobbleTween;
    private Vector3 _basePos;


    public bool TutorialComplete => _completed;


    private Vector2[] spawnDirections = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    void OnEnable()
    {
        Player.OnAbilityChargeChanged += HandleAbilityChargeChanged;
        ArrowBase.OnArrowResolved += HandleArrowResolved;
    }

    void OnDisable()
    {
        Player.OnAbilityChargeChanged -= HandleAbilityChargeChanged;
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
    }

    private void HandleAbilityChargeChanged(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        // Check if bar is full
        if (Player.Instance.AbilityCharge >= Player.Instance.MaxAbilityCharge && !_tutorialComplete && !_lockChargeChanged)
        {
            _tutorialComplete = true;
            float chargePercent = (float)Player.Instance.AbilityCharge / Player.Instance.MaxAbilityCharge;
            int filledPercent = Mathf.RoundToInt(chargePercent * 100f);
            typewriter.SetInstant($"Fill ability bar [<color=#FFD84C>{filledPercent}%</color>]"); // force update
            StartCoroutine(OnTutorialComplete());
        }

        typewriterContianer.PlayJumpDip(baseY: _basePos.y,
                                        dipAmount: 15f,
                                        dipDuration: 0.15f,
                                        returnDuration: 0.25f);
    }


    private void HandleArrowResolved(ArrowResolvedData data)
    {
        if(!_allowAbilityTextUpdates || _tutorialComplete)
            return;

        if(data.goalType == Goal.GoalType.Critical)
        {
            UpdateAbilityChargeText();
        }
    }

    void Start()
    {
        _completed = false;
        _basePos = tutorialText.rectTransform.anchoredPosition;
        tutorialText.alpha = 1f;
        Player.Instance.MaxAbilityCharge = playerMaxAbilityCharge;

        string keyName = InputBindingManager.Instance.GetKeyName(InputActionType.Confirm).ToString();
        string hint = $"\n<size=70%><color=#aaaaaa>Press [{keyName}] to continue</color></size>";
        hintText.text = hint;
        hintText.alpha = 0f;

        StartCoroutine(RunCriticalCatchTutorial());
        StartIdleWobble();
    }


    private IEnumerator RunCriticalCatchTutorial()
    {
        // 🟡 Demonstration loops
        Player.Instance.lockInput = true;
        _lockChargeChanged = true;
        yield return StartCoroutine(ShowCatchDemonstration());
        
        _lockChargeChanged = false;
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("Charge your ability"));
        
        // Start spawning arrows
        StartCoroutine(SpawnArrowsUntilFilled());
    }

    private IEnumerator RunDemonstration(
    string text,
    Action onArrowSpawned = null
    )
    {
        bool continuePressed = false;
        bool hintShown = false;

        float elapsed = 0f;
        float arrowTimer = 0f;

        typewriter.StartTyping(text);

        while (!continuePressed)
        {
            elapsed += Time.deltaTime;
            arrowTimer += Time.deltaTime;

            if (arrowTimer >= demoSpawnInterval)
            {
                arrowTimer = 0f;

                SpawnTutorialArrow(Vector2.up);

                onArrowSpawned?.Invoke();
            }

            if (!hintShown && elapsed >= continueTextDelay)
            {
                hintShown = true;
                hintText.DOColor(Color.white, hintTextFadeDuration);
            }

            if (elapsed >= continueTextDelay &&
                InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            {
                continuePressed = true;
            }

            yield return null;
        }

        hintText.DOColor(Color.clear, hintTextFadeDuration);

        tutorialText.DOFade(0f, tutorialTextFadeDuration);
        yield return new WaitForSeconds(0.3f);
        tutorialText.DOFade(1f, tutorialTextFadeDuration);
        ArrowSpawner.Instance.ClearAllArrows();
    }

    private IEnumerator ShowCatchDemonstration()
    {
        SpriteRenderer playerSR = Player.Instance.spriteObj.GetComponent<SpriteRenderer>();
        SpriteRenderer goalSR = Player.Instance.goal.GetComponentInChildren<Goal>().sRend;

        // --- NORMAL CATCH DEMO ---
        Player.Instance.SetFullyLocked(true);
        Player.Instance.goal.GetComponentInChildren<Goal>().SetGoalDirection(Vector2.up);

        // Fade player to gray
        playerSR.DOColor(inactiveColor, playerColorChangeDuration);
        goalSR.DOColor(inactiveColor, playerColorChangeDuration);

        Player.Instance.AbilityChargeLocked = true;


        yield return RunDemonstration(
            "A normal catch is holding the goal in the right direction."
        );

        yield return RunDemonstration(
            "A critical catch is moving into the arrow just before it hits.",
            () =>
            {
                if (_criticalCatchDemoCoroutine != null)
                    StopCoroutine(_criticalCatchDemoCoroutine);

                _criticalCatchDemoCoroutine = StartCoroutine(SimulateCriticalCatch());
            }
        );
        
        Player.Instance.AbilityChargeLocked = false;

        // Fade back to white
        playerSR.DOColor(Color.white, playerColorChangeDuration);
        goalSR.DOColor(Color.white, playerColorChangeDuration);

        // --- ABILITY BAR EXPLANATION ---
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("Critical catches charge your ability bar on the left."));

        Player.Instance.AbilityCharge = 0;
        Player.Instance.SetFullyLocked(false);
    }

    private void SpawnTutorialArrow(Vector2 direction)
    {
        float currentTime =
            (float)MusicManager.Instance.ScaledElapsedTime;

        float travelTime =
            ArrowSpawner.Instance.SpawnDistance / demoArrowSpeed;

        ArrowSpawner.Instance.SpawnArrow(
            direction,
            demoArrowSpeed,
            currentTime,
            currentTime + travelTime,
            normalArrowType.displayName,
            damageOverride: 0
        );
    }



    private IEnumerator SimulateCriticalCatch()
    {
        Player.Instance.goal.GetComponentInChildren<Goal>().SetGoalDirection(Vector2.right);
        yield return new WaitForSeconds(criticalCatchDemoDelay);
        Player.Instance.goal.GetComponentInChildren<Goal>().SetGoalDirection(Vector2.up);
    }



    // --------------------------------------------------------
    // 🧩 Helper coroutine — waits for typing, then waits for user input
    // --------------------------------------------------------
    private IEnumerator PlayTypewriterLineWaitForInput(string text)
    {
        string keyName = InputBindingManager.Instance.GetKeyName(InputActionType.Confirm).ToString();
        string hint = $"\n<size=70%><color=#aaaaaa>Press [{keyName}] to continue</color></size>";

        bool done = false;

        typewriter.StartTyping(text, () => done = true);

        // Wait for typing to finish
        yield return new WaitUntil(() => done);

        // Reveal suffix AFTER typing
        hintText.DOColor(Color.white, hintTextFadeDuration);

        // Wait for confirm
        yield return new WaitUntil(() =>
            InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm)
        );

        hintText.DOColor(Color.clear, hintTextFadeDuration);

        // Fade polish
        tutorialText.DOFade(0f, tutorialTextFadeDuration);
        yield return new WaitForSeconds(0.3f);
        tutorialText.DOFade(1f, tutorialTextFadeDuration);
    }




    private IEnumerator SpawnArrowsUntilFilled()
    {
        _allowAbilityTextUpdates = true;
        UpdateAbilityChargeText();

        while (!_tutorialComplete)
        {
            int dir = UnityEngine.Random.Range(0, 4);
            Vector2 direction = spawnDirections[dir];

            SpawnTutorialArrow(direction);

            yield return new WaitForSeconds(arrowSpawnInterval);
        }
    }

  

    void UpdateAbilityChargeText()
    {
        float chargePercent = (float)Player.Instance.AbilityCharge / Player.Instance.MaxAbilityCharge;
        int filledPercent = Mathf.RoundToInt(chargePercent * 100f);

        typewriter.SetInstant(
            $"Fill ability bar [<color=#FFD84C>{filledPercent}%</color>]"
        );
    }




    private IEnumerator OnTutorialComplete()
    {
        _allowAbilityTextUpdates = false;

        //Destroy remainng arrows
        ArrowSpawner.Instance.ClearAllArrows();

        yield return new WaitForSeconds(0.5f);

        // Tell player they filled their ability bar
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("Great!"));

        string keyName = InputBindingManager.Instance.GetKeyName(InputActionType.UseAbility).ToString();
        typewriter.StartTyping($"Press [{keyName}] to use your ability.", () =>
        {
            Player.Instance.lockInput = false;
        });

        // 🟣 Wait for the player to use their ability
        bool used = false;
        System.Action handler = () => used = true;
        Player.OnAbilityUsed += handler;

        yield return new WaitUntil(() => used);
        Player.OnAbilityUsed -= handler;

        yield return StartCoroutine(PlayTypewriterLineWaitForInput("This ability slows time briefly when used"));
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("You might want to slow time when you are feeling pressured"));
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("You can unlock other abilities as you progress"));

        ConfettiEffect.TriggerConfetti();

        typewriter.textComponent.text = "";

        _completed = true;
    }


    private void StartIdleWobble()
    {
        _idleWobbleTween?.Kill();
        _idleWobbleTween = tutorialText.rectTransform
            .DOAnchorPosY(_basePos.y + 10f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
