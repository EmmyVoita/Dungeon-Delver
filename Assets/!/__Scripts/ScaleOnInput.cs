using UnityEngine;
using DG.Tweening;


public class ScaleOnInput : MonoBehaviour
{
    [SerializeField] private InputActionType inputAction;
    [SerializeField] private Ease scaleEaseIn;
    [SerializeField] private Ease scaleEaseOut;
    [SerializeField] private RectTransform rect;

    [SerializeField] private float scaleUp = 1.2f;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private float scaleDownDuration = 0.5f;

    private Sequence _scaleSequence;
    private Vector3 _baseScale;

    void Awake()
    {
        _baseScale = rect.localScale;
    }

    void Update()
    {
        if(InputBindingManager.Instance.GetKeyDown(inputAction))
        {
            Debug.Log("INPUTTTTTTT");

            rect.DOKill();

            _scaleSequence = DOTween.Sequence();


            _scaleSequence.Append(
                rect.DOScale(_baseScale * scaleUp,scaleUpDuration)
                    .SetEase(scaleEaseIn)
            );

            _scaleSequence.Append(
                rect.DOScale(_baseScale,scaleDownDuration)
                    .SetEase(scaleEaseOut)
            );   
        }
    }
}