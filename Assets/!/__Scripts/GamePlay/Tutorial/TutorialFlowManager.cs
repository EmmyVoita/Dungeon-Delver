using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// Central controller that sequences Movement, Obstacle, and Ability tutorials.
/// Attach this to a GameObject in your tutorial scene.
/// </summary>
public class TutorialFlowManager : MonoBehaviour
{
    [Header("Tutorial Sections")]
    public MovementTutorialUI movementTutorial;
    public ObstacleTutorialUI obstacleTutorial;
    public AbilityTutorialUI abilityTutorial;

    [Header("Transition Settings")]
    public float finalTransitionDelay = 1.0f;
    public float transitionDelay = 1.0f;
    public float fadeDuration = 0.5f;

    [Header("Audio")]
    public AudioClip transitionSound;
    private AudioSource audioSource;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.Tutorial && previousState != newState)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            StartCoroutine(RunTutorialSequence());
        }
    }

    private IEnumerator RunTutorialSequence()
    {
        // --- STEP 1: MOVEMENT TUTORIAL ---
        
        obstacleTutorial.gameObject.SetActive(false);
        abilityTutorial.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f); // slight delay before starting
        movementTutorial.gameObject.SetActive(true);
       

        yield return new WaitUntil(() => movementTutorial.TutorialComplete);
        movementTutorial.gameObject.SetActive(false);

        // Optional transition between tutorials
        yield return StartCoroutine(PlayTransition("Good! Now let's learn how to dodge obstacles."));

        // --- STEP 2: OBSTACLE TUTORIAL ---
        obstacleTutorial.gameObject.SetActive(true);
        yield return new WaitUntil(() => obstacleTutorial.TutorialComplete);
        obstacleTutorial.gameObject.SetActive(false);

        yield return StartCoroutine(PlayTransition("Nice work! Now let's charge and use your ability."));

        // --- STEP 3: ABILITY TUTORIAL ---
        abilityTutorial.gameObject.SetActive(true);
        yield return new WaitUntil(() => abilityTutorial.TutorialComplete);

        yield return StartCoroutine(PlayTransition("Complete"));

        // --- STEP 4: END ---
        StartCoroutine(FinishTutorial());
    }

    // ----------------------------------------
    // 🎬 Transition message or fade
    // ----------------------------------------
    private IEnumerator PlayTransition(string message)
    {
        // Optionally play a sound between steps
        if (transitionSound != null)
            audioSource.PlayOneShot(transitionSound);

        yield return new WaitForSeconds(transitionDelay);
    }

    private IEnumerator FinishTutorial()
    {
        yield return new WaitForSeconds(finalTransitionDelay);

        // Example: Load main game scene
        Time.timeScale = 1f;
        SceneReturnHandler.ReturnToAbilitySelect = false;
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
