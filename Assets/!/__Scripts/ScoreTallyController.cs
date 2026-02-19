using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public enum TallyType { Combo, AbilityCharge }
public struct TallyTick
{
    public int addedScore;
    public TallyType type;
    public int index;
    public int total;
}

public class ScoreTallyController : MonoBehaviour
{
    public static event Action<TallyTick> OnTallyTick;
    public static event Action<TallyType> OnTallyStart;
    public static event Action<TallyType> OnTallyComplete;
    public static event Action RoundEndTallyComplete;



  
    // Struct to hold queued tally requests
    private struct TallyRequest
    {
        public int count;
        public TallyType type;
        public Action onComplete;

        public TallyRequest(int count, TallyType type, Action onComplete = null)
        {
            this.count = count;
            this.type = type;
            this.onComplete = onComplete;
        }
    }

    [SerializeField] private float countDelay = 0.1f;
    private readonly Queue<TallyRequest> tallyQueue = new Queue<TallyRequest>();
    private bool isTallying = false;
    private Coroutine tallyRoutine;

    public void AnimateComboAdd(Action onComplete = null)
    {
        int comboCount = ComboManager.Instance.GetCurrentComboCount;
        if (comboCount <= 0)
        {
            onComplete?.Invoke();
            return;
        } 

        tallyQueue.Enqueue(new TallyRequest(comboCount, TallyType.Combo, onComplete));

        Debug.Log($"Enqueued Combo Tally: {comboCount}, isTallying={isTallying}");

        if (!isTallying) EnsureTallyProcessor();
    }

    public IEnumerator StartRoundEndTally()
    {
        bool finished = false;

        AnimateComboAdd(() => finished = true);

        
        yield return CoroutineHelpers.WaitUntilOrTimeout(
            () => finished,
            10.0f
        );
        

        finished = false;

        tallyQueue.Enqueue(new TallyRequest(
            Player.Instance.AbilityCharge,
            TallyType.AbilityCharge,
            () => finished = true
        ));

        if (!isTallying) EnsureTallyProcessor();
            

        yield return  CoroutineHelpers.WaitUntilOrTimeout(
            () => finished,
            10.0f
        );

        RoundEndTallyComplete?.Invoke();
    }


     private void EnsureTallyProcessor()
    {
        if (tallyRoutine == null)
            tallyRoutine = StartCoroutine(ProcessTallyQueue());
    }

    private IEnumerator ProcessTallyQueue()
    {
        isTallying = true;

        while (tallyQueue.Count > 0)
        {
            TallyRequest request = tallyQueue.Dequeue();
            yield return StartCoroutine(TallyScoreRoutine(
                request.count, 
                request.onComplete, 
                request.type));
        }

        isTallying = false;
        tallyRoutine = null;
    }

    private IEnumerator TallyScoreRoutine(int count, Action onComplete, TallyType type)
    {
        if (count <= 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        OnTallyStart?.Invoke(type);

        for (int i = 0; i < count; i++)
        {
            int added = 0;

            switch (type)
            {
                case TallyType.Combo:
                    added = Mathf.RoundToInt(ComboManager.CalculateComboScore(i + 1));
                    break;

                case TallyType.AbilityCharge:
                    if (Player.Instance.AbilityCharge <= 0) break;
                    added = Mathf.RoundToInt(ScoreManager.Instance.abilityChargeScorePerUnit);
                    Player.Instance.AbilityCharge -= 1;
                    break;
            }

            int finalAddedScore = ScoreManager.Instance.AddScore(added,type == TallyType.Combo ? ScoreSource.Combo : ScoreSource.Bonus);

            OnTallyTick?.Invoke(new TallyTick
            {
                addedScore = finalAddedScore,
                type = type,
                index = i,
                total = count
            });

            float scaledDelay = countDelay * ComboDelayScale(count);
            scaledDelay = Mathf.Max(scaledDelay, 0.015f);

            yield return new WaitForSeconds(scaledDelay);
        }

        OnTallyComplete?.Invoke(type);
        onComplete?.Invoke();
    }

    float ComboDelayScale(int totalCombo)
    {
        const int softCap = 40;        // no speed-up below this
        const int hardCap = 225;       // max expected combo
        const float minScale = 0.35f;  // fastest allowed (35% of base)

        if (totalCombo <= softCap)
            return 1f;

        float t = Mathf.InverseLerp(softCap, hardCap, totalCombo);

        // Ease-out curve: slow at first, stronger later
        t = Mathf.Pow(t, 2f);

        return Mathf.Lerp(1f, minScale, t);
    }



}