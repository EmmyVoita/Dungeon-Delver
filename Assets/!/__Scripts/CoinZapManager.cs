using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CoinZapManager : MonoBehaviour
{
    public static CoinZapManager Instance;

    [Header("Targeting")]
    [SerializeField] private LayerMask arrowLayerMask;

    [Header("Audio")]
    [SerializeField] private SoundEffect procSoundEffect;

    [Header("VFX")]
    [SerializeField] private GameObject zapVFX;
    [SerializeField] private LightningZapEffect zapPrefab;
    [SerializeField] private Transform zapOrigin;
    [SerializeField] private float minZapDistance = 2f;

    [SerializeField] private float queueProcessDelay = 0.08f;

    //private readonly Queue<ArrowBase> _pendingZaps = new();
    private bool _processingQueue;

    private int _hits;

    private struct ZapRequest
    {
        public float radius;
        public bool preventComboBreak;
        public bool empowered;

        public ZapRequest(float radius, bool preventComboBreak, bool empowered)
        {
            this.radius = radius;
            this.preventComboBreak = preventComboBreak;
            this.empowered = empowered;
        }
    }

    private readonly Queue<ZapRequest> _pendingZaps = new();

    private void Awake()
    {
        Instance = this;
    }

    public bool HasTarget(float radius)
    {
        return GetBestArrow(radius) != null;
    }

    public ArrowBase GetFurthestArrow(float radius)
    {
        Vector2 origin = zapOrigin != null
            ? zapOrigin.position
            : Player.Instance.transform.position;

        Collider2D[] coll = Physics2D.OverlapCircleAll(
            origin,
            radius,
            arrowLayerMask
        );

        ArrowBase furthest = null;
        float furthestDistanceSqr = minZapDistance * minZapDistance;

        for (int i = 0; i < coll.Length; i++)
        {
            if (coll[i] == null)
                continue;

            ArrowBase arrow = coll[i].GetComponentInParent<ArrowBase>();

            if (arrow == null)
                continue;

            if(arrow.IsDead)
                continue;

            float distSqr =
                ((Vector2)arrow.transform.position - origin).sqrMagnitude;

            if (distSqr > furthestDistanceSqr)
            {
                furthestDistanceSqr = distSqr;
                furthest = arrow;
            }
        }

        Debug.Log($"ZAPPING TARGET => {furthest}. HITS => {coll.Length}");

        return furthest;
    }

    public ArrowBase GetBestArrow(float radius)
    {
        Vector2 origin = zapOrigin != null
            ? zapOrigin.position
            : Player.Instance.transform.position;

        Collider2D[] coll = Physics2D.OverlapCircleAll(
            origin,
            radius,
            arrowLayerMask
        );

        ArrowBase best = null;
        float bestScore = float.NegativeInfinity;
        float minDistanceSqr = minZapDistance * minZapDistance;

        for (int i = 0; i < coll.Length; i++)
        {
            if (coll[i] == null)
                continue;

            ArrowBase arrow = coll[i].GetComponentInParent<ArrowBase>();

            if (arrow == null || arrow.IsDead)
                continue;

            float distSqr = ((Vector2)arrow.transform.position - origin).sqrMagnitude;

            if (distSqr < minDistanceSqr)
                continue;

            float score = 0f;

            if (arrow.IsGolden)
                score += 1000f;

            // Prefer closer arrows after golden priority.
            score -= distSqr;

            if (score > bestScore)
            {
                bestScore = score;
                best = arrow;
            }
        }

        Debug.Log($"ZAPPING TARGET => {best}. HITS => {coll.Length}");

        return best;
    }

    public void ZapArrow(ArrowBase target, bool preventComboBreak, bool empowered)
    {
        Debug.Log("ZAPPING ARROW");

        if (target == null)
            return;

        Vector3 origin = zapOrigin != null
            ? zapOrigin.position
            : Player.Instance.transform.position;

        if (zapPrefab != null)
        {
            LightningZapEffect zap = Instantiate(zapPrefab);
            zap.Play(origin, target.transform.position);

            
        }

        GameObject vfxObj = Instantiate(zapVFX,  target.transform.position, quaternion.identity);
        EmpoweredVisualEffect empoweredEffect = vfxObj.GetComponent<EmpoweredVisualEffect>();
        empoweredEffect?.Play(empowered);

        AudioHelpers.PlaySoundEffect(procSoundEffect, target.transform.position);

        target.KillArrow(Goal.GoalType.Critical, true);
    }

    public void QueueZap(float radius, bool preventComboBreak, bool empowered)
    {
        _pendingZaps.Enqueue(new ZapRequest(radius, preventComboBreak, empowered));

        if (!_processingQueue)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _processingQueue = true;

        while (_pendingZaps.Count > 0)
        {
            yield return new WaitForSeconds(queueProcessDelay);

            ZapRequest request = _pendingZaps.Dequeue();

            ArrowBase arrow = GetBestArrow(request.radius);

            if (arrow != null)
                ZapArrow(arrow, request.preventComboBreak, request.empowered);
        }

        _processingQueue = false;
    }
}