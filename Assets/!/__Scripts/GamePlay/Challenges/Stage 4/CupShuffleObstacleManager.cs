using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CupShuffleObstacleManager : MonoBehaviour
{
    public GameObject timerRingPrefab;
    public float roundTimeLimit =5f;
    public float timerVerticalOffset = -200f;

    public AudioClip sucessSound;
    public AudioClip failSound;
    public AudioClip shuffleSound;
    public AudioClip coverSound;

    [Header("Cups")]
    public Transform cupPrefab;
    public Transform ballPrefab;
    public Transform spikyballPrefab;

    [Header("Shuffle Settings")]
    public int shuffleCount = 10;
    public float shuffleTime = 0.25f;

    [Header("Positions")]
    public Vector2 spawnTopOffset = new Vector2(0, 6f);
    public Vector3 triangleLeftOffset  = new Vector3(-2f, -1f, 0);
    public Vector3 triangleTopOffset   = new Vector3( 0f,  1f, 0);
    public Vector3 triangleRightOffset = new Vector3( 2f, -1f, 0);

    public Vector2 leftOffset  = new Vector2(-3f, 1.2f);
    public Vector2 topOffset   = new Vector2( 0f, 3f);
    public Vector2 rightOffset = new Vector2( 3f, 1.2f);

    [Header("Reveal / Hiding")]
    public float ballHeightOffset = -0.5f;
    public float liftHeight = 0.5f;
    public float hideDelay = 0.8f;
    public float hideDuration = 0.35f;
    public float cupToPlayerSideDuration = 0.8f;

    [Header("Wiggle on Selection")]
    public float wiggleStrength = 5f;
    public int wiggleVibrato = 8;
    public float wiggleElasticity = 0.3f;
    public float wiggleLength = 6;

    private Transform[] cups = new Transform[3];
    private Vector3[] shufflePositions = new Vector3[3];


    private int ballCupIndex = -1;

    private Transform ball;
    private Transform spikyballA;
    private Transform spikyballB;


    private bool choosing = false;
    private GameObject timerRingInstance;
    private int selectedCupIndex = -1;


    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        StartCoroutine(ObstacleRoutine());

        timerRingInstance = Instantiate(timerRingPrefab, transform.position, Quaternion.identity);
    }


    private IEnumerator ObstacleRoutine()
    {
        SpawnCupsInTriangle();
        SpawnBall();

        yield return HideBallUnderCup();
        yield return ShuffleRoutine();
        yield return MoveCupsToPlayerSides();

        choosing = true;
    }


    // ================================================================
    //  SPAWN PHASE
    // ================================================================

    void SpawnCupsInTriangle()
    {
        Vector3 basePos = Vector3.zero + (Vector3)spawnTopOffset;

        shufflePositions[0] = basePos + triangleLeftOffset;
        shufflePositions[1] = basePos + triangleTopOffset;
        shufflePositions[2] = basePos + triangleRightOffset;

        for (int i = 0; i < 3; i++)
        {
            cups[i] = Instantiate(cupPrefab, shufflePositions[i], Quaternion.identity);
            cups[i].transform.parent = this.transform;
            AnimateCupAppear(cups[i]);

            var selector = cups[i].gameObject.GetComponent<CupSelector>();
            selector.Init(this, i);
        }
    }


    void SpawnBall()
    {
        ballCupIndex = Random.Range(0, 3);

        // --- Real ball ---
        ball = Instantiate(ballPrefab);
        ball.position = shufflePositions[ballCupIndex] + new Vector3(0, ballHeightOffset, 0);

        // --- Spiky balls for wrong cups ---
        int spikyAIndex = (ballCupIndex + 1) % 3;
        int spikyBIndex = (ballCupIndex + 2) % 3;

        spikyballA = Instantiate(spikyballPrefab);
        spikyballA.position = shufflePositions[spikyAIndex] + new Vector3(0, ballHeightOffset, 0);

        spikyballB = Instantiate(spikyballPrefab);
        spikyballB.position = shufflePositions[spikyBIndex] + new Vector3(0, ballHeightOffset, 0);
        
        ball.GetComponent<SpriteRenderer>().color = Color.clear;
        ball.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.25f);

        spikyballA.GetComponent<SpriteRenderer>().color = Color.clear;
        spikyballA.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.25f);

        spikyballB.GetComponent<SpriteRenderer>().color = Color.clear;
        spikyballB.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.25f);
    }



    // ================================================================
    //  HIDE BALL
    // ================================================================

    IEnumerator HideBallUnderCup()
    {
        // Lift ALL cups first
        for (int i = 0; i < 3; i++)
            cups[i].position += new Vector3(0, liftHeight, 0);

        yield return new WaitForSeconds(hideDelay);

        

        // Lower ALL cups
        for (int i = 0; i < 3; i++)
        {
            yield return cups[i].DOMoveY(cups[i].position.y - liftHeight, hideDuration)
                .SetEase(Ease.InBack)
                .WaitForCompletion();

            AudioHelpers.PlayMyClipAtPoint(coverSound, AudioChannel.SFX, Camera.main.transform.position);
        }

        // Hide sprites for ALL balls
        ball.GetComponent<SpriteRenderer>().color = Color.clear;
        spikyballA.GetComponent<SpriteRenderer>().color = Color.clear;
        spikyballB.GetComponent<SpriteRenderer>().color = Color.clear;
    }


    // ================================================================
    //  SHUFFLING
    // ================================================================

    IEnumerator ShuffleRoutine()
    {
        for (int i = 0; i < shuffleCount; i++)
        {
            int a = Random.Range(0, 3);
            int b;
            do { b = Random.Range(0, 3); } while (b == a);

            yield return SwapCups(a, b);
        }
    }

    public void AnimateCupAppear(Transform cup)
    {
        SpriteRenderer sr = cup.GetComponent<SpriteRenderer>();
        sr.color = new Color(1,1,1,0);

        float startY = cup.position.y + liftHeight + 3f;
        cup.position = new Vector3(cup.position.x, startY, cup.position.z);

        Sequence seq = DOTween.Sequence();

        seq.Join(sr.DOFade(1f, 0.35f))
        .Join(cup.DOMoveY(startY - 3f, 0.45f).SetEase(Ease.OutBack));
    }



    IEnumerator SwapCups(int a, int b)
    {
        Vector3 posA = shufflePositions[a];
        Vector3 posB = shufflePositions[b];

        float tiltAmount = 10f;       // degrees of tilt
        float tiltTime = shuffleTime * 0.5f;

        // calculate movement direction for each cup
        float dirA = Mathf.Sign(posB.x - posA.x); // +1 = moving right, -1 = moving left
        float dirB = Mathf.Sign(posA.x - posB.x);

        // movement tweens
        var t1 = cups[a].DOMove(posB, shuffleTime).SetEase(Ease.InOutQuad);
        var t2 = cups[b].DOMove(posA, shuffleTime).SetEase(Ease.InOutQuad);

        // rotation tweens (tilt OPPOSITE the direction)
        cups[a].DORotate(new Vector3(0, 0, -dirA * tiltAmount), tiltTime)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                // restore rotation to neutral
                cups[a].DORotate(Vector3.zero, tiltTime).SetEase(Ease.InSine);
            });

        cups[b].DORotate(new Vector3(0, 0, -dirB * tiltAmount), tiltTime)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                cups[b].DORotate(Vector3.zero, tiltTime).SetEase(Ease.InSine);
            });

        // swap stored positions
        shufflePositions[a] = posB;
        shufflePositions[b] = posA;

        AudioHelpers.PlayClipWithVariation(shuffleSound, AudioChannel.SFX, Camera.main.transform.position, pitchRange: 0.2f);

        yield return t1.WaitForCompletion();
    }



    // ================================================================
    // MOVE TO FINAL PLAYER SIDES
    // ================================================================

    IEnumerator MoveCupsToPlayerSides()
    {
        Vector3 basePos = Vector3.zero;

        Vector3[] finalPositions = new Vector3[]
        {
            basePos + (Vector3)leftOffset,
            basePos + (Vector3)topOffset,
            basePos + (Vector3)rightOffset
        };

        for (int i = 0; i < 3; i++)
            cups[i].DOMove(finalPositions[i], cupToPlayerSideDuration).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(cupToPlayerSideDuration);

        // Enable selection
        for (int i = 0; i < 3; i++)
            cups[i].GetComponent<CupSelector>().OnShuffled();

        timerRingInstance?.GetComponent<BasicFillBar>().Show(roundTimeLimit,() => 
        {
            StartCoroutine(HandleFailure(true));
        }, 
        new Vector2(0, timerVerticalOffset));
    }




    // ================================================================
    //  SELECTION
    // ================================================================

    private Tween PlayCupWiggle(Transform cup, float liftHeight)
    {
        Sequence seq = DOTween.Sequence();

        // Lift first
        seq.Append(cup.DOMoveY(cup.position.y + liftHeight, 0.25f).SetEase(Ease.OutCubic));

        // Even back-and-forth rotation (centered wiggle)
        seq.Join(
            cup.DOPunchRotation(
                new Vector3(0, 0, wiggleStrength), // peak rotation
                duration: wiggleLength,
                vibrato: wiggleVibrato,
                elasticity: wiggleElasticity
            )
        );

        return seq;
    }



    public void ChooseCup(int selectedCupIndex)
    {
        if (!choosing) return;
        choosing = false;

        this.selectedCupIndex = selectedCupIndex;

        bool correct = (selectedCupIndex == ballCupIndex);

        if (correct)
        {
            // Reveal ball
            ball.position = cups[selectedCupIndex].position + new Vector3(0, ballHeightOffset, 0);
            ball.GetComponent<SpriteRenderer>().color = Color.white;

            StartCoroutine(HandleSuccess());
        }
        else
        {
            // Reveal spiky ball for THAT specific wrong cup
            Transform spikyToReveal = null;

            int spikyAIndex = (ballCupIndex + 1) % 3;
            int spikyBIndex = (ballCupIndex + 2) % 3;

            if (selectedCupIndex == spikyAIndex) spikyToReveal = spikyballA;
            else if (selectedCupIndex == spikyBIndex) spikyToReveal = spikyballB;

            if (spikyToReveal != null)
            {
                spikyToReveal.position = cups[selectedCupIndex].position + new Vector3(0, ballHeightOffset, 0);
                spikyToReveal.GetComponent<SpriteRenderer>().color = Color.white;
            }

            StartCoroutine(HandleFailure());
        }
    }


    IEnumerator HandleFailure(bool timerExpired = false)
    {
        Debug.Log("Wrong!");
        AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);


        foreach (var c in cups)
        {
            c.GetComponent<CupSelector>().DisableCol();
        }

        timerRingInstance?.GetComponent<BasicFillBar>().HideSmooth();

        if (!timerExpired)
        {
            Transform selectedCup = cups[selectedCupIndex];

            yield return PlayCupWiggle(selectedCup, liftHeight).WaitForCompletion();
        }


        yield return spikyballA.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.2f).WaitForCompletion();
        yield return spikyballB.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.2f).WaitForCompletion();
        
        foreach (var c in cups)
        {
            c.GetComponent<CupSelector>().OnDeath();
            yield return new WaitForSeconds(0.2f);
        }

        // /layer.Instance.DamageSelf(1); // or your damage system
        

        yield return new WaitForSeconds(2.0f);

        ObstacleManager.Instance.UnregisterObstacle(gameObject);

        Destroy(timerRingInstance);
        Destroy(gameObject);
    }

    IEnumerator HandleSuccess()
    {
        AudioHelpers.PlayMyClipAtPoint(sucessSound, AudioChannel.SFX, Camera.main.transform.position);
        timerRingInstance?.GetComponent<BasicFillBar>().HideSmooth();

        // Move the cup sequence
        {
            Transform selectedCup = cups[selectedCupIndex];

            yield return PlayCupWiggle(selectedCup, liftHeight).WaitForCompletion();


            foreach (var c in cups)
            {
                c.GetComponent<CupSelector>().OnDeath();
                yield return new WaitForSeconds(0.2f);
            }

            Destroy(spikyballA.gameObject);
            Destroy(spikyballB.gameObject); 
            Destroy(ball.gameObject);
        }


        yield return new WaitForSeconds(2.0f);

        ObstacleManager.Instance.UnregisterObstacle(gameObject);

        Destroy(timerRingInstance);
        Destroy(gameObject);
    }


    // ================================================================
    //  REVEAL
    // ================================================================

    /*
    IEnumerator RevealRoutine(Transform selectedCup, bool success)
    {
       

        ObstacleManager.Instance.UnregisterObstacle(gameObject);
        Destroy(gameObject);
    }
    */
}
