using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

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
    public SoundEffect transitionSound;
    public float transitionStartDelay = 0.75f;
    private AudioSource audioSource;

    [Header("Menu Transition")]
    [SerializeField] private Material transitionMaterial;
    [SerializeField] private string transitionPropertyName = "_TransitionValue";
    [SerializeField] private float targetValue = 1;
    [SerializeField] private float duration = 0.5f;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad9))
        {
            StopAllCoroutines();
            StartCoroutine(FinishTutorial());
        }
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
        yield return new WaitForSeconds(transitionStartDelay);

        // Optionally play a sound between steps
        AudioHelpers.PlaySoundEffect(transitionSound, transform.position);

        yield return new WaitForSeconds(transitionDelay);
    }

    private IEnumerator FinishTutorial()
    {
        yield return new WaitForSeconds(finalTransitionDelay);

        

        transitionMaterial.DOFloat(targetValue,transitionPropertyName, duration);

        SceneEntryContext.EntryType = SceneEntryType.ReturnFromTutorial;

        yield return new WaitForSeconds(duration);

        // Example: Load main game scene
        Time.timeScale = 1f;
        SceneReturnHandler.ReturnToAbilitySelect = false;
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
