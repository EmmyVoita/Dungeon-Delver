using UnityEngine;

public class ExpandingRingOrb : MonoBehaviour, IReversible
{
    private Vector2 _direction;
    private float _speed;
    private float _currentSpeed;
    private float _directionSign = 1f;

    private bool _initialized;

    public void Initialize(Vector2 dir, float speed)
    {
        _direction = dir.normalized;
        _speed = speed;
        _currentSpeed = speed;
        _initialized = true;
    }

    void Update()
    {
        if (!_initialized) return;

        transform.position += (Vector3)(_direction * _currentSpeed * _directionSign * Time.deltaTime);
    }

    public void Reverse()
    {
        _directionSign *= -1f;
    }
}