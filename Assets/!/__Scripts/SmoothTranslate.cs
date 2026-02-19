using UnityEngine;
using DG.Tweening;

public class SmoothTranslate : MonoBehaviour
{
    private enum TranslateSpace
    {
        Local,
        World
    }

    [Header("Move Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(5f, 0f, 0f);
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private Ease moveEase = Ease.InOutSine;
    [SerializeField] private TranslateSpace translateSpace = TranslateSpace.Local;
    [SerializeField] private bool playOnAwake = false;

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
        if(playOnAwake)
        {
            PlayMove();
        }
    }

    [ContextMenu("Play Move")]
    public void PlayMove()
    {
        if (translateSpace == TranslateSpace.Local)
        {
            transform
                .DOLocalMove(startPos + moveOffset, moveDuration)
                .SetEase(moveEase);
        }
        else
        {
            transform
                .DOMove(startPos + moveOffset, moveDuration)
                .SetEase(moveEase);
        }
    }

    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        transform.position = startPos;
    }
}
