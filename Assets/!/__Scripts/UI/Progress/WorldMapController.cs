using UnityEngine;
using DG.Tweening;
using System.Collections;

public class WorldMapController : MonoBehaviour
{
    [SerializeField] private bool useStateChange = false;
    //[SerializeField] private GameObject background;
    [SerializeField] private SoundEffect moveSound;
    [SerializeField] private bool test = false;
    [SerializeField] private WorldMapView mapView;
    [SerializeField] private Transform playerMarker;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private Ease moveEase = Ease.InOutCubic;

    [Header("Lean")]
    [SerializeField] private float maxLeanAngle = 20f;
    [SerializeField] private float leanDuration = 0.2f;
    [SerializeField] private float returnDuration = 0.3f;


    [SerializeField] private SoundEffect toggleDisplaySound;

    private int currentIndex;
    private Tween moveTween;
    private Tween rotateTween;
    private Coroutine displaySequence;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        GameStatsUI.OnStatsTallyComplete += HandleTallyComplete;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        GameStatsUI.OnStatsTallyComplete -= HandleTallyComplete;
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
            mapView.Clear();
            HideMarker();
        }   

        

        if (newState == GameState.WorldMapView && previousState != GameState.Paused)
        {
            mapView.Build();

            if(useStateChange)
            {
                if (displaySequence != null)
                {
                    StopCoroutine(displaySequence);
                    displaySequence = null;
                }

                displaySequence = StartCoroutine(DisplaySequence());
            }
        }
    }

    private void HideMarker()
    {
        playerMarker.gameObject.SetActive(false);
        //AudioHelpers.PlaySoundEffect(toggleDisplaySound, transform.position);
    }

    private void ShowMarker()
    {
        playerMarker.gameObject.SetActive(true);
        AudioHelpers.PlaySoundEffect(toggleDisplaySound, transform.position);
    }

    private IEnumerator DisplaySequence()
    {
        //background.SetActive(true);
        
        Debug.LogWarning("Current Index -> " + CurrentIndex);
        SetStartNode(Mathf.Max(CurrentIndex - 1,0));

        
        
        yield return new WaitForSeconds(1.0f);

        ShowMarker();

        yield return new WaitForSeconds(1.0f);

        MoveToNode(CurrentIndex);

        AudioHelpers.PlaySoundEffect(moveSound, transform.position);

        yield return new WaitForSeconds(0.25f);

      

        yield return new WaitForSeconds(3.0f);

        if(!test)
        {
            GameStateManager.Instance.SetState(GameState.UpgradeSelection);
            //GameStateManager.Instance.RequestStateChange(GameState.UpgradeSelection);
        } 
        //background.SetActive(false);
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

        playerMarker.position = node.transform.position;
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(0.5f);
        GameStateManager.Instance.SetState(GameState.WorldMapViewEnd);
    }

    public void MoveToNode(int index)
    {
        var node = mapView.GetNode(index);
        if (node == null) return;

        currentIndex = index;

        Vector3 start = playerMarker.position;
        Vector3 target = node.transform.position;
        Vector3 dir = (target - start).normalized;

        // Kill existing tweens (important)
        moveTween?.Kill();
        rotateTween?.Kill();

        // --- MOVE ---
        moveTween = playerMarker.DOMove(target, moveDuration)
            .SetEase(moveEase)
            .OnComplete(()=>
             {
                StartCoroutine(FinishSequence());
             });

        // --- LEAN ---
        float lean = dir.x * maxLeanAngle; // left/right tilt

        // Quick lean toward direction
        rotateTween = playerMarker
            .DORotate(new Vector3(0, 0, -lean), leanDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Return to neutral after movement
                playerMarker.DORotate(Vector3.zero, returnDuration)
                    .SetEase(Ease.OutCubic);
            });
    }
}