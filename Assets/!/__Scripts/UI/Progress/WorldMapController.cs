using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class WorldMapController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextTypewriter typewriter;
    [SerializeField] private KeyPromptController promptController;
    [SerializeField] private WorldMapRouteSelector routeSelector;
    [SerializeField] private RectTransform worldMapRootRect;
    



    [SerializeField] private bool useStateChange = false;
    //[SerializeField] private GameObject background;
    [SerializeField] private SoundEffect moveSound;
    [SerializeField] private bool test = false;
    //[SerializeField] private WorldMapView mapView;
    [SerializeField] private WorldMapViewer mapView;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private bool animateMarker;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private Ease moveEase = Ease.InOutCubic;

    [Header("Lean")]
    [SerializeField] private float maxLeanAngle = 20f;
    [SerializeField] private float leanDuration = 0.2f;
    [SerializeField] private float returnDuration = 0.3f;

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;


    [SerializeField] private SoundEffect toggleDisplaySound;

    private int currentIndex;
    private Tween moveTween;
    private Tween rotateTween;
    private Coroutine displaySequence;

    public RectTransform WorldMapRootRect => worldMapRootRect;

  

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        //GameStatsUI.OnStatsTallyComplete += HandleTallyComplete;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        //GameStatsUI.OnStatsTallyComplete -= HandleTallyComplete;
    }

    private void Awake()
    {
        WorldMapRootRect.gameObject.SetActive(false);
    }


    private void HandleTallyComplete()
    {
        if(GameStateManager.Instance.CurrentState != GameState.WorldMapView) return;

        if (displaySequence != null)
        {
            StopCoroutine(displaySequence);
            displaySequence = null;
        }

        displaySequence = StartCoroutine(DisplaySequence());
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.Paused) return;

        if(newState != GameState.WorldMapView && newState != GameState.WorldMapViewEnd)
        {
            //promptController.PopPrompts();
            if(mapView)
                mapView.Clear();
            if(animateMarker)
                HideMarker();
        }   

        if(newState == GameState.WorldMapViewEnd)
        {
            promptController.PushPrompts(keyPrompts);
        }

        

        if (newState == GameState.WorldMapView && previousState != GameState.Paused)
        {
            WorldMapRootRect.gameObject.SetActive(true);
            //canvasGroup.alpha = 1;
            typewriter.StartTyping(RoundManager.Instance.CurrentStageName, () =>
            {
                mapView.Build(()=>
                {
                    if(useStateChange)
                    {
                        if (displaySequence != null)
                        {
                            StopCoroutine(displaySequence);
                            displaySequence = null;
                        }

                        displaySequence = StartCoroutine(DisplaySequence());
                    }
                });
            });
        }
    }

    private void HideMarker()
    {
        playerMarker.gameObject.SetActive(false);
    }

    private void ShowMarker()
    {
        playerMarker.gameObject.SetActive(true);
        AudioHelpers.PlaySoundEffect(toggleDisplaySound, transform.position);
    }

    /*
    private IEnumerator DisplaySequence()
    {
        SetStartNode(Mathf.Max(CurrentIndex - 1,0));

        yield return new WaitForSeconds(1.0f);

        ShowMarker();

        yield return new WaitForSeconds(1.0f);

        Tween movement = MoveToNode(CurrentIndex);

        AudioHelpers.PlaySoundEffect(moveSound, transform.position);

        if(movement != null)
            yield return movement.WaitForCompletion();
        
        yield return new WaitForSeconds(0.5f);

        routeSelector.Open();
    }
    */

    private IEnumerator DisplaySequence()
    {
        int startIndex = Mathf.Max(CurrentIndex - 1, 0);

        SetStartNode(startIndex);

        yield return new WaitForSecondsRealtime(0.5f);

        if(animateMarker)
        {
            //ShowMarker();

            //yield return new WaitForSecondsRealtime(0.5f);
        }
     

        //Tween movement = MoveToNode(CurrentIndex);

        AudioHelpers.PlaySoundEffect(
            moveSound,
            transform.position
        );

        //if (movement != null)
            //yield return movement.WaitForCompletion();

        yield return new WaitForSecondsRealtime(0.15f);

        Tween scrollTween = mapView.ScrollToNode(CurrentIndex);

        if (scrollTween != null)
            yield return scrollTween.WaitForCompletion();

        yield return new WaitForSecondsRealtime(0.25f);

        if(animateMarker)
        ShowMarker();

        routeSelector.Open();
    }

    private int CurrentIndex =>
        RoundManager.Instance != null
        ? RoundManager.Instance.CurrentLevelIndex
        : 0;



    public void SetStartNode(int index)
    {
        currentIndex = index;

        var node = mapView.GetNode(index);
        if (node == null) return;

        //playerMarker.position = node.transform.position;
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(0.5f);
        GameStateManager.Instance.SetState(GameState.WorldMapViewEnd);
    }

    public Tween MoveToNode(int index)
    {
        var node = mapView.GetNode(index);

        if (node == null)
            return null;

        currentIndex = index;

        Vector3 start = playerMarker.position;
        Vector3 target = node.transform.position;
        Vector3 dir = (target - start).normalized;

        moveTween?.Kill();
        rotateTween?.Kill();

        bool movingToNextStage =
            RoundManager.Instance != null &&
            RoundManager.Instance.IsAdvancingToNextStage;
        
        bool titleUpdated = false;

        moveTween = playerMarker
            .DOMove(target, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                if (!movingToNextStage || titleUpdated)
                    return;

                if (moveTween.ElapsedPercentage() >= 0.65f)
                {
                    titleUpdated = true;

                    UpdateStageTitle(
                        RoundManager.Instance.NextStageName
                    );
                }
            });

        float lean = dir.x * maxLeanAngle;

        rotateTween = playerMarker
            .DORotate(
                new Vector3(0, 0, -lean),
                leanDuration
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                playerMarker
                    .DORotate(Vector3.zero, returnDuration)
                    .SetEase(Ease.OutCubic);
            });

        return moveTween;
    }

    private void UpdateStageTitle(string stageName)
    {
        if (typewriter != null)
        {
            typewriter.StartTyping(stageName);
        }
        else if (titleText != null)
        {
            titleText.text = stageName;
        }
    }
}