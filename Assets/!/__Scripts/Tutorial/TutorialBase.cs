using DG.Tweening;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class TutorialBase : MonoBehaviour
{
    [Header("Typewriter")]
    [SerializeField] protected RectTransform typewriterRect;
    [SerializeField] private float wobbleHeight = 0.1f;
    

    protected bool _tutorialActive;
    protected Tween _idleWobbleTween;
    private Vector3 _typewriterBasePos;

    
    public bool TutorialComplete { get; protected set; } = false;

    public virtual void StartTutorial()
    {
        _tutorialActive = true;
    }

    protected virtual void CompleteTutorial()
    {
        _tutorialActive = false;
        TutorialComplete = true;
    }

    protected virtual void Awake()
    {
        _tutorialActive = false;
        TutorialComplete = false;
        if(typewriterRect)
            _typewriterBasePos = typewriterRect.anchoredPosition;
    }

    protected virtual void StartIdleWobble()
    {
        if(!typewriterRect)
            return;

        _idleWobbleTween?.Kill();
        _idleWobbleTween = typewriterRect
            .DOAnchorPosY(_typewriterBasePos.y + 10f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    protected virtual void KillIdleWobble()
    {
        _idleWobbleTween?.Kill();
        typewriterRect.anchoredPosition = _typewriterBasePos;
    }
}