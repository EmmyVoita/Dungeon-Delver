using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class AbilityTutorialUI : MonoBehaviour
{
    [Header("UI References")]
    public TextTypewriter typewriter;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI hintText;

    public ArrowTypeDefinition normalArrowType;


    public float demoArrowSpeed = 5f;
    public float arrowSpeed = 5f;
    public float criticalCatchDemoDelay = 1.8f;
    public float greyFadeDuration = 0.4f;
    public Color inactiveColor = new Color(1f, 1f, 1f, 1.0f);   

    [Header("Settings")]
    public float textDisplayTime = 1.5f;
    public float fadeDuration = 0.4f;
    public float arrowSpawnInterval = 1.2f;
    public int arrowTypeIndex = 0; // index for your normal/crit arrow prefab
    public Vector2 arrowSpawnPosition = new Vector2(8f, 0f);

    [Header("Audio")]
    public AudioClip showTextSound;
    public AudioClip completeSound;
    public AudioClip spawnSound;

    private AudioSource audioSource;
    private Tween idleWobbleTween;
    private Vector3 basePos;
    private bool isDipping = false;

    private bool tutorialActive = false;
    private bool tutorialComplete = false;
    private int arrowsSpawned = 0;
    private int arrowsCaught = 0;
    private bool completed = false;
    private bool lockChargeChanged = false;
    public bool TutorialComplete => completed;
    private Coroutine criticalCatchDemoCoroutine;
    private bool allowAbilityTextUpdates = false;


    private Vector2[] spawnDirections = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    void Start()
    {
        completed = false;
        audioSource = gameObject.AddComponent<AudioSource>();
        basePos = tutorialText.rectTransform.anchoredPosition;
        tutorialText.alpha = 1f;
        Player.Instance.MaxAbilityCharge = 10;

        string keyName = InputBindingManager.Instance.GetKey(InputActionType.Confirm).ToString();
        string hint = $"\n<size=70%><color=#aaaaaa>Press [{keyName}] to continue</color></size>";
        hintText.text = hint;
        hintText.alpha = 0f;

        StartCoroutine(RunCriticalCatchTutorial());
        StartIdleWobble();
    }

    void OnEnable()
    {
        Player.OnAbilityChargeChanged += HandleAbilityChargeChanged;
    }

    void OnDisable()
    {
        Player.OnAbilityChargeChanged -= HandleAbilityChargeChanged;
    }

    private IEnumerator RunCriticalCatchTutorial()
    {
        // 🟡 Demonstration loops
        Player.Instance.lockInput = true;
        lockChargeChanged = true;
        yield return StartCoroutine(ShowCatchDemonstration());
        
        lockChargeChanged = false;
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("Charge your ability"));
        
        // Start spawning arrows
        StartCoroutine(SpawnArrowsUntilFilled());
    }

    private IEnumerator ShowCatchDemonstration()
    {
        SpriteRenderer playerSR = Player.Instance.spriteObj.GetComponent<SpriteRenderer>();
        SpriteRenderer goalSR = Player.Instance.goal.GetComponentInChildren<Goal>().sRend;

        // --- NORMAL CATCH DEMO ---

        Player.Instance.SetFullyLocked(true);

        // 🩶 Fade player to gray
        playerSR.DOColor(inactiveColor, greyFadeDuration);
        goalSR.DOColor(inactiveColor, greyFadeDuration);
        

        bool continuePressed = false;
        float arrowTimer = 0f;
        float spawnInterval = 2.7f;
        float minWaitTime = 5f;
        float elapsed = 0f;

 
    
        typewriter.StartTyping(
            "A normal catch is holding the goal in the right direction."
        );

        
        bool hintShown = false;

        while (!continuePressed)
        {
            elapsed += Time.deltaTime;
            arrowTimer += Time.deltaTime;

            if (arrowTimer >= spawnInterval)
            {
                arrowTimer = 0f;
                ArrowSpawner.Instance.SpawnArrow(Vector2.up, demoArrowSpeed, normalArrowType.displayName, damageOverride: 0);
                Player.Instance.goal.GetComponentInChildren<Goal>().SetGoalDirection(Vector2.up);
            }

            if (!hintShown && elapsed >= minWaitTime)
            {
                hintShown = true;

                // later, when you want it to appear
                hintText.DOColor(Color.white, 0.5f);
                //tutorialText.alpha = 0f;
                //tutorialText.DOFade(1f, 0.5f);
            }

            if (elapsed >= minWaitTime && InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
                continuePressed = true;

            yield return null;
        }

        hintText.DOColor(Color.clear, 0.5f);

        // 🧹 Transition out
        tutorialText.DOFade(0f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        tutorialText.DOFade(1f, 0.3f);
        ArrowSpawner.Instance.ClearAllArrows();

        // --- CRITICAL CATCH DEMO ---
          typewriter.StartTyping(
            "A critical catch is moving into the arrow just before it hits."
        );

        yield return new WaitForSeconds(0.8f);

        continuePressed = false;
        arrowTimer = 0f;
        elapsed = 0f;
        hintShown = false;

        while (!continuePressed)
        {
            elapsed += Time.deltaTime;
            arrowTimer += Time.deltaTime;

            if (arrowTimer >= spawnInterval)
            {
                arrowTimer = 0f;
                ArrowSpawner.Instance.SpawnArrow(Vector2.up, demoArrowSpeed, normalArrowType.displayName, damageOverride: 0);

                if (criticalCatchDemoCoroutine != null)
                    StopCoroutine(criticalCatchDemoCoroutine);
                criticalCatchDemoCoroutine = StartCoroutine(SimulateCriticalCatch());
            }

            if (!hintShown && elapsed >= minWaitTime)
            {
                hintShown = true;
                hintText.DOColor(Color.white, 0.5f);
            }

            if (elapsed >= minWaitTime && InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
                continuePressed = true;

            yield return null;
        }

        hintText.DOColor(Color.clear, 0.5f);
        // 🧹 Stop demo and fade back
        tutorialText.DOFade(0f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        tutorialText.DOFade(1f, 0.3f);
        ArrowSpawner.Instance.ClearAllArrows();
        

        // 🤍 Fade back to white
        playerSR.DOColor(Color.white, greyFadeDuration);
        goalSR.DOColor(Color.white, greyFadeDuration);

        // --- ABILITY BAR EXPLANATION ---
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("Critical catches charge your ability."));

        Player.Instance.AbilityCharge = 0;
        Player.Instance.SetFullyLocked(false);
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
        string keyName = InputBindingManager.Instance.GetKey(InputActionType.Confirm).ToString();
        string hint = $"\n<size=70%><color=#aaaaaa>Press [{keyName}] to continue</color></size>";

        bool done = false;

        typewriter.StartTyping(text, () => done = true);

        if (showTextSound)
            audioSource.PlayOneShot(showTextSound);

        // Wait for typing to finish
        yield return new WaitUntil(() => done);

        // Reveal suffix AFTER typing
        hintText.DOColor(Color.white, 0.5f);

        // Wait for confirm
        yield return new WaitUntil(() =>
            InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm)
        );

        hintText.DOColor(Color.clear, 0.5f);

        // Fade polish
        tutorialText.DOFade(0f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        tutorialText.DOFade(1f, 0.3f);
    }




    private IEnumerator SpawnArrowsUntilFilled()
    {
        tutorialActive = true;

        // start a lightweight text updater
        allowAbilityTextUpdates = true;
        StartCoroutine(UpdateAbilityTextContinuously());


        while (!tutorialComplete)
        {
            int dir = Random.Range(0, 4);
            Vector2 direction = spawnDirections[dir];

            ArrowSpawner.Instance.SpawnArrow(direction, arrowSpeed, normalArrowType.displayName, damageOverride: 0);

            if (spawnSound)
                audioSource.PlayOneShot(spawnSound);

            yield return new WaitForSeconds(arrowSpawnInterval);
        }
    }

    // ----------------------------------------------
    // 🟢 Separate coroutine that updates % every frame
    // ----------------------------------------------
    private IEnumerator UpdateAbilityTextContinuously()
    {
        while (!tutorialComplete)
        {
            if (!allowAbilityTextUpdates)
            {
                yield return null;
                continue;
            }

            float chargePercent = (float)Player.Instance.AbilityCharge / Player.Instance.MaxAbilityCharge;
            int filledPercent = Mathf.RoundToInt(chargePercent * 100f);

            typewriter.SetInstant(
                $"Fill ability bar [<color=#FFD84C>{filledPercent}%</color>]"
            );

            yield return null;
        }
    }



    private void HandleAbilityChargeChanged(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        // Check if bar is full
        if (Player.Instance.AbilityCharge >= Player.Instance.MaxAbilityCharge && !tutorialComplete && !lockChargeChanged)
        {
            tutorialComplete = true;
            float chargePercent = (float)Player.Instance.AbilityCharge / Player.Instance.MaxAbilityCharge;
            int filledPercent = Mathf.RoundToInt(chargePercent * 100f);
            typewriter.SetInstant($"Fill ability bar [<color=#FFD84C>{filledPercent}%</color>]"); // force update
            StartCoroutine(OnTutorialComplete());
        }
        
        TriggerJumpDip();
    }

    private IEnumerator OnTutorialComplete()
    {
        allowAbilityTextUpdates = false;

        tutorialActive = false;

        //Destroy remainng arrows
        ArrowSpawner.Instance.ClearAllArrows();

        //ScreenDimmerManager.Instance.RemoveDimSource("tutorial");
        //ObstacleManager.Instance.UnregisterObstacle(this.gameObject);

        yield return new WaitForSeconds(0.5f);

        // Tell player they filled their ability bar
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("Great!"));

        string keyName = InputBindingManager.Instance.GetKey(InputActionType.UseAbility).ToString();
        typewriter.StartTyping($"Press [{keyName}] to use your ability.", () =>
        {
            Player.Instance.lockInput = false;
            // completed typing
        });

        // 🟣 Wait for the player to use their ability
        bool used = false;
        System.Action handler = () => used = true;
        Player.OnAbilityUsed += handler;

        yield return new WaitUntil(() => used);
        Player.OnAbilityUsed -= handler;

        yield return StartCoroutine(PlayTypewriterLineWaitForInput("This ability slows time briefly when used"));
        yield return StartCoroutine(PlayTypewriterLineWaitForInput("You can unlock other abilities as you progress"));

        typewriter.textComponent.text = "";

        completed = true;
    }


    // --------------------------------------------------
    // 🌀 Wobble & Dip Animation (same as obstacle tutorial)
    // --------------------------------------------------
    private void StartIdleWobble()
    {
        idleWobbleTween?.Kill();
        idleWobbleTween = tutorialText.rectTransform
            .DOAnchorPosY(basePos.y + 10f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void TriggerJumpDip()
    {
        if (isDipping) return;
        isDipping = true;

        idleWobbleTween?.Kill();

        Sequence dipSeq = DOTween.Sequence();
        dipSeq.Append(tutorialText.rectTransform.DOAnchorPosY(basePos.y - 15f, 0.15f).SetEase(Ease.OutQuad));
        dipSeq.Append(tutorialText.rectTransform.DOAnchorPosY(basePos.y, 0.25f).SetEase(Ease.OutBack));
        dipSeq.OnComplete(() =>
        {
            StartIdleWobble();
            isDipping = false;
        });
    }
}
