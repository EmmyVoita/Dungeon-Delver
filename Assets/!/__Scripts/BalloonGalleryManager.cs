using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BalloonGalleryManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip counterSound;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip incorrectSound;
    public AudioClip dartThrowSound;

    [Header("General Settings")]
    public GameObject instructionCanvasPrefab;
    public GameObject timerRingPrefab;
    public float timerVerticalOffset = -200f;
    public float roundTimeLimit = 8f;
    public string displayMessage = "POP SAME COLOR";
    public float messageDuration = 2.0f;

    [Header("Balloon Settings")]
    public GameObject balloonPrefab;
    public float spawnInterval = 0.7f;
    public float row1MoveSpeed = 3f;
    public float row2MoveSpeed = 3.5f;

    [Header("Row Layout")]
    public Vector2 row1SpawnPos = new Vector2(-8f, 3.5f);
    public Vector2 row2SpawnPos = new Vector2(-8f, 2.5f);

    public float despawnDistance = 10f;
    public Vector2 row1MoveDirection = Vector2.right;
    public Vector2 row2MoveDirection = Vector2.left;

    [Header("Colors")]
    public BalloonColor[] allowedColors;
    public int requiredStreak = 3;

    [Header("Dart")]
    [SerializeField] private Transform aimingDartPrefab;      // visual only
    [SerializeField] private GameObject projectileDartPrefab; // real projectile
    [SerializeField] private Vector3 dartVisualOffset = new Vector3(0, 0.4f, 0);
    [SerializeField] private Vector3 dartFireOffset = new Vector3(0, 0.2f, 0);
    [SerializeField] private float dartSpeed = 12f;

    [Header("Fire Animation")]
    [SerializeField] private float pullBackDistance = 0.15f;
    [SerializeField] private float pullBackDuration = 0.08f;
    [SerializeField] private float fireDelay = 0.02f;
    [SerializeField] private float dartRespawnDelay = 0.5f;

    [Header("Spawn Fairness")]
    [Tooltip("Every X balloons, force one to be correct color (0 = off)")]
    [SerializeField] private int forceCorrectEvery = 5;

    // ──────────────────────────────

    private Transform aimingDart;
    private Vector3 aimingDartBaseScale;
    private Vector3 projectileDartBaseScale;

    private BalloonColor? currentTargetColor = null;
    private int streakCount = 0;
    private int balloonsSpawnedSinceCorrect = 0;

    private bool obstacleActive = false;
    private bool canFire = true;

    private List<BalloonObject> activeBalloons = new();
    private GameObject timerRingInstance;
    private Sequence spawnDartSequence;
    private bool isEnding = false;
    public bool IsEnding => isEnding;

    // ──────────────────────────────
    // LIFECYCLE
    // ──────────────────────────────

    void OnEnable()
    {
        Player.OnJumpInput += HandleFireDart;
    }

    void OnDisable()
    {
        Player.OnJumpInput -= HandleFireDart;
    }

    void Start()
    {
        isEnding = false;
        Player.Instance.UseEightDirections = true;
        Player.Instance.SetPlayerControlState(Player.PlayerControlState.LockedShooter);

        ObstacleManager.Instance.RegisterObstacle(gameObject);
        Player.Instance.ResetPositionAndVelocity();
        obstacleActive = true;

        aimingDartBaseScale = aimingDartPrefab.localScale;
        projectileDartBaseScale = projectileDartPrefab.transform.localScale;

        SpawnAimingDart();
  

        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        if (!obstacleActive || aimingDart == null)
            return;

        UpdateAimingDartRotation();
    }

    // ──────────────────────────────
    // AIMING (GOAL-DRIVEN)
    // ──────────────────────────────

    private void SpawnAimingDart()
    {
        canFire = true;

        if (aimingDart != null)
            Destroy(aimingDart.gameObject);

        aimingDart = Instantiate(aimingDartPrefab, Player.Instance.transform);
        aimingDart.localPosition = dartVisualOffset;
        aimingDart.localScale = aimingDartBaseScale;

        UpdateAimingDartRotation();
    }

    private void UpdateAimingDartRotation()
    {
        Transform goal = Player.Instance.goal.transform;
        aimingDart.rotation = goal.rotation;
    }

    // ──────────────────────────────
    // FIRING
    // ──────────────────────────────

    private void HandleFireDart()
    {
        if (!canFire || !obstacleActive || aimingDart == null)
            return;

        canFire = false;
        aimingDart.DOKill();

        Vector3 originalPos = aimingDart.localPosition;
        Vector3 pullDir = -aimingDart.up;

        spawnDartSequence = DOTween.Sequence();

        spawnDartSequence.Append(
            aimingDart.DOLocalMove(
                originalPos + pullDir * pullBackDistance,
                pullBackDuration
            ).SetEase(Ease.OutQuad)
        );

        spawnDartSequence.AppendCallback(FireProjectile);
        spawnDartSequence.AppendInterval(fireDelay);

        spawnDartSequence.AppendCallback(() =>
        {
            Destroy(aimingDart.gameObject);
        });

        spawnDartSequence.AppendInterval(dartRespawnDelay);
        spawnDartSequence.AppendCallback(SpawnAimingDart);
    }

    private void FireProjectile()
    {
        AudioHelpers.PlayMyClipAtPoint(
            dartThrowSound,
            AudioChannel.SFX,
            Camera.main.transform.position
        );

        Transform goal = Player.Instance.goal.transform;
        Vector2 dir = goal.up.normalized;

        GameObject dart = Instantiate(
            projectileDartPrefab,
            goal.position + goal.up * dartFireOffset.y,
            goal.rotation
        );

        dart.transform.localScale = projectileDartBaseScale;

        if (dart.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = dir * dartSpeed;
    }

    // ──────────────────────────────
    // BALLOON SPAWNING
    // ──────────────────────────────

    IEnumerator SpawnRoutine()
    {
        isEnding = false;
        StartCoroutine(ShowInstructionMessage(displayMessage));

        yield return new WaitForSeconds(messageDuration);

        if (timerRingPrefab != null)
        {
            timerRingInstance = Instantiate(timerRingPrefab, transform.position, Quaternion.identity);
            timerRingInstance.GetComponent<BasicFillBar>().Show(
                roundTimeLimit,
                () => { if (streakCount < requiredStreak) RanOutTime(); },
                new Vector2(0, timerVerticalOffset)
            );
        }

        while (obstacleActive)
        {
            SpawnBalloonAt(row1MoveSpeed, row1SpawnPos, row1MoveDirection);
            yield return new WaitForSeconds(spawnInterval);

            SpawnBalloonAt(row2MoveSpeed, row2SpawnPos, row2MoveDirection);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBalloonAt(float speed, Vector2 spawnPos, Vector2 moveDir)
    {
        GameObject obj = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);
        BalloonObject balloon = obj.GetComponentInChildren<BalloonObject>();

        BalloonColor color;
        bool forceCorrect =
            forceCorrectEvery > 0 &&
            currentTargetColor != null &&
            balloonsSpawnedSinceCorrect >= forceCorrectEvery - 1;

        if (forceCorrect)
        {
            color = currentTargetColor.Value;
            balloonsSpawnedSinceCorrect = 0;
        }
        else
        {
            color = allowedColors[Random.Range(0, allowedColors.Length)];
            balloonsSpawnedSinceCorrect++;
        }

        balloon.Init(this, color, speed, moveDir);

        if (currentTargetColor != null)
        {
            if (color == currentTargetColor) balloon.EnableGlow();
            else balloon.DisableGlow();
        }

        activeBalloons.Add(balloon);

        Vector2 finalPos = spawnPos + moveDir.normalized * despawnDistance;
        balloon.moveTween = balloon.transform.DOMove(finalPos, speed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                activeBalloons.Remove(balloon);
                Destroy(balloon.gameObject);
            });
    }

    // ──────────────────────────────
    // HIT LOGIC
    // ──────────────────────────────

    public void OnBalloonHit(BalloonObject balloon)
    {
        BalloonColor color = balloon.balloonColor;

        if (currentTargetColor == null)
        {
            currentTargetColor = color;
            streakCount = 1;
            balloonsSpawnedSinceCorrect = 0;
            PlayCounterSound();
            HighlightMatchingColor(color);
        }
        else if (color == currentTargetColor)
        {
            streakCount++;
            PlayCounterSound();

            if (streakCount >= requiredStreak)
                OnSuccess();
        }
        else
        {
            OnFail();
        }
    }

    private void HighlightMatchingColor(BalloonColor color)
    {
        foreach (var b in activeBalloons)
            if (b != null)
                if (b.balloonColor == color) b.EnableGlow();
                else b.DisableGlow();
    }

    private void PlayCounterSound()
    {
        float pitch = 1f + streakCount * 0.1f;
        AudioHelpers.PlayMyClipAtPoint(counterSound, AudioChannel.SFX, Camera.main.transform.position, pitch: pitch);
    }

    // ──────────────────────────────
    // SUCCESS / FAIL
    // ──────────────────────────────

    void OnSuccess()
    {
        if(isEnding) return;
        isEnding = true;
        obstacleActive = false;
        AudioHelpers.PlayMyClipAtPoint(successSound, AudioChannel.SFX, Camera.main.transform.position);
        timerRingInstance?.GetComponent<BasicFillBar>().HideImmediate();
        StartCoroutine(DestroySequence());
    }

    void OnFail()
    {
        AudioHelpers.PlayMyClipAtPoint(incorrectSound, AudioChannel.SFX, Camera.main.transform.position);
    }

    void RanOutTime()
    {
        if (isEnding) return;
        isEnding = true;

        AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);
        Player.Instance.DamageSelf(1);
        obstacleActive = false;
        StartCoroutine(DestroySequence());
    }

    IEnumerator DestroySequence()
    {
        if (spawnDartSequence != null && spawnDartSequence.IsActive())
            spawnDartSequence.Kill();

        if(aimingDart != null)
        Destroy(aimingDart.gameObject);

        foreach (var b in activeBalloons)
            if (b != null)
                b.SlowToStop(0.75f);

        yield return new WaitForSeconds(0.8f);

        foreach (var b in activeBalloons)
        {
            if (b != null) b.ForceKill();
            yield return new WaitForSeconds(0.1f);
        }

        if (aimingDart != null)
            Destroy(aimingDart.gameObject);

        Player.Instance.UseEightDirections = false;
        Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
        ObstacleManager.Instance.UnregisterObstacle(gameObject);
        Destroy(gameObject, 0.1f);
    }

    private IEnumerator ShowInstructionMessage(string message)
    {
        bool finished = false;
        Instantiate(instructionCanvasPrefab)
            .GetComponent<InstructionCanvas>()
            .ShowMessage(message, messageDuration, () => finished = true);

        while (!finished)
            yield return null;
    }
}
