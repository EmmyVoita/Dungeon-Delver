using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CountdownHandController : MonoBehaviour
{
    public float startDelay = 1.0f;
    [Header("References")]
    [SerializeField] private SpriteRenderer handRenderer;
    [SerializeField] private Transform handTransform;
    [SerializeField] private Transform rotationTransform;
    [SerializeField] private GameObject visualEffect;

    [Header("Sprites")]
    [SerializeField] private Sprite hand3;
    [SerializeField] private Sprite hand2;
    [SerializeField] private Sprite hand1;
    [SerializeField] private Sprite handClosed;

    [Header("Animation")]
    [SerializeField] private ScreenShakeRequest ssRequest;
    [SerializeField] private float punchScale = 0.25f;
    [SerializeField] private float punchDuration = 0.2f;
    [SerializeField] private float moveInDuration = 0.4f;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float interval = 1f;
    [SerializeField] private float arcHeight = 0.3f;
    [SerializeField] private AnimationCurve handLeaveCurve;
    [SerializeField] private float handLeaveDuration = 0.2f;

    [Header("Positioning")]
    [SerializeField] private Vector3 startOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 targetOffset = Vector3.zero;
    [SerializeField] private Vector3[] positions; // assign 3 points in inspector

    [Header("Audio")]
    [SerializeField] private SoundEffect countDownSound;
    [SerializeField] private SoundEffect finalSound;
    [SerializeField] private int finalSoundCount = 1;

    [SerializeField] private int counter = 0;

    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = handTransform.position;
        handTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector3 direction = handTransform.position - Vector3.zero;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotationTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public IEnumerator PlayCountdownRoutine()
    {
        StopAllCoroutines();
        handTransform.DOKill();
        yield return CountdownRoutine();
    }

    private IEnumerator CountdownRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        handTransform.gameObject.SetActive(true);

        counter = 0;

        // Entrance
        handTransform.position = basePosition + startOffset;
        handTransform.localScale = Vector3.zero;

        handTransform.DOMove(basePosition + targetOffset, moveInDuration)
            .SetEase(Ease.OutBack);

        handTransform.DOScale(1f, moveInDuration)
            .SetEase(Ease.OutBack);

        yield return new WaitForSecondsRealtime(moveInDuration);

        // 🔥 Generate randomized order of positions
        int[] order = GetRandomOrder();

        // 3 → closed → 2 → closed → 1 (moving between positions)
        yield return MoveAndStep(hand3, order[0]);
        yield return Closed();
        //yield return MoveAndClosed(order[1]);
        yield return MoveAndStep(hand2, order[2]);
        yield return Closed();
        //yield return MoveAndClosed(order[0]);
        yield return MoveAndStep(hand1, order[1]);

        // Hold on 1
        yield return new WaitForSecondsRealtime(interval * 0.5f);

        Vector3 forwardDir = (handTransform.position - Vector3.zero).normalized;
        Vector3 backOffset = forwardDir * 5f;
        handTransform.DOMove(handTransform.position + backOffset, handLeaveDuration).SetEase(handLeaveCurve);
        // Exit
        //handTransform.DOPunchScale(Vector3.one * 0.4f, 0.2f);
        //handTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack);

        for(int i = 0; i < finalSoundCount; i++)
        {
            float pitchMult = 1.0f + 0.1f * i;
            AudioHelpers.PlaySoundEffect(finalSound, transform.position,pitchMult);
            yield return new WaitForSeconds(0.1f);
        }
        

        yield return new WaitForSecondsRealtime(0.5f);

        

        handTransform.gameObject.SetActive(false);
    }

    // 🔥 RANDOMIZED ORDER (no duplicates, shuffled)
    private int[] GetRandomOrder()
    {
        int[] arr = new int[] { 0, 1, 2 };

        for (int i = 0; i < arr.Length; i++)
        {
            int rand = Random.Range(i, arr.Length);
            (arr[i], arr[rand]) = (arr[rand], arr[i]);
        }

        return arr;
    }

    // 🔥 ARC MOVEMENT (feels alive)
    private Tween MoveToPosition(int index)
    {
        Vector3 start = handTransform.position;
        Vector3 target = positions[index];

        Vector3 mid = (start + target) * 0.5f + Vector3.up * arcHeight;

        return handTransform.DOPath(
            new Vector3[] { start, mid, target },
            moveDuration,
            PathType.CatmullRom
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            //handTransform.DOPunchScale(Vector3.one * punchScale, punchDuration);
        });
    }

    private IEnumerator MoveAndStep(Sprite sprite, int posIndex)
    {
        Tween move = MoveToPosition(posIndex);
        yield return move.WaitForCompletion();

        yield return PunchSequence(sprite);

        yield return new WaitForSecondsRealtime(interval * 0.5f);
    }

    private IEnumerator Closed()
    {
        handRenderer.sprite = handClosed;
        yield return new WaitForSecondsRealtime(interval * 0.3f);
    }

    private IEnumerator MoveAndClosed(int posIndex)
    {
        handRenderer.sprite = handClosed;

        Tween move = MoveToPosition(posIndex);
        yield return move.WaitForCompletion();

        handTransform.DOPunchScale(Vector3.one * (punchScale * 0.5f), punchDuration);

        yield return new WaitForSecondsRealtime(interval * 0.3f);
    }

    private IEnumerator PunchSequence(Sprite sprite)
    {
        Vector3 forwardDir = (handTransform.position - Vector3.zero).normalized;
        Vector3 backOffset = forwardDir * -0.3f;
        Vector3 forwardOffset = forwardDir * 0.2f;

        Sequence s = DOTween.Sequence();

        // 🔹 1. Pull back (anticipation)
        s.Append(
            handTransform.DOMove(handTransform.position + backOffset, 0.1f)
        );
        s.Join(
            handTransform.DOScale(1.2f, 0.1f)
        );

        // 🔹 2. Punch forward (impact moment)
        s.AppendCallback(() =>
        {
            handRenderer.sprite = sprite; // 🔥 swap HERE (impact)
            ScreenShakeManager.Instance.Shake(ssRequest);

            float pitchMult = 1.0f + counter * 0.1f;
            AudioHelpers.PlaySoundEffect(countDownSound, transform.position, pitchMult);

            counter += 1;
            if(visualEffect)
            {
            
                Instantiate(visualEffect,transform.position,Quaternion.identity);
            }
        });

        s.Append(
            handTransform.DOMove(handTransform.position + forwardOffset, 0.08f)
        );
        s.Join(
            handTransform.DOScale(0.95f, 0.08f)
        );

        // 🔹 3. Settle
        s.Append(
            handTransform.DOScale(1f, 0.1f)
        );

        yield return s.WaitForCompletion();

    }
}