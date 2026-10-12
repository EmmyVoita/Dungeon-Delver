using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ArrowBoardRotator : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationAmount = 90f;
    [SerializeField] private float rotationDuration = 0.35f;
    [SerializeField] private Ease rotationEase = Ease.OutCubic;

    [Header("Timing")]
    [SerializeField] private float minRotationInterval = 8f;
    [SerializeField] private float maxRotationInterval = 12f;

    [Header("Direction")]
    [SerializeField] private bool randomDirection = true;

    [Header("Audio")]
    [SerializeField] private SoundEffect rotationStartSound;
    [SerializeField] private SoundEffect rotationEndSound;

    private Coroutine _rotationRoutine;
    private Tween _rotationTween;

    private bool shouldRotate;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousStatee, GameState newState)
    {
        if(newState == GameStateManager.LevelStartState && shouldRotate)
        {
            transform.eulerAngles = new Vector3(0,0,0);
            StartRotating();
        } 
        else if (newState == GameStateManager.LevelEndState && shouldRotate)
            StopRotating();
    }

    private void Start()
    {
        shouldRotate = GameSessionBootstrap.Config.RotateBoard;
    }

    public void StartRotating()
    {
        if (_rotationRoutine != null)
            return;

        _rotationRoutine = StartCoroutine(RotationLoop());
    }

    public void StopRotating()
    {
        if (_rotationRoutine != null)
        {
            StopCoroutine(_rotationRoutine);
            _rotationRoutine = null;
        }

        _rotationTween?.Kill();
        _rotationTween = null;
    }

    private IEnumerator RotationLoop()
    {
        while (true)
        {
            float delay = Random.Range(
                minRotationInterval,
                maxRotationInterval
            );

            yield return new WaitForSeconds(delay);

            yield return new WaitUntil(() => ObstacleManager.Instance.AnyActive == false);

            yield return RotateBoard();
        }
    }

    private IEnumerator RotateBoard()
    {
        float direction = 1f;

        if (randomDirection)
            direction = Random.value < 0.5f ? -1f : 1f;

        float targetRotation = transform.localEulerAngles.z +rotationAmount * direction;

        AudioHelpers.PlaySoundEffect(rotationStartSound,transform.position);

        bool complete = false;

        _rotationTween = transform.DOLocalRotate(new Vector3(0f, 0f, targetRotation),rotationDuration)
        .SetEase(rotationEase)
        .OnComplete(() =>
        {
            complete = true;
            AudioHelpers.PlaySoundEffect(rotationEndSound,transform.position);
        });

        yield return new WaitUntil(() => complete);

        _rotationTween = null;
    }

    private void OnDestroy()
    {
        _rotationTween?.Kill();
    }
}