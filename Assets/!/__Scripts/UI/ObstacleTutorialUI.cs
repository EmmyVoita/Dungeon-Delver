using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class ObstacleTutorialUI : MonoBehaviour
{
    [SerializeField] private PlayerControlState controlMode = PlayerControlState.BasicJump;
    [SerializeField] private List<ObstacleTypeDefinition> obstacleTypes;
    [SerializeField] private GameObject collectStar;
    [SerializeField] private List<Vector3> starLocations;

    [Header("UI References")]
    [SerializeField] private TextTypewriter typewriter;
    [SerializeField] private RectTransform dipTransform;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Direction Indicators")]
    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;
    [SerializeField] private Image leftArrow;
    [SerializeField] private Image rightArrow;

    [Header("Settings")]
    [SerializeField] private float textDisplayTime = 1.5f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float obstacleSpawnInterval = 2.0f;
    [SerializeField] private int obstacleCountGoal = 3;
    [SerializeField] private Vector2 obstacleSpawnPosition = new Vector2(8f, 0f);
    [SerializeField] private int obstacleTypeA = 0;
    [SerializeField] private int obstacleTypeB = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip showTextSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip directionJumpSound;
    [SerializeField] private AudioClip obstacleJumpSound;

    private AudioSource _audioSource;
    private Tween _idleWobbleTween;
    private Vector3 _basePos;
    private readonly HashSet<Vector2> _completedJumps = new();
    private bool _tutorialComplete = false;

    private int _obstaclesCleared = 0;
    private bool _spawningObstacles = false;
    private bool _tookDamageThisRound = false;
    private bool _checkForJumpInput = false;
    private bool _completed = false;
    [SerializeField] private int _starsCollected = 0;

    public bool TutorialComplete => _completed;

    void OnEnable()
    {
        Player.OnJumped += HandlePlayerJump;
        TutorialStar.OnStarCollected += HandleStarCollected;
    }

    void OnDisable()
    {
        Player.OnJumped -= HandlePlayerJump;
        TutorialStar.OnStarCollected -= HandleStarCollected;
    }

    void HandleStarCollected()
    {
        _starsCollected++;
    }

    void HandlePlayerJump(Vector2 dir)
    {
        return;

        if (!_checkForJumpInput)
            return;
                
        dipTransform.PlayJumpDip(baseY: _basePos.y,
                                        dipAmount: 15f,
                                        dipDuration: 0.15f,
                                        returnDuration: 0.25f);

        // --- Phase 1: Learn jump directions ---
        if (!_tutorialComplete)
        {
            Vector2 snapped = SnapToCardinal(dir);

            if (!_completedJumps.Contains(snapped))
            {
                _completedJumps.Add(snapped);
                FadeOutArrow(snapped);
                PlayDirectionSound(_completedJumps.Count);
            }

            if (_completedJumps.Count >= 4 && !_tutorialComplete)
            {
                _tutorialComplete = true;
                StartCoroutine(OnAllDirectionsJumped());
            }
        }
    }

    void Start()
    {
        _completed = false;
        _audioSource = gameObject.AddComponent<AudioSource>();
        _basePos = tutorialText.rectTransform.anchoredPosition;

        SetArrowsAlpha(0f);
        StartCoroutine(RunObstacleTutorial());
        StartIdleWobble();
    }

    private IEnumerator RunObstacleTutorial()
    {
        

        string keyName = InputBindingManager.Instance.GetKeyName(InputActionType.Jump).ToString();
        typewriter.StartTyping($"Hold [{keyName}] to jump");

        yield return new WaitForSeconds(textDisplayTime);

        foreach(Vector3 spawnPos in starLocations)
        {
            GameObject obj = Instantiate(collectStar, spawnPos, Quaternion.identity);
        }

     

        //FadeInArrows();

        yield return new WaitForSeconds(1.0f);

        Player.Instance.SetPlayerControlState(controlMode);
        ScreenDimmerManager.Instance.AddDimSource("tutorial");
        ObstacleManager.Instance.RegisterObstacle(this.gameObject);

        while(_starsCollected < starLocations.Count)
        {
            yield return null;
        }

        yield return StartCoroutine(OnAllDirectionsJumped());

        //_checkForJumpInput = true;
    }

    private IEnumerator OnAllDirectionsJumped()
    {
        _checkForJumpInput = false;
        yield return new WaitForSeconds(1.0f);

        ScreenDimmerManager.Instance.RemoveDimSource("tutorial");
        yield return new WaitForSeconds(0.5f);

        typewriter.StartTyping("Jump over obstacles");
        if (completeSound) _audioSource.PlayOneShot(completeSound);

        yield return new WaitForSeconds(2.0f);

        StartCoroutine(SpawnObstaclePhase());
    }

    // --------------------------------------------------
    // 🚧 Obstacle Jump Phase
    // --------------------------------------------------
    private IEnumerator SpawnObstaclePhase()
    {
        _spawningObstacles = true;
        _obstaclesCleared = 0;
        _tookDamageThisRound = false;

        Player.OnDamageTaken += HandleDamageTaken;

        bool updateText = true;

        while (_obstaclesCleared < obstacleCountGoal)
        {
            int typeIndex = (_obstaclesCleared % 2 == 0) ? obstacleTypeA : obstacleTypeB;
            _tookDamageThisRound = false;

            // Spawn obstacle
            ArrowSpawner.Instance.SpawnObstacle(obstacleSpawnPosition, obstacleTypes[typeIndex].fileName, 0);

            // Update text at start of each round
            int remaining = Mathf.Max(0, obstacleCountGoal - _obstaclesCleared);
            
            if(updateText)
            {
                typewriter.StartTyping($"Clear [<color=#FFD84C>{remaining}</color>]");
            }
                

            // Play sound
            if (obstacleJumpSound && updateText)
            {
                float pitch = 1f + (_obstaclesCleared * 0.1f);
                _audioSource.pitch = pitch;
                _audioSource.PlayOneShot(obstacleJumpSound);
            }

            // Wait for the obstacle’s active time
            yield return new WaitForSeconds(obstacleSpawnInterval);

            // ✅ Count cleared only if no damage taken
            if (!_tookDamageThisRound)
            {
                updateText = true;
                _obstaclesCleared++;
            }
            else
            {
                updateText = false;
            }
                
        }

        Player.OnDamageTaken -= HandleDamageTaken;
        _spawningObstacles = false;

        yield return new WaitForSeconds(1f);

        typewriter.StartTyping("Great!");
        _audioSource.pitch = 1f;
        _audioSource.PlayOneShot(completeSound);

        yield return new WaitForSeconds(textDisplayTime);
        ObstacleManager.Instance.UnregisterObstacle(this.gameObject);
        tutorialText.DOFade(0f, 0.5f);

        _completed = true;
    }

    private void HandleDamageTaken(int newHealth)
    {
        if (_spawningObstacles)
            _tookDamageThisRound = true;
    }

    // --------------------------------------------------
    // 🌀 Wobble & Dip Animation
    // --------------------------------------------------
    private void StartIdleWobble()
    {
        _idleWobbleTween?.Kill();
        _idleWobbleTween = tutorialText.rectTransform
            .DOAnchorPosY(_basePos.y + 10f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
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
        _audioSource.pitch = pitch;
        _audioSource.PlayOneShot(directionJumpSound);
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
