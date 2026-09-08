using UnityEngine;

public class RotationArrow : ArrowBase
{
    [Header("Rotation Hold")]
    [SerializeField] private float holdDuration = 0.75f;

    [Tooltip("How long the goal can briefly lose contact before the note fails.")]
    [SerializeField] private float contactGraceTime = 0.08f;

    [Header("Path")]
    [SerializeField] private EdgeCollider2D pathCollider;
    [SerializeField] private LineRenderer pathVisual;

    [Tooltip("Goal collider that must remain touching the path.")]
    [SerializeField] private Collider2D goalCollider;

    [Header("Arc Shape")]
    [SerializeField] private float arcRadius = 2f;

    [Tooltip("Number of segments used to build the visible/collision arc.")]
    [SerializeField] private int arcSegments = 16;

    [Tooltip("True = rotate 90 degrees clockwise.")]
    [SerializeField] private bool clockwise = true;

    [Header("Debug")]
    [SerializeField] private bool drawDebugPath = true;


    private float _holdStartTime;
    private float _holdEndTime;

    private float _startAngle;
    private float _endAngle;

    private bool _holdStarted;
    private bool _holdResolved;

    private float _contactLostTime = -1f;


    public override void Init(
        Vector2 direction,
        float speed,
        float spawnTime,
        float arrivalTime,
        Vector3 startPos,
        Vector3 endPos)
    {
        base.Init(
            direction,
            speed,
            spawnTime,
            arrivalTime,
            startPos,
            endPos
        );

        _holdStartTime = arrivalTime;
        _holdEndTime = arrivalTime + holdDuration;

        _startAngle = DirectionToAngle(direction);

        float rotationAmount =
            clockwise ? -90f : 90f;

        _endAngle =
            _startAngle + rotationAmount;

        BuildArc();

        SetPathVisible(false);
    }


    protected override void Update()
    {
        if (_isDead)
            return;

        float elapsed =
            (float)MusicManager.Instance.ScaledElapsedTime;

        // Normal approach toward the goal.
        if (elapsed < _holdStartTime)
        {
            UpdateApproach(elapsed);
            return;
        }

        // Start the hold once the arrow reaches the goal.
        if (!_holdStarted)
        {
            BeginHold();
        }

        // Hold is still active.
        if (elapsed <= _holdEndTime)
        {
            UpdateHold(elapsed);
            return;
        }

        // Reached the end successfully.
        CompleteHold();
    }


    private void UpdateApproach(float elapsed)
    {
        float t = Mathf.InverseLerp(
            _spawnTime,
            _arrivalTime,
            elapsed
        );

        Vector2 targetPosition =
            Vector2.Lerp(
                _startPos,
                _endPos,
                t
            );

        SmoothTranslate(targetPosition);
    }


    private void BeginHold()
    {
        _holdStarted = true;

        // Keep this object's root centered on the player/goal area
        // while the player follows the path.
        transform.position = Vector3.zero;

        SetPathVisible(true);

        _contactLostTime = -1f;
    }


    private void UpdateHold(float elapsed)
    {
        if (goalCollider == null || pathCollider == null)
            return;

        bool touching =
            goalCollider.IsTouching(pathCollider);

        if (touching)
        {
            _contactLostTime = -1f;
            return;
        }

        // Contact was just lost.
        if (_contactLostTime < 0f)
        {
            _contactLostTime = elapsed;
            return;
        }

        // Allow a tiny amount of forgiveness.
        if (elapsed - _contactLostTime >= contactGraceTime)
        {
            FailHold();
        }
    }


    private void CompleteHold()
    {
        if (_holdResolved)
            return;

        _holdResolved = true;

        // Require contact at the end as well.
        if (goalCollider != null &&
            pathCollider != null &&
            !goalCollider.IsTouching(pathCollider))
        {
            FailHold();
            return;
        }

        KillArrow(
            Goal.GoalType.Normal,
            playKillEffect: false
        );
    }


    private void FailHold()
    {
        if (_holdResolved || _isDead)
            return;

        _holdResolved = true;

        KillArrow(
            Goal.GoalType.Miss,
            playKillEffect: false
        );
    }


    private void BuildArc()
    {
        if (pathCollider == null)
            return;

        int segmentCount =
            Mathf.Max(2, arcSegments);

        Vector2[] points =
            new Vector2[segmentCount + 1];

        for (int i = 0; i <= segmentCount; i++)
        {
            float t =
                i / (float)segmentCount;

            float angle =
                Mathf.Lerp(
                    _startAngle,
                    _endAngle,
                    t
                );

            float radians =
                angle * Mathf.Deg2Rad;

            points[i] =
                new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                ) * arcRadius;
        }

        pathCollider.points = points;

        if (pathVisual != null)
        {
            pathVisual.positionCount =
                points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                pathVisual.SetPosition(
                    i,
                    points[i]
                );
            }
        }
    }


    private float DirectionToAngle(
        Vector2 direction)
    {
        return Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;
    }


    private void SetPathVisible(bool visible)
    {
        if (pathVisual != null)
            pathVisual.enabled = visible;

        if (pathCollider != null)
            pathCollider.enabled = visible;

        if (sRend != null)
            sRend.enabled = !visible;
    }


    // Important:
    // The initial collision should NOT kill this arrow like
    // a normal ArrowBase would.
    public override void OnArrowHit(
        float damage = 1f,
        Goal.GoalType goalType = Goal.GoalType.Normal,
        Vector2 hitDirection = default)
    {
        if (_isDead)
            return;

        // During the approach / initial hit, don't resolve the arrow.
        // Reaching _arrivalTime starts the hold instead.
        if (!_holdStarted)
            return;

        // Ignore ordinary arrow-hit calls while the hold is active.
        // RotationArrow resolves itself through CompleteHold/FailHold.
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawDebugPath)
            return;

        if (arcSegments < 2)
            return;

        float startAngle =
            Application.isPlaying
                ? _startAngle
                : 90f;

        float endAngle =
            Application.isPlaying
                ? _endAngle
                : clockwise
                    ? startAngle - 90f
                    : startAngle + 90f;

        Vector3 previous = transform.position + (Vector3)AngleToVector(startAngle) * arcRadius;

        for (int i = 1; i <= arcSegments; i++)
        {
            float t =
                i / (float)arcSegments;

            float angle =
                Mathf.Lerp(
                    startAngle,
                    endAngle,
                    t
                );

            Vector3 next =
                transform.position +
                (Vector3)(
                    AngleToVector(angle) *
                    arcRadius
                );

            Gizmos.DrawLine(
                previous,
                next
            );

            previous = next;
        }
    }


    private Vector2 AngleToVector(float angle)
    {
        float radians =
            angle * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        );
    }
#endif
}