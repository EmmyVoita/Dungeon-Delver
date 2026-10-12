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
    [SerializeField] private MovementTutorialUI movementTutorial;
    [SerializeField] private ObstacleTutorialUI obstacleTutorial;
    [SerializeField] private AbilityTutorialUI abilityTutorial;

    [Header("Root Objects")]
    [SerializeField] private GameObject obstacleTutorialRoot;
    [SerializeField] private GameObject movementTutorialRoot;
    [SerializeField] private GameObject abilityTutorialRoot;

    [Header("Tutorial Timing")]
    [SerializeField] private float tutorialStartDelay = 1.0f;

    [Header("Section Transition")]
    [SerializeField] private float sectionTransitionLeadIn = 0.75f;
    [SerializeField] private float sectionTransitionPause = 1.0f;

    [Header("Tutorial End")]
    [SerializeField] private float tutorialEndDelay = 1.0f;
    [SerializeField] private float exitFadeDuration = 0.5f;


    [Header("Audio")]
    public SoundEffect transitionSound;
    private AudioSource audioSource;


    [Header("Menu Transition")]
    [SerializeField] private Material transitionMaterial;
    [SerializeField] private string transitionPropertyName = "_TransitionValue";
    [SerializeField] private float targetValue = 1;


    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Awake()
    {
        movementTutorialRoot.SetActive(false);
        obstacleTutorialRoot.SetActive(false);
        abilityTutorialRoot.SetActive(false);
    }

    private void Update()
    {
        #if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.Keypad9))
            {
                StopAllCoroutines();
                StartCoroutine(FinishTutorial());
            }
        #endif
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
        yield return new WaitForSeconds(tutorialStartDelay);

        // Movement tutorial
        movementTutorialRoot.SetActive(true);
        movementTutorial.StartTutorial();
        yield return new WaitUntil(() => movementTutorial.TutorialComplete);
        movementTutorialRoot.SetActive(false);

        // transition between tutorials
        yield return StartCoroutine(PlayTransition());

        // Obstacle tutorial
        obstacleTutorialRoot.SetActive(true);
        obstacleTutorial.StartTutorial();
        yield return new WaitUntil(() => obstacleTutorial.TutorialComplete);
        obstacleTutorialRoot.SetActive(false);

        yield return StartCoroutine(PlayTransition());

        // Ability tutorial
        abilityTutorialRoot.SetActive(true);
        abilityTutorial.StartTutorial();
        yield return new WaitUntil(() => abilityTutorial.TutorialComplete);
        abilityTutorialRoot.SetActive(false);

        yield return StartCoroutine(PlayTransition());

        StartCoroutine(FinishTutorial());
    }


    private IEnumerator PlayTransition()
    {
        yield return new WaitForSeconds(sectionTransitionLeadIn);
        AudioHelpers.PlaySoundEffect(transitionSound, transform.position);
        yield return new WaitForSeconds(sectionTransitionPause);
    }

    private IEnumerator FinishTutorial()
    {
        yield return new WaitForSeconds(tutorialEndDelay);

        transitionMaterial.DOFloat(targetValue,transitionPropertyName, exitFadeDuration);
        SceneEntryContext.EntryType = SceneEntryType.ReturnFromTutorial;

        yield return new WaitForSeconds(exitFadeDuration);

        // Go back to main menu
        Time.timeScale = 1f;
        SceneReturnHandler.ReturnToAbilitySelect = false;
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
