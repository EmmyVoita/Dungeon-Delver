using DG.Tweening;
using UnityEngine;

public class ArcProjectile : MonoBehaviour, IReversible
{
    private Vector3 _startPos;
    private Vector3 _endPos;

    private float _duration;
    private float _height;
    private float _arcYOffset;

    private float _time;
    private bool _initialized = false;

    private Tween currentTween;

    [SerializeField] private float _reverseSpeedMult = 2f;
    private float _speedMultiplier = 1f;
    private float _direction = 1f;

    // =========================
    // Rotation Settings
    // =========================
    [Header("Rotation")]
    [SerializeField] private bool rotate = false;
    [SerializeField] private float rotationSpeed = 360f; // degrees per second
    [SerializeField] private bool randomizeStartRotation = true;

    public void Initialize(Vector3 start, Vector3 end, float duration, float height, float arcYOffset)
    {
        _startPos = start;
        _endPos = end;
        _duration = duration;
        _height = height;
        _arcYOffset = arcYOffset;

        transform.position = start;
        _time = 0f;
        _initialized = true;

        // 🔥 Optional random start rotation
        if (rotate && randomizeStartRotation)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }
    }

    public void Reverse()
    {
        // kill any existing tween
        currentTween?.Kill();

        currentTween = DOTween.Sequence()

        // 🔹 Slow down to 0
        .Append(DOTween.To(
            () => _speedMultiplier,
            x => _speedMultiplier = x,
            0f,
            0.2f
        ).SetEase(Ease.OutQuad))

        // 🔹 Flip direction + rotation
        .AppendCallback(() =>
        {
            _direction *= -1f;

            if (rotate)
                rotationSpeed *= -1f; // 🔥 reverse spin direction
        })

        // 🔹 Speed back up
        .Append(DOTween.To(
            () => _speedMultiplier,
            x => _speedMultiplier = x,
            _reverseSpeedMult,
            0.3f
        ).SetEase(Ease.InQuad));
    }

    void Update()
    {
        if (!_initialized) return;

        _time += Time.deltaTime * _direction * _speedMultiplier;

        float t = Mathf.Clamp01(_time / _duration);

        // Horizontal lerp
        Vector3 pos = Vector3.Lerp(_startPos, _endPos, t);

        // Arc (parabola)
        float arc = 4f * _height * (t - t * t);
        arc -= _arcYOffset;

        pos.y += arc;

        transform.position = pos;

        // 🔥 Optional rotation
        if (rotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        // Optional cleanup if needed later
        /*
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
        */
    }
}