using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class ObstacleTutorialUI : TutorialBase
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
    [SerializeField] private int obstacleCountGoal = 3;
    [SerializeField] private Vector2 obstacleSpawnPosition = new Vector2(8f, 0f);
    [SerializeField] private int obstacleTypeA = 0;
    [SerializeField] private int obstacleTypeB = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip showTextSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip directionJumpSound;
    [SerializeField] private AudioClip obstacleJumpSound;

    [Header("Obstacle Timing")]
    [SerializeField] private float obstacleSpawnInterval = 2.0f;
    [SerializeField] private float obstacleIntroDelay = 0.5f;
    [SerializeField] private float obstacleInstructionDuration = 2.0f;

    [Header("Jumping Section Timing")]
    [SerializeField] private float jumpStartDelay = 1.0f;
    [SerializeField] private float jumpCompleteDelay = 1.0f;
    
    [Header("CompletionTiming")]
    [SerializeField] private float completionDelay = 1.0f;



    private AudioSource _audioSource;
    private int _obstaclesCleared = 0;
    private bool _spawningObstacles = false;
    private bool _tookDamageThisRound = false;
    [SerializeField] private int _starsCollected = 0;


    void OnEnable()
    {
        TutorialStar.OnStarCollected += HandleStarCollected;
    }

    void OnDisable()
    {
        TutorialStar.OnStarCollected -= HandleStarCollected;
    }

    void HandleStarCollected()
    {
        _starsCollected++;
    }

    protected override void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        base.Awake();
    }

    public override void StartTutorial()
    {
        if(_tutorialActive)
            return;

        SetArrowsAlpha(0f);
        StartCoroutine(RunObstacleTutorial());
        StartIdleWobble();

        base.StartTutorial();
    }

    protected override void CompleteTutorial()
    {
        ObstacleManager.Instance.UnregisterObstacle(this.gameObject);
        tutorialText.DOFade(0f, 0.5f);
        KillIdleWobble();
        base.CompleteTutorial();
    }


    private IEnumerator RunObstacleTutorial()
    {
        string keyName = InputBindingManager.Instance.GetBoundKey(InputActionType.Jump).ToString();

        bool typewriterComplete = false;

        typewriter.StartTyping($"Hold [{keyName}] to jump", () =>
        {
            typewriterComplete = true;
        });

        yield return new WaitUntil(() => typewriterComplete == true);

        foreach(Vector3 spawnPos in starLocations)
        {
            Instantiate(collectStar, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(jumpStartDelay);

        Player.Instance.SetPlayerControlState(controlMode);
        ScreenDimmerManager.Instance.AddDimSource("tutorial");
        ObstacleManager.Instance.RegisterObstacle(gameObject);

        while(_starsCollected < starLocations.Count)
        {
            yield return null;
        }

        yield return StartCoroutine(OnAllDirectionsJumped());
    }

    private IEnumerator OnAllDirectionsJumped()
    {
        yield return new WaitForSeconds(jumpCompleteDelay);

        ScreenDimmerManager.Instance.RemoveDimSource("tutorial");

        yield return new WaitForSeconds(obstacleIntroDelay);

        typewriter.StartTyping("Jump over obstacles");

        if (completeSound)
            _audioSource.PlayOneShot(completeSound);

        yield return new WaitForSeconds(obstacleInstructionDuration);

        StartCoroutine(SpawnObstaclePhase());
    }


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
                typewriter.StartTyping($"Clear [<color=#FFD84C>{remaining}</color>]");
                
            // Play sound
            if (obstacleJumpSound && updateText)
            {
                float pitch = 1f + (_obstaclesCleared * 0.1f);
                _audioSource.pitch = pitch;
                _audioSource.PlayOneShot(obstacleJumpSound);
            }

            // Wait for the obstacle’s active time
            yield return new WaitForSeconds(obstacleSpawnInterval);

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

        yield return new WaitForSeconds(completionDelay);

        bool typingComplete = false;

        typewriter.StartTyping("Great!", ()=>
        {
            typingComplete = true;
        });

        _audioSource.pitch = 1f;
        _audioSource.PlayOneShot(completeSound);

        yield return new WaitUntil(() => typingComplete == true);
    
        CompleteTutorial();
    }


    private void HandleDamageTaken(int newHealth)
    {
        if (_spawningObstacles)
            _tookDamageThisRound = true;
    }

    private void SetArrowsAlpha(float a)
    {
        SetArrowAlpha(upArrow, a);
        SetArrowAlpha(downArrow, a);
        SetArrowAlpha(leftArrow, a);
        SetArrowAlpha(rightArrow, a);
    }

    private void SetArrowAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
