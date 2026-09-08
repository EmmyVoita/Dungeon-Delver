using UnityEngine;

public class PlayerFacingPosition : MonoBehaviour
{
    [SerializeField] private Transform visual;

    [Header("Position")]
    [SerializeField] private bool moveToOuterEdge = false;

    [Tooltip("Small movement when using a separate goal.")]
    [SerializeField] private float nudgeDistance = 0.2f;

    [Tooltip("Distance from center when the player acts as the goal.")]
    [SerializeField] private float outerEdgeDistance = 0.75f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 15f;

    private Vector3 _centerLocalPosition;
    private bool _lockToCenter;

    private void OnEnable()
    {
        Player.OnControlStateChanged += HandlePlayerControlStateChanged;
    }

    private void OnDisable()
    {
        Player.OnControlStateChanged -= HandlePlayerControlStateChanged;
    }

    private void HandlePlayerControlStateChanged(PlayerControlState controlState)
    {
        if(controlState != PlayerControlState.Normal)
        {
            _lockToCenter = true;
        }
        else
        {
            _lockToCenter = false;
        }
    }

    private void Awake()
    {
        if (visual != null)
            _centerLocalPosition = visual.localPosition;
    }

    private void Update()
    {
        if (Player.Instance == null || visual == null)
            return;

        Vector2 direction = Player.Instance.LastFacingDirection;

        float distance = moveToOuterEdge
            ? outerEdgeDistance
            : nudgeDistance;

        Vector3 targetPosition;

        if(_lockToCenter)
        {
            targetPosition = _centerLocalPosition;
        }
        else
        {
            targetPosition = _centerLocalPosition + (Vector3)(direction * distance);
        }

        visual.localPosition = Vector3.Lerp(
            visual.localPosition,
            targetPosition,
            1f - Mathf.Exp(-moveSpeed * Time.deltaTime)
        );
    }
}