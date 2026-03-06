using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private Canvas gameOverCanvas;   // 🔹 Add this in the Inspector
    [SerializeField] private GameObject pauseDimmer;
    [SerializeField] private AudioSource pauseSound;

    [Header("Settings")]
    [SerializeField] private Key toggleKey = Key.Escape;
    [SerializeField] private float gameOverDelay = 0.5f;
    public AudioClip gameOverClip;

    public static bool IsPaused { get; private set; } = false;
    public static event Action OnResumeCountdownStarted;
    public static event Action OnGameOver;
    public static event Action OnGameOverUI;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    void OnEnable()
    {
        Player.OnDamageTaken += HandlePlayerDamage;   // 👈 Subscribe
    }

    void OnDisable()
    {
        Player.OnDamageTaken -= HandlePlayerDamage;
    }

    void Start()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(true);
            settingsCanvas.enabled = false;
        }

        if (pauseDimmer != null)
            pauseDimmer.SetActive(false);

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
            gameOverCanvas.enabled = false;
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        //if (kb[toggleKey].wasPressedThisFrame && !gameOverCanvas.enabled && settingsCanvas != null)
        //    TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused)
            StartResumeCountdown();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        if (settingsCanvas != null) settingsCanvas.enabled = true;
        if (pauseDimmer != null) pauseDimmer.SetActive(true);
        if (pauseSound != null) pauseSound.Play();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Game Paused");
    }

    private void StartResumeCountdown()
    {
        OnResumeCountdownStarted?.Invoke();
        if (settingsCanvas != null) settingsCanvas.enabled = false;
        IsPaused = false;
    }

    public void FinalizeResume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseDimmer != null) pauseDimmer.SetActive(false);
        if (pauseSound != null) pauseSound.Play();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Game Resumed");
    }

    // 🔹 Called whenever the Player takes damage
    private void HandlePlayerDamage(int currentHealth)
    {
        if (currentHealth <= 0)
        {
            OnGameOver?.Invoke();
           StartCoroutine(GameOverSequence());
        }
    }

    
    private IEnumerator GameOverSequence()
    {
        // 🔹 Step 1: Broadcast + sound
        OnGameOver?.Invoke();
        if (gameOverClip != null)
            AudioHelpers.PlayMyClipAtPoint(gameOverClip, AudioChannel.SFX, Camera.main.transform.position);

        // 🔹 Step 2: Enter slow motion
        float originalTimeScale = Time.timeScale;
        float targetSlowMo = 0.25f;         // 25 % speed
        float slowMoFadeTime = 0.2f;        // fade into slow-mo over 0.2 s real time
       // float holdSlowMoTime = 0.4f;        // stay slowed for 0.4 s real time

        float elapsed = 0f;
        while (elapsed < slowMoFadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(originalTimeScale, targetSlowMo, elapsed / slowMoFadeTime);
            yield return null;
        }
        Time.timeScale = targetSlowMo;

        // 🔹 Step 3: Hold the slow-mo for dramatic effect
        yield return new WaitForSecondsRealtime(gameOverDelay);

        // 🔹 Step 4: Freeze world time and show UI
        Time.timeScale = 0f;
        IsPaused = true;

        if (pauseDimmer != null)
            pauseDimmer.SetActive(true);

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(true);
            gameOverCanvas.enabled = true;
        }
           

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        OnGameOverUI?.Invoke();

        Debug.Log("🩸 Game Over Screen Shown");
    }

}
