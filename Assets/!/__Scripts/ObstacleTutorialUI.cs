using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class ObstacleTutorialUI : MonoBehaviour
{
    public List<ObstacleTypeDefinition> obstacleTypes;
    [Header("UI References")]
    public TextTypewriter typewriter;
    public TextMeshProUGUI tutorialText;

    [Header("Direction Indicators")]
    public Image upArrow;
    public Image downArrow;
    public Image leftArrow;
    public Image rightArrow;

    [Header("Settings")]
    public float textDisplayTime = 1.5f;
    public float fadeDuration = 0.4f;
    public float obstacleSpawnInterval = 2.0f;
    public int obstacleCountGoal = 3;
    public Vector2 obstacleSpawnPosition = new Vector2(8f, 0f);
    public int obstacleTypeA = 0;
    public int obstacleTypeB = 1;

    [Header("Audio")]
    public AudioClip showTextSound;
    public AudioClip completeSound;
    public AudioClip directionJumpSound;
    public AudioClip obstacleJumpSound;

    private AudioSource audioSource;
    private Tween idleWobbleTween;
    private Vector3 basePos;
    private bool isDipping = false;
    private readonly HashSet<Vector2> completedJumps = new();
    private bool tutorialComplete = false;

    private int obstaclesCleared = 0;
    private bool spawningObstacles = false;
    private bool tookDamageThisRound = false;
    private bool checkForJumpInput = false;
    private bool completed = false;

    public bool TutorialComplete => completed;

    void OnEnable()
    {
        Player.OnJumped += HandlePlayerJump;
    }

    void OnDisable()
    {
        Player.OnJumped -= HandlePlayerJump;
    }

    void HandlePlayerJump(Vector2 dir)
    {
        if (!checkForJumpInput)
            return;
                
        TriggerJumpDip();

        // --- Phase 1: Learn jump directions ---
        if (!tutorialComplete)
        {
            Vector2 snapped = SnapToCardinal(dir);

            if (!completedJumps.Contains(snapped))
            {
                completedJumps.Add(snapped);
                FadeOutArrow(snapped);
                PlayDirectionSound(completedJumps.Count);
            }

            if (completedJumps.Count >= 4 && !tutorialComplete)
            {
                tutorialComplete = true;
                StartCoroutine(OnAllDirectionsJumped());
            }
        }
    }

    void Start()
    {
        completed = false;
        audioSource = gameObject.AddComponent<AudioSource>();
        basePos = tutorialText.rectTransform.anchoredPosition;

        SetArrowsAlpha(0f);
        StartCoroutine(RunObstacleTutorial());
        StartIdleWobble();
    }

    private IEnumerator RunObstacleTutorial()
    {
        string keyName = InputBindingManager.Instance.GetKey(InputActionType.Jump).ToString();
        typewriter.StartTyping($"Jump [{keyName}]");

        yield return new WaitForSeconds(textDisplayTime);

        FadeInArrows();

        yield return new WaitForSeconds(1.0f);

        ScreenDimmerManager.Instance.AddDimSource("tutorial");
        ObstacleManager.Instance.RegisterObstacle(this.gameObject);

        checkForJumpInput = true;
    }

    private IEnumerator OnAllDirectionsJumped()
    {
        checkForJumpInput = false;
        yield return new WaitForSeconds(1.0f);

        ScreenDimmerManager.Instance.RemoveDimSource("tutorial");
        yield return new WaitForSeconds(0.5f);

        typewriter.StartTyping("Jump over obstacles");
        if (completeSound) audioSource.PlayOneShot(completeSound);

        yield return new WaitForSeconds(2.0f);

        StartCoroutine(SpawnObstaclePhase());
    }

    // --------------------------------------------------
    // 🚧 Obstacle Jump Phase
    // --------------------------------------------------
    private IEnumerator SpawnObstaclePhase()
    {
        spawningObstacles = true;
        obstaclesCleared = 0;
        tookDamageThisRound = false;

        Player.OnDamageTaken += HandleDamageTaken;

        bool updateText = true;

        while (obstaclesCleared < obstacleCountGoal)
        {
            int typeIndex = (obstaclesCleared % 2 == 0) ? obstacleTypeA : obstacleTypeB;
            tookDamageThisRound = false;

            // Spawn obstacle
            ArrowSpawner.Instance.SpawnObstacle(obstacleSpawnPosition, obstacleTypes[typeIndex].fileName, 0);

            // Update text at start of each round
            int remaining = Mathf.Max(0, obstacleCountGoal - obstaclesCleared);
            
            if(updateText)
            {
                typewriter.StartTyping($"Clear [<color=#FFD84C>{remaining}</color>]");
            }
                

            // Play sound
            if (obstacleJumpSound && updateText)
            {
                float pitch = 1f + (obstaclesCleared * 0.1f);
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(obstacleJumpSound);
            }

            // Wait for the obstacle’s active time
            yield return new WaitForSeconds(obstacleSpawnInterval);

            // ✅ Count cleared only if no damage taken
            if (!tookDamageThisRound)
            {
                updateText = true;
                obstaclesCleared++;
            }
            else
            {
                updateText = false;
            }
                
        }

        Player.OnDamageTaken -= HandleDamageTaken;
        spawningObstacles = false;

        yield return new WaitForSeconds(1f);

        typewriter.StartTyping("Great!");
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(completeSound);

        yield return new WaitForSeconds(textDisplayTime);
        ObstacleManager.Instance.UnregisterObstacle(this.gameObject);
        tutorialText.DOFade(0f, 0.5f);

        completed = true;
    }

    private void HandleDamageTaken(int newHealth)
    {
        if (spawningObstacles)
            tookDamageThisRound = true;
    }

    // --------------------------------------------------
    // 🌀 Wobble & Dip Animation
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

    // --------------------------------------------------
    // 🔹 Arrow Handling
    // --------------------------------------------------
    private void SetArrowsAlpha(float a)
    {
        SetArrowAlpha(upArrow, a);
        SetArrowAlpha(downArrow, a);
        SetArrowAlpha(leftArrow, a);
        SetArrowAlpha(rightArrow, a);
    }

    private void FadeInArrows()
    {
        upArrow?.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);
        downArrow?.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);
        leftArrow?.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);
        rightArrow?.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);
    }

    private void FadeOutArrow(Vector2 dir)
    {
        Image arrow = null;
        if (dir == Vector2.up) arrow = upArrow;
        else if (dir == Vector2.down) arrow = downArrow;
        else if (dir == Vector2.left) arrow = leftArrow;
        else if (dir == Vector2.right) arrow = rightArrow;

        if (arrow != null)
            arrow.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine);
    }

    private void SetArrowAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    // --------------------------------------------------
    // 🔊 Direction Jump Sound
    // --------------------------------------------------
    private void PlayDirectionSound(int count)
    {
        if (directionJumpSound == null) return;
        float pitch = Mathf.Lerp(1f, 1.3f, (count - 1) / 3f);
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(directionJumpSound);
    }

    // --------------------------------------------------
    // 🔹 Direction Helper
    // --------------------------------------------------
    private Vector2 SnapToCardinal(Vector2 dir)
    {
        if (dir == Vector2.zero) return Vector2.zero;

        Vector2[] cardinals = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        float bestDot = -Mathf.Infinity;
        Vector2 best = Vector2.zero;

        foreach (Vector2 c in cardinals)
        {
            float dot = Vector2.Dot(dir.normalized, c);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = c;
            }
        }

        return best;
    }
}
